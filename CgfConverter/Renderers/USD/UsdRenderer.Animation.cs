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

        if (_cryData.Animations is null || _cryData.Animations.Count == 0)
        {
            Log.D("No animations found in CryEngine data");
            return animations;
        }

        int maxEndFrame = 0;

        // Process each animation chunk (typically from .dba files)
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
                        Log.D($"Created animation: {name} ({endFrame} frames)");
                    }
                }
            }
        }

        // Set header timeline info if we have animations
        if (animations.Count > 0)
        {
            header.TimeCodesPerSecond = 30;
            header.StartTimeCode = 0;
            header.EndTimeCode = maxEndFrame;
        }

        Log.D($"Created {animations.Count} animation(s)");
        return animations;
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
        if (_cryData.Animations is null || _cryData.Animations.Count == 0)
            return 0;

        int exportedCount = 0;
        var baseFileName = Path.GetFileNameWithoutExtension(usdOutputFile.FullName);
        var outputDir = usdOutputFile.DirectoryName ?? ".";

        // Collect all animations with their metadata
        var allAnimations = new List<(string name, UsdSkelAnimation skelAnim, int endFrame)>();

        foreach (var animModel in _cryData.Animations)
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
