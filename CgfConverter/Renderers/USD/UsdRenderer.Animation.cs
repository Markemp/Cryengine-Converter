using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Models.Structs;
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

        // Process DBA animations (ChunkController_905 for non-Ivo, ChunkIvoDBAData for Ivo)
        if (_cryData.Animations is not null && _cryData.Animations.Count > 0)
        {
            foreach (var animModel in _cryData.Animations)
            {
                // Try standard DBA format first (ChunkController_905)
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

                // Try Ivo DBA format (ChunkIvoDBAData + ChunkIvoDBAMetadata)
                var ivoDbaData = animModel.ChunkMap.Values.OfType<ChunkIvoDBAData>().FirstOrDefault();
                var ivoDbaMetadata = animModel.ChunkMap.Values.OfType<ChunkIvoDBAMetadata>().FirstOrDefault();

                if (ivoDbaData is not null && ivoDbaData.AnimationBlocks.Count > 0)
                {
                    Log.D($"Found Ivo DBA with {ivoDbaData.AnimationBlocks.Count} animation blocks");

                    // Get animation names from metadata if available
                    var animNames = ivoDbaMetadata?.AnimPaths ?? [];
                    var animEntries = ivoDbaMetadata?.Entries ?? [];

                    // Log animation names from metadata
                    if (animNames.Count > 0)
                    {
                        Log.D($"Ivo DBA metadata has {animNames.Count} animation paths:");
                        for (int j = 0; j < animNames.Count; j++)
                        {
                            Log.D($"  [{j}] {animNames[j]}");
                        }
                    }
                    else
                        Log.D("Ivo DBA has no metadata animation paths");

                    int ivoSuccessCount = 0;
                    int ivoFailCount = 0;

                    for (int i = 0; i < ivoDbaData.AnimationBlocks.Count; i++)
                    {
                        var block = ivoDbaData.AnimationBlocks[i];
                        var animName = i < animNames.Count
                            ? Path.GetFileNameWithoutExtension(animNames[i])
                            : $"animation_{i}";
                        var metaEntry = i < animEntries.Count ? animEntries[i] : (IvoDBAMetaEntry?)null;

                        Log.D($"Processing Ivo DBA animation block {i}: '{animName}'");

                        var (skelAnim, endFrame) = CreateSkelAnimationFromIvoDBA(
                            block, animName, metaEntry, controllerIdToJointPath);

                        if (skelAnim is not null)
                        {
                            animations.Add(skelAnim);
                            maxEndFrame = Math.Max(maxEndFrame, endFrame);
                            Log.D($"SUCCESS: Created Ivo DBA animation: {animName} ({endFrame} frames)");
                            ivoSuccessCount++;
                        }
                        else
                        {
                            Log.D($"FAILED: Could not create Ivo DBA animation: {animName}");
                            ivoFailCount++;
                        }
                    }

                    Log.D($"Ivo DBA summary: {ivoSuccessCount} succeeded, {ivoFailCount} failed out of {ivoDbaData.AnimationBlocks.Count} blocks");
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

        // Set header timeline info if we have animations.
        // Both timeCodesPerSecond AND framesPerSecond must be set: USD treats them as
        // distinct (timeCodes are logical units; framesPerSecond is the playback hint).
        // Blender's USD importer reads framesPerSecond for the scene FPS — when missing,
        // it falls back to 24fps and the imported frame range can collapse to 0.
        header.TimeCodesPerSecond = 30;
        header.FramesPerSecond = 30;
        header.StartTimeCode = 0;
        header.EndTimeCode = maxEndFrame;

        Log.D($"Created {animations.Count} animation(s)");
        return animations;
    }

    /// <summary>
    /// Creates a SkelAnimation prim from a CafAnimation.
    /// Handles both absolute and additive animations - additive animations are converted
    /// to absolute by applying deltas to the rest pose.
    /// </summary>
    private (UsdSkelAnimation? skelAnim, int endFrame) CreateSkelAnimationFromCaf(
        CafAnimation cafAnim,
        Dictionary<uint, string> controllerIdToJointPath)
    {
        // Build rest pose mappings
        var jointPathToRestTranslation = BuildRestTranslationMapping();
        var jointPathToRestRotation = BuildRestRotationMapping();

        Log.D($"CAF[{cafAnim.Name}]: Rest translation mapping has {jointPathToRestTranslation.Count} entries");
        if (jointPathToRestTranslation.Count > 0)
        {
            var first = jointPathToRestTranslation.First();
            Log.D($"CAF[{cafAnim.Name}]: First rest translation: '{first.Key}' = {first.Value}");
        }

        bool isAdditive = cafAnim.IsAdditive;
        if (isAdditive)
            Log.D($"CAF[{cafAnim.Name}]: Converting additive animation to absolute");

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

        // Debug: Check how many animated joints have rest translations
        int foundRestCount = animatedJoints.Count(jp => jointPathToRestTranslation.ContainsKey(jp));
        Log.D($"CAF[{cafAnim.Name}]: {animatedJoints.Count} animated joints, {foundRestCount} have rest translations");
        if (animatedJoints.Count > 0 && foundRestCount == 0)
        {
            Log.W($"CAF[{cafAnim.Name}]: No rest translations found for animated joints!");
            Log.D($"CAF[{cafAnim.Name}]: First animated joint: '{animatedJoints[0]}'");
            if (jointPathToRestTranslation.Count > 0)
                Log.D($"CAF[{cafAnim.Name}]: First rest key: '{jointPathToRestTranslation.Keys.First()}'");
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

                // Get rest pose for this joint
                bool foundRest = jointPathToRestTranslation.TryGetValue(jointPath, out var restT);
                var restTranslation = foundRest ? restT : Vector3.Zero;
                var restRotation = jointPathToRestRotation.TryGetValue(jointPath, out var restR)
                    ? restR
                    : Quaternion.Identity;

                // Sample position and rotation at this frame
                // CAF positions are absolute local transforms, not deltas from rest pose
                // For bones WITH position animation: use animation data directly
                // For bones WITHOUT position animation: SampleCafPosition returns restTranslation
                Vector3 position = SampleCafPosition(track, frame + startFrame, restTranslation);
                Quaternion rotation = SampleCafRotation(track, frame + startFrame);

                // If no rotation keys exist, SampleCafRotation returns Identity.
                // USD SkelAnimation replaces restTransform, so we must use rest rotation
                // to preserve bone orientation for non-animated bones.
                if (track.Rotations.Count == 0)
                    rotation = restRotation;

                if (isAdditive)
                {
                    // Additive animations: rotation is also a delta from rest
                    rotation = restRotation * rotation;
                }

                translations.Add(position);
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
    /// Creates a SkelAnimation prim from an Ivo DBA animation block.
    /// Ivo DBA uses bone hashes (CRC32) instead of controller IDs.
    ///
    /// Ivo animations may store values as deltas from a reference pose (StartPosition/StartRotation
    /// in metadata), or as absolute values. The metadata's StartPosition/StartRotation appears to
    /// be for the root bone reference.
    /// </summary>
    private (UsdSkelAnimation? skelAnim, int endFrame) CreateSkelAnimationFromIvoDBA(
        IvoAnimationBlock block,
        string animName,
        IvoDBAMetaEntry? metaEntry,
        Dictionary<uint, string> controllerIdToJointPath)
    {
        // Get reference pose from metadata (for root bone) and skeleton rest pose (for other bones)
        var refPosition = metaEntry?.StartPosition ?? Vector3.Zero;
        var refRotation = metaEntry?.StartRotation ?? Quaternion.Identity;

        Log.D($"IvoDBA[{animName}]: Reference pose from metadata: pos=({refPosition.X:F3}, {refPosition.Y:F3}, {refPosition.Z:F3}), rot=({refRotation.X:F3}, {refRotation.Y:F3}, {refRotation.Z:F3}, {refRotation.W:F3})");

        // Build rest pose mappings for bones without animation
        var jointPathToRestTranslation = BuildRestTranslationMapping();
        var jointPathToRestRotation = BuildRestRotationMapping();

        // Map bone hashes to joint paths
        var animatedJoints = new List<string>();
        var tracksByJointPath = new Dictionary<string, (List<Quaternion>? rotations, List<float>? rotTimes,
            List<Vector3>? positions, List<float>? posTimes, ushort posFormat)>();

        int matchedBones = 0;
        int unmatchedBones = 0;
        int bonesWithData = 0;
        var unmatchedHashes = new List<uint>();

        // Build hash-to-controller-index lookup for format flags
        var hashToControllerIndex = new Dictionary<uint, int>();
        for (int i = 0; i < block.BoneHashes.Length; i++)
            hashToControllerIndex[block.BoneHashes[i]] = i;

        Log.D($"IvoDBA[{animName}]: Processing {block.BoneHashes.Length} bone hashes, skeleton has {controllerIdToJointPath.Count} controller IDs");

        foreach (var boneHash in block.BoneHashes)
        {
            // Try to find joint path by bone hash (same as controller ID for Ivo)
            if (!controllerIdToJointPath.TryGetValue(boneHash, out var jointPath))
            {
                unmatchedBones++;
                unmatchedHashes.Add(boneHash);
                continue;
            }

            matchedBones++;

            block.Rotations.TryGetValue(boneHash, out var rotations);
            block.RotationTimes.TryGetValue(boneHash, out var rotTimes);
            block.Positions.TryGetValue(boneHash, out var positions);
            block.PositionTimes.TryGetValue(boneHash, out var posTimes);

            // Get position format flags from controller entry
            ushort posFormat = 0;
            if (hashToControllerIndex.TryGetValue(boneHash, out var ctrlIdx) && ctrlIdx < block.Controllers.Length)
                posFormat = block.Controllers[ctrlIdx].PosFormatFlags;

            // Only add if there's actual animation data
            if ((rotations is not null && rotations.Count > 0) ||
                (positions is not null && positions.Count > 0))
            {
                bonesWithData++;
                animatedJoints.Add(jointPath);
                tracksByJointPath[jointPath] = (rotations, rotTimes, positions, posTimes, posFormat);
            }
        }

        Log.D($"IvoDBA[{animName}]: Matched {matchedBones}/{block.BoneHashes.Length} bones, {bonesWithData} have animation data, {unmatchedBones} unmatched");

        if (unmatchedBones > 0 && unmatchedBones <= 10)
        {
            // Log individual unmatched hashes if there aren't too many
            foreach (var hash in unmatchedHashes)
            {
                Log.D($"IvoDBA[{animName}]: Unmatched bone hash 0x{hash:X08}");
            }
        }
        else if (unmatchedBones > 10)
        {
            // Just log the first few if there are many
            Log.D($"IvoDBA[{animName}]: First 5 unmatched hashes: {string.Join(", ", unmatchedHashes.Take(5).Select(h => $"0x{h:X08}"))}");
        }

        if (animatedJoints.Count == 0)
        {
            Log.W($"IvoDBA[{animName}]: No valid bone tracks found (matched={matchedBones}, withData={bonesWithData}, unmatched={unmatchedBones})");
            return (null, 0);
        }

        // Calculate frame range from all tracks
        var allFrames = new SortedSet<int>();
        foreach (var (_, (_, rotTimes, _, posTimes, _)) in tracksByJointPath)
        {
            if (rotTimes is not null)
            {
                foreach (var t in rotTimes)
                    allFrames.Add((int)t);
            }
            if (posTimes is not null)
            {
                foreach (var t in posTimes)
                    allFrames.Add((int)t);
            }
        }

        // Ensure at least one frame
        if (allFrames.Count == 0)
            allFrames.Add(0);

        int endFrame = allFrames.Count > 0 ? allFrames.Max() : 0;

        // Build time-sampled arrays
        var translationSamples = new SortedDictionary<float, List<Vector3>>();
        var rotationSamples = new SortedDictionary<float, List<Quaternion>>();
        var scaleSamples = new SortedDictionary<float, List<Vector3>>();

        // Log first bone's values to diagnose if Ivo stores absolute or delta values
        bool loggedFirstBone = false;

        foreach (var frame in allFrames)
        {
            var translations = new List<Vector3>();
            var rotations = new List<Quaternion>();
            var scales = new List<Vector3>();

            foreach (var jointPath in animatedJoints)
            {
                var (trackRots, rotTimes, trackPos, posTimes, posFormat) = tracksByJointPath[jointPath];

                // Get rest pose for this joint
                var restTranslation = jointPathToRestTranslation.TryGetValue(jointPath, out var restT)
                    ? restT
                    : Vector3.Zero;
                var restRotation = jointPathToRestRotation.TryGetValue(jointPath, out var restR)
                    ? restR
                    : Quaternion.Identity;

                // All Ivo position formats decode to ABSOLUTE local positions.
                //   C0 (0xC0xx) — raw float Vector3, used as-is.
                //   C1/C2 (0xC1xx/0xC2xx) — SNORM int16 with a per-bone [rangeMin, rangeMax]
                //     header that maps [-32767, +32767] linearly into absolute world bounds
                //     (verified against AEGS Avenger DBA: rangeMax ≈ rest, rangeMin ≈ 0,
                //     snorm=0 → midpoint of bounds, snorm=±32767 → endpoints).
                // The previous "delta from rest" interpretation doubled the bone distance,
                // because SNORM frames near rest were being added on top of rest.
                Vector3 position;
                if (trackPos is not null && trackPos.Count > 0)
                {
                    position = SampleIvoPositionDelta(trackPos, posTimes, frame);
                }
                else
                {
                    // No position animation - use rest translation for skeleton structure
                    position = restTranslation;
                }

                // Sample rotation - Ivo DBA stores ABSOLUTE rotations (not deltas)
                // Evidence: magAttach in p4ar rifle has rest rotation (0.1248, 0, 0, 0.9922),
                // and animation frames 0-11 store the SAME value, not identity.
                // If it were delta, identity would mean "no change". Storing rest value = absolute.
                Quaternion rotation;
                if (trackRots is not null && trackRots.Count > 0)
                {
                    // Use animation value directly - it's the absolute local rotation
                    rotation = Quaternion.Normalize(SampleIvoRotationDelta(trackRots, rotTimes, frame));
                }
                else
                {
                    // No rotation animation - use rest rotation for skeleton structure
                    rotation = restRotation;
                }

                translations.Add(position);
                rotations.Add(rotation);
                scales.Add(Vector3.One);
            }

            translationSamples[frame] = translations;
            rotationSamples[frame] = rotations;
            scaleSamples[frame] = scales;
        }

        // Create the SkelAnimation prim
        var cleanName = CleanPathString(animName);
        var skelAnim = new UsdSkelAnimation(cleanName);

        skelAnim.Attributes.Add(new UsdTokenArray("joints", animatedJoints, isUniform: true));
        skelAnim.Attributes.Add(new UsdTimeSampledFloat3Array("translations", translationSamples));
        skelAnim.Attributes.Add(new UsdTimeSampledQuatfArray("rotations", rotationSamples));
        skelAnim.Attributes.Add(new UsdTimeSampledHalf3Array("scales", scaleSamples));

        Log.D($"IvoDBA[{animName}]: Created animation with {animatedJoints.Count} joints, {allFrames.Count} frames");
        return (skelAnim, endFrame);
    }

    /// <summary>
    /// Samples position delta from Ivo animation data at a given frame.
    /// Returns the animation delta value (NOT absolute position).
    /// Caller must add rest translation to convert to absolute: absolute = rest + delta
    /// </summary>
    private static Vector3 SampleIvoPositionDelta(List<Vector3> positions, List<float>? keyTimes, int frame)
    {
        if (positions.Count == 0)
            return Vector3.Zero;

        if (positions.Count == 1 || keyTimes is null || keyTimes.Count == 0)
            return positions[0];

        // Find surrounding keyframes
        int i = 0;
        while (i < keyTimes.Count - 1 && keyTimes[i + 1] <= frame)
            i++;

        if (i >= positions.Count)
            return positions[^1];

        if (i == keyTimes.Count - 1 || keyTimes[i] >= frame)
            return i < positions.Count ? positions[i] : positions[^1];

        // Linear interpolation
        float t0 = keyTimes[i];
        float t1 = keyTimes[i + 1];
        float alpha = (frame - t0) / (t1 - t0);

        int i1 = Math.Min(i + 1, positions.Count - 1);
        return Vector3.Lerp(positions[i], positions[i1], alpha);
    }

    /// <summary>
    /// Samples rotation delta from Ivo animation data at a given frame.
    /// Returns the animation delta value (NOT absolute rotation).
    /// Caller must multiply by rest rotation to convert to absolute: absolute = rest * delta
    /// </summary>
    private static Quaternion SampleIvoRotationDelta(List<Quaternion> rotations, List<float>? keyTimes, int frame)
    {
        if (rotations.Count == 0)
            return Quaternion.Identity;

        if (rotations.Count == 1 || keyTimes is null || keyTimes.Count == 0)
            return rotations[0];

        // Find surrounding keyframes
        int i = 0;
        while (i < keyTimes.Count - 1 && keyTimes[i + 1] <= frame)
            i++;

        if (i >= rotations.Count)
            return rotations[^1];

        if (i == keyTimes.Count - 1 || keyTimes[i] >= frame)
            return i < rotations.Count ? rotations[i] : rotations[^1];

        // Spherical linear interpolation
        float t0 = keyTimes[i];
        float t1 = keyTimes[i + 1];
        float alpha = (frame - t0) / (t1 - t0);

        int i1 = Math.Min(i + 1, rotations.Count - 1);
        return Quaternion.Slerp(rotations[i], rotations[i1], alpha);
    }

    /// <summary>
    /// Builds a mapping from joint path to rest pose translation.
    /// Used for bones without position animation to maintain their skeleton structure.
    /// </summary>
    private Dictionary<string, Vector3> BuildRestTranslationMapping()
    {
        var mapping = new Dictionary<string, Vector3>();

        if (_bonePathMap is null)
        {
            Log.W("BuildRestTranslationMapping: _bonePathMap is null!");
            return mapping;
        }

        Log.D($"BuildRestTranslationMapping: Processing {_bonePathMap.Count} bones");
        int zeroCount = 0;

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
            // CryEngine matrices store translation in column 4 (M14, M24, M34), not row 4.
            var translation = new Vector3(restMatrix.M14, restMatrix.M24, restMatrix.M34);

            // Sanity check: if translation values are garbage (very large), use zero
            // This can happen if the skeleton matrices are corrupted or incompatible
            if (Math.Abs(translation.X) > 1e6 || Math.Abs(translation.Y) > 1e6 || Math.Abs(translation.Z) > 1e6 ||
                float.IsNaN(translation.X) || float.IsNaN(translation.Y) || float.IsNaN(translation.Z) ||
                float.IsInfinity(translation.X) || float.IsInfinity(translation.Y) || float.IsInfinity(translation.Z))
            {
                Log.W($"  Bone '{bone.BoneName}': garbage translation {translation}, using zero");
                translation = Vector3.Zero;
            }

            if (translation == Vector3.Zero)
                zeroCount++;

            mapping[jointPath] = translation;
        }

        Log.D($"BuildRestTranslationMapping: {zeroCount}/{mapping.Count} bones have zero translation");
        return mapping;
    }

    /// <summary>
    /// Builds a mapping from joint path to rest pose rotation.
    /// Used for converting additive animations to absolute.
    /// </summary>
    private Dictionary<string, Quaternion> BuildRestRotationMapping()
    {
        var mapping = new Dictionary<string, Quaternion>();

        if (_bonePathMap is null)
            return mapping;

        foreach (var (bone, jointPath) in _bonePathMap)
        {
            // Compute rest rotation: local rotation of this bone relative to parent
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

            // Extract rotation from the rest matrix.
            // Transpose before decomposing so the quaternion matches the skeleton's
            // restTransforms convention (which are also transposed from CryEngine to USD).
            if (Matrix4x4.Decompose(Matrix4x4.Transpose(restMatrix), out _, out var rotation, out _))
            {
                mapping[jointPath] = rotation;
            }
            else
            {
                mapping[jointPath] = Quaternion.Identity;
            }
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

        var keyTimes = track.PositionKeyTimes;

        if (track.Positions.Count == 1 || keyTimes.Count == 0)
        {
            var pos = track.Positions[0];
            return IsValidPosition(pos) ? pos : restTranslation;
        }

        // Find surrounding keyframes using position key times
        int i = 0;
        while (i < keyTimes.Count - 1 && keyTimes[i + 1] <= frame)
            i++;

        if (i >= track.Positions.Count)
        {
            var pos = track.Positions[^1];
            return IsValidPosition(pos) ? pos : restTranslation;
        }

        if (i == keyTimes.Count - 1 || keyTimes[i] >= frame)
        {
            var pos = i < track.Positions.Count ? track.Positions[i] : track.Positions[^1];
            return IsValidPosition(pos) ? pos : restTranslation;
        }

        // Linear interpolation
        float t0 = keyTimes[i];
        float t1 = keyTimes[i + 1];
        float alpha = (frame - t0) / (t1 - t0);

        int i1 = Math.Min(i + 1, track.Positions.Count - 1);
        var p0 = track.Positions[i];
        var p1 = track.Positions[i1];

        // If either position is invalid, fall back to rest translation
        if (!IsValidPosition(p0) || !IsValidPosition(p1))
            return restTranslation;

        return Vector3.Lerp(p0, p1, alpha);
    }

    /// <summary>
    /// Checks if a position vector has valid (non-garbage) values.
    /// Positions in character animations should be small (within a few meters).
    /// </summary>
    private static bool IsValidPosition(Vector3 pos)
    {
        const float maxValidPosition = 1e6f;
        return Math.Abs(pos.X) < maxValidPosition &&
               Math.Abs(pos.Y) < maxValidPosition &&
               Math.Abs(pos.Z) < maxValidPosition &&
               !float.IsNaN(pos.X) && !float.IsNaN(pos.Y) && !float.IsNaN(pos.Z) &&
               !float.IsInfinity(pos.X) && !float.IsInfinity(pos.Y) && !float.IsInfinity(pos.Z);
    }

    /// <summary>
    /// Samples rotation from a CAF bone track at a given frame.
    /// </summary>
    private static Quaternion SampleCafRotation(BoneTrack track, float frame)
    {
        if (track.Rotations.Count == 0)
            return Quaternion.Identity;

        var keyTimes = track.RotationKeyTimes;

        if (track.Rotations.Count == 1 || keyTimes.Count == 0)
            return track.Rotations[0];

        // Find surrounding keyframes using rotation key times
        int i = 0;
        while (i < keyTimes.Count - 1 && keyTimes[i + 1] <= frame)
            i++;

        if (i >= track.Rotations.Count)
            return track.Rotations[^1];

        if (i == keyTimes.Count - 1 || keyTimes[i] >= frame)
            return i < track.Rotations.Count ? track.Rotations[i] : track.Rotations[^1];

        // Spherical linear interpolation
        float t0 = keyTimes[i];
        float t1 = keyTimes[i + 1];
        float alpha = (frame - t0) / (t1 - t0);

        int i1 = Math.Min(i + 1, track.Rotations.Count - 1);
        return Quaternion.Slerp(track.Rotations[i], track.Rotations[i1], alpha);
    }

    /// <summary>
    /// Creates a single SkelAnimation prim from a CryEngine Animation struct.
    /// Handles both absolute and additive animations - additive animations are converted
    /// to absolute by applying deltas to the rest pose.
    /// Returns the animation prim and the end frame number.
    /// </summary>
    private (UsdSkelAnimation? skelAnim, int endFrame) CreateSkelAnimation(
        ChunkController_905.Animation anim,
        ChunkController_905 animChunk,
        Dictionary<uint, string> controllerIdToJointPath,
        string animName)
    {
        // Check if this is an additive animation
        bool isAdditive = (anim.MotionParams.AssetFlags & ChunkController_905.AssetFlags.Additive) != 0;
        if (isAdditive)
            Log.D($"DBA[{animName}]: Converting additive animation to absolute");

        // Build rest pose mappings - needed for:
        // 1. Bones without animation tracks (must use rest values to maintain skeleton structure)
        // 2. Additive animation conversion (rest * delta)
        var jointPathToRestTranslation = BuildRestTranslationMapping();
        var jointPathToRestRotation = BuildRestRotationMapping();

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

                // Get rest translation for this joint (used when no position animation)
                var restTranslation = jointPathToRestTranslation.TryGetValue(jointPath, out var restT)
                    ? restT
                    : Vector3.Zero;

                // Get translation for this joint at this frame
                // If no position track exists, use rest translation to maintain skeleton structure
                Vector3 translation = restTranslation;
                if (controller.HasPosTrack && animChunk.KeyTimes is not null && animChunk.KeyPositions is not null)
                {
                    translation = SamplePositionAtFrame(
                        animChunk.KeyTimes[controller.PosKeyTimeTrack],
                        animChunk.KeyPositions[controller.PosTrack],
                        frame);
                }

                // Get rest rotation for this joint (used when no rotation animation)
                var restRotation = jointPathToRestRotation is not null
                    && jointPathToRestRotation.TryGetValue(jointPath, out var restR)
                    ? restR
                    : Quaternion.Identity;

                // Get rotation for this joint at this frame
                // If no rotation track exists, use rest rotation to preserve bone orientation
                // (USD SkelAnimation replaces restTransform, so omitting rest = zeroing the rotation)
                Quaternion rotation = restRotation;
                if (controller.HasRotTrack && animChunk.KeyTimes is not null && animChunk.KeyRotations is not null)
                {
                    rotation = SampleRotationAtFrame(
                        animChunk.KeyTimes[controller.RotKeyTimeTrack],
                        animChunk.KeyRotations[controller.RotTrack],
                        frame);
                }

                if (isAdditive)
                {
                    // Convert additive deltas to absolute transforms:
                    // Additive rotation: absolute = rest * delta
                    // Additive translation: absolute = rest + delta (translation already has rest if no pos track)
                    rotation = restRotation * rotation;
                    // For additive with position track, add animation delta to rest
                    if (controller.HasPosTrack)
                        translation = restTranslation + translation;
                }

                translations.Add(translation);
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

        // Process DBA animations (both standard ChunkController_905 and Ivo format)
        if (hasDbaAnimations)
        {
            foreach (var animModel in _cryData.Animations!)
            {
                // Try standard DBA format (ChunkController_905)
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

                // Try Ivo DBA format (ChunkIvoDBAData + ChunkIvoDBAMetadata)
                var ivoDbaData = animModel.ChunkMap.Values.OfType<ChunkIvoDBAData>().FirstOrDefault();
                var ivoDbaMetadata = animModel.ChunkMap.Values.OfType<ChunkIvoDBAMetadata>().FirstOrDefault();

                if (ivoDbaData is not null && ivoDbaData.AnimationBlocks.Count > 0)
                {
                    var animNames = ivoDbaMetadata?.AnimPaths ?? [];
                    var animEntries = ivoDbaMetadata?.Entries ?? [];

                    for (int i = 0; i < ivoDbaData.AnimationBlocks.Count; i++)
                    {
                        var block = ivoDbaData.AnimationBlocks[i];
                        var animName = i < animNames.Count
                            ? Path.GetFileNameWithoutExtension(animNames[i])
                            : $"animation_{i}";
                        var metaEntry = i < animEntries.Count ? animEntries[i] : (IvoDBAMetaEntry?)null;

                        var (skelAnim, endFrame) = CreateSkelAnimationFromIvoDBA(
                            block, animName, metaEntry, controllerIdToJointPath);

                        if (skelAnim is not null)
                        {
                            allAnimations.Add((animName, skelAnim, endFrame));
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
                DefaultPrim = _rootPrimName,
                TimeCodesPerSecond = 30,
                FramesPerSecond = 30,
                StartTimeCode = 0,
                EndTimeCode = endFrame
            }
        };

        // Create root
        usdDoc.Prims.Add(new UsdXform(_rootPrimName, "/"));
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
        var animPath = $"</{_rootPrimName}/Armature/{skelAnim.Name}>";
        skeleton.Attributes.Add(new UsdRelationship("skel:animationSource", animPath));

        skelRoot.Children.Add(skeleton);
        skelRoot.Children.Add(skelAnim);
        rootPrim.Children.Add(skelRoot);

        return usdDoc;
    }
}
