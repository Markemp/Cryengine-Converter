using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Renderers.USD.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CgfConverter.Renderers.USD;

/// <summary>
/// UsdRenderer partial class - Skeletal animation support
/// </summary>
public partial class UsdRenderer
{
    /// <summary>
    /// Creates animation prims from CryEngine animation data.
    /// Returns a list of SkelAnimation prims to be added to the scene.
    /// Also updates the USD header with animation timeline info.
    /// </summary>
    private List<UsdPrim> CreateAnimations(Dictionary<uint, string> controllerIdToJointPath, UsdHeader header)
    {
        var animations = new List<UsdPrim>();
        int maxEndFrame = 0;

        // Process DBA animations (ChunkController_905)
        if (_cryData.Animations is not null && _cryData.Animations.Count > 0)
        {
            foreach (var animModel in _cryData.Animations)
            {
                var animChunks = animModel.ChunkMap.Values.OfType<ChunkController_905>().ToList();

                foreach (var animChunk in animChunks)
                {
                    if (animChunk.Animations is null)
                        continue;

                    // Get stripped names for cleaner animation names
                    var names = StripCommonParentPaths(
                        animChunk.Animations.Select(x => Path.ChangeExtension(x.Name, null)).ToList());

                    foreach (var (anim, name) in animChunk.Animations.Zip(names))
                    {
                        var (skelAnim, endFrame) = CreateSkelAnimation(anim, animChunk, controllerIdToJointPath, name);
                        if (skelAnim is not null)
                        {
                            animations.Add(skelAnim);
                            maxEndFrame = Math.Max(maxEndFrame, endFrame);
                            Log.D($"Created DBA animation: {name} ({endFrame} frames)");
                        }
                    }
                }
            }
        }

        // Process CAF animations
        if (_cryData.CafAnimations is not null && _cryData.CafAnimations.Count > 0)
        {
            foreach (var cafAnim in _cryData.CafAnimations)
            {
                var (skelAnim, endFrame) = CreateSkelAnimationFromCaf(cafAnim, controllerIdToJointPath);
                if (skelAnim is not null)
                {
                    animations.Add(skelAnim);
                    maxEndFrame = Math.Max(maxEndFrame, endFrame);
                    Log.D($"Created CAF animation: {cafAnim.Name} ({endFrame} frames)");
                }
            }
        }

        if (animations.Count == 0)
        {
            Log.D("No animations found in CryEngine data");
            return animations;
        }

        // Set header timeline info if we have animations
        header.TimeCodesPerSecond = 30;
        header.StartTimeCode = 0;
        header.EndTimeCode = maxEndFrame;

        Log.D($"Created {animations.Count} animation(s)");
        return animations;
    }

    /// <summary>
    /// Creates a SkelAnimation prim from a CafAnimation.
    /// </summary>
    private (UsdSkelAnimation? skelAnim, int endFrame) CreateSkelAnimationFromCaf(
        CafAnimation cafAnim,
        Dictionary<uint, string> controllerIdToJointPath)
    {
        // Build rest translation mapping for bones without position animation
        var jointPathToRestTranslation = BuildRestTranslationMapping();

        // Map controller IDs to joint paths
        var animatedJoints = new List<string>();
        var tracksByJointPath = new Dictionary<string, BoneTrack>();

        foreach (var (controllerId, track) in cafAnim.BoneTracks)
        {
            // Try to find joint path by controller ID
            if (!controllerIdToJointPath.TryGetValue(controllerId, out var jointPath))
            {
                // Try using bone name from CAF's bone name list
                if (cafAnim.ControllerIdToBoneName.TryGetValue(controllerId, out var boneName))
                {
                    // Search for matching joint path by bone name
                    jointPath = controllerIdToJointPath.Values
                        .FirstOrDefault(jp => jp.EndsWith("/" + boneName) || jp == boneName);
                }

                if (jointPath is null)
                {
                    Log.D($"CAF[{cafAnim.Name}]: Controller 0x{controllerId:X08} not found in skeleton");
                    continue;
                }
            }

            animatedJoints.Add(jointPath);
            tracksByJointPath[jointPath] = track;
        }

        if (animatedJoints.Count == 0)
        {
            Log.W($"CAF[{cafAnim.Name}]: No valid bone tracks found");
            return (null, 0);
        }

        // Calculate frame range
        int startFrame = cafAnim.StartFrame;
        int endFrame = cafAnim.EndFrame;

        // If timing chunk didn't have valid range, calculate from keyframes
        if (endFrame <= startFrame)
        {
            foreach (var track in tracksByJointPath.Values)
            {
                if (track.KeyTimes.Count > 0)
                {
                    endFrame = Math.Max(endFrame, (int)track.KeyTimes.Max());
                }
            }
        }

        // Normalize frame range
        int durationFrames = endFrame - startFrame;
        if (durationFrames <= 0)
            durationFrames = 1;

        // Collect all unique frame numbers
        var allFrames = new SortedSet<int>();
        foreach (var track in tracksByJointPath.Values)
        {
            foreach (var t in track.KeyTimes)
            {
                int frame = (int)t - startFrame;
                if (frame >= 0 && frame <= durationFrames)
                    allFrames.Add(frame);
            }
        }

        // Ensure we have at least frame 0 and end frame
        allFrames.Add(0);
        allFrames.Add(durationFrames);

        // Build time-sampled arrays
        var translationSamples = new SortedDictionary<float, List<Vector3>>();
        var rotationSamples = new SortedDictionary<float, List<Quaternion>>();
        var scaleSamples = new SortedDictionary<float, List<Vector3>>();

        foreach (var frame in allFrames)
        {
            var translations = new List<Vector3>();
            var rotations = new List<Quaternion>();
            var scales = new List<Vector3>();

            foreach (var jointPath in animatedJoints)
            {
                var track = tracksByJointPath[jointPath];

                // Get rest translation for this joint (used when no position animation)
                var restTranslation = jointPathToRestTranslation.TryGetValue(jointPath, out var rest)
                    ? rest
                    : Vector3.Zero;

                // Sample position at this frame, using rest translation for bones without position animation
                Vector3 position = SampleCafPosition(track, frame + startFrame, restTranslation);
                translations.Add(position);

                // Sample rotation at this frame
                Quaternion rotation = SampleCafRotation(track, frame + startFrame);
                rotations.Add(rotation);

                // CAF animations don't typically have scale
                scales.Add(Vector3.One);
            }

            translationSamples[frame] = translations;
            rotationSamples[frame] = rotations;
            scaleSamples[frame] = scales;
        }

        // Create the SkelAnimation prim
        var cleanName = CleanPathString(cafAnim.Name);
        var skelAnim = new UsdSkelAnimation(cleanName);

        skelAnim.Attributes.Add(new UsdTokenArray("joints", animatedJoints, isUniform: true));
        skelAnim.Attributes.Add(new UsdTimeSampledFloat3Array("translations", translationSamples));
        skelAnim.Attributes.Add(new UsdTimeSampledQuatfArray("rotations", rotationSamples));
        skelAnim.Attributes.Add(new UsdTimeSampledHalf3Array("scales", scaleSamples));

        return (skelAnim, durationFrames);
    }

    /// <summary>
    /// Builds a mapping from joint path to rest pose translation.
    /// Used for bones without position animation to maintain their skeleton structure.
    /// </summary>
    private Dictionary<string, Vector3> BuildRestTranslationMapping()
    {
        var mapping = new Dictionary<string, Vector3>();

        if (_bonePathMap is null)
            return mapping;

        foreach (var (bone, jointPath) in _bonePathMap)
        {
            // Compute rest translation: local position of this bone relative to parent
            Matrix4x4 restMatrix;

            if (bone.ParentBone == null)
            {
                // Root bone: local = world, invert BindPoseMatrix to get boneToWorld
                if (Matrix4x4.Invert(bone.BindPoseMatrix, out var boneToWorld))
                {
                    restMatrix = boneToWorld;
                }
                else
                {
                    restMatrix = Matrix4x4.Identity;
                }
            }
            else
            {
                // Child bone: compute local transform relative to parent
                // localTransform = parentWorldToBone * childBoneToWorld
                if (Matrix4x4.Invert(bone.BindPoseMatrix, out var childBoneToWorld))
                {
                    restMatrix = bone.ParentBone.BindPoseMatrix * childBoneToWorld;
                }
                else
                {
                    restMatrix = Matrix4x4.Identity;
                }
            }

            // Extract translation from the rest matrix
            // CryEngine matrices are column-major with translation in column 4 (M14, M24, M34)
            // (GetRestTransforms transposes this to M41, M42, M43 for USD output)
            var translation = new Vector3(restMatrix.M14, restMatrix.M24, restMatrix.M34);
            mapping[jointPath] = translation;
        }

        return mapping;
    }

    /// <summary>
    /// Samples position from a CAF bone track at a given frame.
    /// If no position keyframes exist, uses the rest translation to maintain skeleton structure.
    /// </summary>
    /// <param name="track">The bone animation track.</param>
    /// <param name="frame">The frame number to sample.</param>
    /// <param name="restTranslation">The rest pose translation to use when no position animation exists.</param>
    private static Vector3 SampleCafPosition(BoneTrack track, float frame, Vector3 restTranslation)
    {
        // If no position animation, use rest pose translation to maintain skeleton structure
        if (track.Positions.Count == 0)
            return restTranslation;

        if (track.Positions.Count == 1 || track.KeyTimes.Count == 0)
            return track.Positions[0];

        // Find surrounding keyframes
        int i = 0;
        while (i < track.KeyTimes.Count - 1 && track.KeyTimes[i + 1] <= frame)
            i++;

        if (i >= track.Positions.Count)
            return track.Positions[^1];

        if (i == track.KeyTimes.Count - 1 || track.KeyTimes[i] >= frame)
            return i < track.Positions.Count ? track.Positions[i] : track.Positions[^1];

        // Linear interpolation
        float t0 = track.KeyTimes[i];
        float t1 = track.KeyTimes[i + 1];
        float alpha = (frame - t0) / (t1 - t0);

        int i1 = Math.Min(i + 1, track.Positions.Count - 1);
        return Vector3.Lerp(track.Positions[i], track.Positions[i1], alpha);
    }

    /// <summary>
    /// Samples rotation from a CAF bone track at a given frame.
    /// </summary>
    private static Quaternion SampleCafRotation(BoneTrack track, float frame)
    {
        if (track.Rotations.Count == 0)
            return Quaternion.Identity;

        if (track.Rotations.Count == 1 || track.KeyTimes.Count == 0)
            return track.Rotations[0];

        // Find surrounding keyframes
        int i = 0;
        while (i < track.KeyTimes.Count - 1 && track.KeyTimes[i + 1] <= frame)
            i++;

        if (i >= track.Rotations.Count)
            return track.Rotations[^1];

        if (i == track.KeyTimes.Count - 1 || track.KeyTimes[i] >= frame)
            return i < track.Rotations.Count ? track.Rotations[i] : track.Rotations[^1];

        // Spherical linear interpolation
        float t0 = track.KeyTimes[i];
        float t1 = track.KeyTimes[i + 1];
        float alpha = (frame - t0) / (t1 - t0);

        int i1 = Math.Min(i + 1, track.Rotations.Count - 1);
        return Quaternion.Slerp(track.Rotations[i], track.Rotations[i1], alpha);
    }

    /// <summary>
    /// Creates a single SkelAnimation prim from a CryEngine Animation struct.
    /// Returns the animation prim and the end frame number.
    /// </summary>
    private (UsdSkelAnimation? skelAnim, int endFrame) CreateSkelAnimation(
        ChunkController_905.Animation anim,
        ChunkController_905 animChunk,
        Dictionary<uint, string> controllerIdToJointPath,
        string animName)
    {
        // Collect all joint paths that have animation in this clip
        var animatedJoints = new List<string>();
        var controllersByJointPath = new Dictionary<string, ChunkController_905.CControllerInfo>();

        foreach (var controller in anim.Controllers)
        {
            if (!controllerIdToJointPath.TryGetValue(controller.ControllerID, out var jointPath))
            {
                Log.D($"Animation[{animName}]: Controller 0x{controller.ControllerID:X08} not found in skeleton");
                continue;
            }

            animatedJoints.Add(jointPath);
            controllersByJointPath[jointPath] = controller;
        }

        if (animatedJoints.Count == 0)
        {
            Log.W($"Animation[{animName}]: No valid controllers found");
            return (null, 0);
        }

        // Collect all unique frame numbers across all tracks
        // CryEngine keyTimes are already in frames (at 30fps)
        var allFrames = new SortedSet<int>();
        foreach (var controller in anim.Controllers)
        {
            if (controller.HasPosTrack && animChunk.KeyTimes is not null)
            {
                var times = animChunk.KeyTimes[controller.PosKeyTimeTrack];
                var startTime = times.Count > 0 ? (int)times[0] : 0;
                foreach (var t in times)
                    allFrames.Add((int)t - startTime);
            }
            if (controller.HasRotTrack && animChunk.KeyTimes is not null)
            {
                var times = animChunk.KeyTimes[controller.RotKeyTimeTrack];
                var startTime = times.Count > 0 ? (int)times[0] : 0;
                foreach (var t in times)
                    allFrames.Add((int)t - startTime);
            }
        }

        if (allFrames.Count == 0)
        {
            Log.W($"Animation[{animName}]: No keyframes found");
            return (null, 0);
        }

        int endFrame = allFrames.Max;

        // Build time-sampled arrays using frame numbers
        // USD expects all joints' values at each time sample
        // USD SkelAnimation requires translations, rotations, AND scales to be valid
        var translationSamples = new SortedDictionary<float, List<Vector3>>();
        var rotationSamples = new SortedDictionary<float, List<Quaternion>>();
        var scaleSamples = new SortedDictionary<float, List<Vector3>>();

        foreach (var frame in allFrames)
        {
            var translations = new List<Vector3>();
            var rotations = new List<Quaternion>();
            var scales = new List<Vector3>();

            foreach (var jointPath in animatedJoints)
            {
                var controller = controllersByJointPath[jointPath];

                // Get translation for this joint at this frame
                Vector3 translation = Vector3.Zero;
                if (controller.HasPosTrack && animChunk.KeyTimes is not null && animChunk.KeyPositions is not null)
                {
                    translation = SamplePositionAtFrame(
                        animChunk.KeyTimes[controller.PosKeyTimeTrack],
                        animChunk.KeyPositions[controller.PosTrack],
                        frame);
                }
                translations.Add(translation);

                // Get rotation for this joint at this frame
                Quaternion rotation = Quaternion.Identity;
                if (controller.HasRotTrack && animChunk.KeyTimes is not null && animChunk.KeyRotations is not null)
                {
                    rotation = SampleRotationAtFrame(
                        animChunk.KeyTimes[controller.RotKeyTimeTrack],
                        animChunk.KeyRotations[controller.RotTrack],
                        frame);
                }
                rotations.Add(rotation);

                // CryEngine animations typically don't have scale, use uniform scale
                scales.Add(Vector3.One);
            }

            translationSamples[frame] = translations;
            rotationSamples[frame] = rotations;
            scaleSamples[frame] = scales;
        }

        // Create the SkelAnimation prim
        var cleanName = CleanPathString(animName);
        var skelAnim = new UsdSkelAnimation(cleanName);

        // Add joints array (token[])
        skelAnim.Attributes.Add(new UsdTokenArray("joints", animatedJoints, isUniform: true));

        // Add time-sampled translations
        skelAnim.Attributes.Add(new UsdTimeSampledFloat3Array("translations", translationSamples));

        // Add time-sampled rotations
        skelAnim.Attributes.Add(new UsdTimeSampledQuatfArray("rotations", rotationSamples));

        // Add time-sampled scales (required by USD SkelAnimation spec, must be half3[])
        skelAnim.Attributes.Add(new UsdTimeSampledHalf3Array("scales", scaleSamples));

        return (skelAnim, endFrame);
    }

    /// <summary>
    /// Samples position at a given frame using linear interpolation.
    /// </summary>
    private Vector3 SamplePositionAtFrame(List<float> keyTimes, List<Vector3> keyPositions, int frame)
    {
        if (keyTimes.Count == 0 || keyPositions.Count == 0)
            return Vector3.Zero;

        // Normalize times to frame numbers relative to start
        float startTime = keyTimes[0];
        var normalizedFrames = keyTimes.Select(t => (int)(t - startTime)).ToList();

        // Find surrounding keyframes
        int i = 0;
        while (i < normalizedFrames.Count - 1 && normalizedFrames[i + 1] <= frame)
            i++;

        if (i >= keyPositions.Count)
            return keyPositions[^1];

        if (i == normalizedFrames.Count - 1 || normalizedFrames[i] >= frame)
            return keyPositions[i];

        // Linear interpolation
        int f0 = normalizedFrames[i];
        int f1 = normalizedFrames[i + 1];
        float alpha = (float)(frame - f0) / (f1 - f0);

        return Vector3.Lerp(keyPositions[i], keyPositions[Math.Min(i + 1, keyPositions.Count - 1)], alpha);
    }

    /// <summary>
    /// Samples rotation at a given frame using spherical linear interpolation.
    /// </summary>
    private Quaternion SampleRotationAtFrame(List<float> keyTimes, List<Quaternion> keyRotations, int frame)
    {
        if (keyTimes.Count == 0 || keyRotations.Count == 0)
            return Quaternion.Identity;

        // Normalize times to frame numbers relative to start
        float startTime = keyTimes[0];
        var normalizedFrames = keyTimes.Select(t => (int)(t - startTime)).ToList();

        // Find surrounding keyframes
        int i = 0;
        while (i < normalizedFrames.Count - 1 && normalizedFrames[i + 1] <= frame)
            i++;

        if (i >= keyRotations.Count)
            return keyRotations[^1];

        if (i == normalizedFrames.Count - 1 || normalizedFrames[i] >= frame)
            return keyRotations[i];

        // Spherical linear interpolation
        int f0 = normalizedFrames[i];
        int f1 = normalizedFrames[i + 1];
        float alpha = (float)(frame - f0) / (f1 - f0);

        return Quaternion.Slerp(keyRotations[i], keyRotations[Math.Min(i + 1, keyRotations.Count - 1)], alpha);
    }

    /// <summary>
    /// Strips common parent paths from animation names for cleaner display.
    /// Based on GltfRendererUtilities.StripCommonParentPaths logic.
    /// </summary>
    private static List<string> StripCommonParentPaths(List<string> paths)
    {
        if (paths.Count == 0)
            return paths;

        // Split all paths into components
        var splitPaths = paths.Select(p => p.Replace('\\', '/').Split('/').ToList()).ToList();

        // Find common prefix length
        int commonPrefixLen = 0;
        bool done = false;
        while (!done)
        {
            string? commonPart = null;
            foreach (var parts in splitPaths)
            {
                if (commonPrefixLen >= parts.Count)
                {
                    done = true;
                    break;
                }

                if (commonPart is null)
                    commonPart = parts[commonPrefixLen];
                else if (parts[commonPrefixLen] != commonPart)
                {
                    done = true;
                    break;
                }
            }
            if (!done)
                commonPrefixLen++;
        }

        // Return paths with common prefix stripped
        return splitPaths
            .Select(parts => string.Join("/", parts.Skip(commonPrefixLen)))
            .ToList();
    }

    /// <summary>
    /// Exports individual animation files for Blender NLA workflow.
    /// Blender's USD importer only reads the single bound animation, so we export
    /// each animation as a separate file with skeleton + that animation bound.
    /// </summary>
    /// <param name="controllerIdToJointPath">Mapping from bone controller IDs to USD joint paths.</param>
    /// <param name="jointPaths">Ordered list of joint paths for skeleton.</param>
    /// <param name="bonePathMap">Mapping from CompiledBone to joint path.</param>
    /// <returns>Number of animation files exported.</returns>
    private int ExportAnimationFiles(
        Dictionary<uint, string> controllerIdToJointPath,
        List<string> jointPaths,
        Dictionary<CompiledBone, string> bonePathMap)
    {
        // Check if we have any animations (DBA or CAF)
        bool hasDbaAnimations = _cryData.Animations is not null && _cryData.Animations.Count > 0;
        bool hasCafAnimations = _cryData.CafAnimations is not null && _cryData.CafAnimations.Count > 0;

        if (!hasDbaAnimations && !hasCafAnimations)
            return 0;

        int exportedCount = 0;
        var baseFileName = Path.GetFileNameWithoutExtension(usdOutputFile.FullName);
        var outputDir = usdOutputFile.DirectoryName ?? ".";

        // Collect all animations with their metadata
        var allAnimations = new List<(string name, UsdSkelAnimation skelAnim, int endFrame)>();

        // Process DBA animations
        if (hasDbaAnimations)
        {
            foreach (var animModel in _cryData.Animations!)
            {
                var animChunks = animModel.ChunkMap.Values.OfType<ChunkController_905>().ToList();

                foreach (var animChunk in animChunks)
                {
                    if (animChunk.Animations is null)
                        continue;

                    var names = StripCommonParentPaths(
                        animChunk.Animations.Select(x => Path.ChangeExtension(x.Name, null)).ToList());

                    foreach (var (anim, name) in animChunk.Animations.Zip(names))
                    {
                        var (skelAnim, endFrame) = CreateSkelAnimation(anim, animChunk, controllerIdToJointPath, name);
                        if (skelAnim is not null)
                        {
                            allAnimations.Add((name, skelAnim, endFrame));
                        }
                    }
                }
            }
        }

        // Process CAF animations
        if (hasCafAnimations)
        {
            foreach (var cafAnim in _cryData.CafAnimations!)
            {
                var (skelAnim, endFrame) = CreateSkelAnimationFromCaf(cafAnim, controllerIdToJointPath);
                if (skelAnim is not null)
                {
                    allAnimations.Add((cafAnim.Name, skelAnim, endFrame));
                }
            }
        }

        // Skip if only one animation (already in main file)
        if (allAnimations.Count <= 1)
        {
            Log.D("Only one animation found, skipping separate animation file export");
            return 0;
        }

        Log.D($"Exporting {allAnimations.Count} animation files for Blender NLA workflow...");

        // Export each animation as a separate file
        foreach (var (animName, skelAnim, endFrame) in allAnimations)
        {
            var cleanAnimName = CleanPathString(animName);
            var animFileName = $"{baseFileName}_anim_{cleanAnimName}.usda";
            var animFilePath = Path.Combine(outputDir, animFileName);

            var animDoc = CreateAnimationOnlyDoc(skelAnim, endFrame, jointPaths, bonePathMap);

            using (var writer = new StreamWriter(animFilePath))
            {
                usdSerializer.Serialize(animDoc, writer);
            }

            Log.D($"  Exported: {animFileName}");
            exportedCount++;
        }

        return exportedCount;
    }

    /// <summary>
    /// Creates a USD document containing only skeleton + single animation.
    /// This is for Blender import workflow where each animation needs its own file.
    /// </summary>
    private UsdDoc CreateAnimationOnlyDoc(
        UsdSkelAnimation skelAnim,
        int endFrame,
        List<string> jointPaths,
        Dictionary<CompiledBone, string> bonePathMap)
    {
        var usdDoc = new UsdDoc
        {
            Header = new UsdHeader
            {
                TimeCodesPerSecond = 30,
                StartTimeCode = 0,
                EndTimeCode = endFrame
            }
        };

        // Create root
        usdDoc.Prims.Add(new UsdXform("root", "/"));
        var rootPrim = usdDoc.Prims[0];

        // Create skeleton structure (same as main export but without mesh)
        var skelRoot = new UsdSkelRoot("Armature");
        var skeleton = new UsdSkeleton("Skeleton");

        // Add joint names array
        skeleton.Attributes.Add(new UsdTokenArray("joints", jointPaths, isUniform: true));

        // Add bind transforms
        var bindTransforms = GetBindTransforms(jointPaths, bonePathMap);
        skeleton.Attributes.Add(new UsdMatrix4dArray("bindTransforms", bindTransforms, isUniform: true));

        // Add rest transforms
        var restTransforms = GetRestTransforms(jointPaths, bonePathMap);
        skeleton.Attributes.Add(new UsdMatrix4dArray("restTransforms", restTransforms, isUniform: true));

        // Bind this animation to the skeleton
        var animPath = $"</root/Armature/{skelAnim.Name}>";
        skeleton.Attributes.Add(new UsdRelationship("skel:animationSource", animPath));

        skelRoot.Children.Add(skeleton);
        skelRoot.Children.Add(skelAnim);
        rootPrim.Children.Add(skelRoot);

        return usdDoc;
    }
}
