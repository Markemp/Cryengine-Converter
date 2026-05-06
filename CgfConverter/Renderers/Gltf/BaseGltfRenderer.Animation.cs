using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Models.Structs;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Utilities;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    private bool CreateAnimation(
        out GltfAnimation newAnimation,
        ChunkController_905.Animation anim,
        IReadOnlyDictionary<int, int> keyTimeAccessors,
        IReadOnlyDictionary<int, int> keyPositionAccessors,
        IReadOnlyDictionary<int, int> keyRotationAccessors,
        IReadOnlyDictionary<uint, int> controllerIdToNodeIndex)
    {
        newAnimation = new GltfAnimation { Name = anim.Name };

        foreach (var con in anim.Controllers)
        {
            if (!controllerIdToNodeIndex.TryGetValue(con.ControllerID, out var nodeIndex))
            {
                Log.W<bool>("Animation[{0}]: Controller 0x{1:X08} not found.",
                    anim.Name, con.ControllerID, con.PosTrack);
                continue;
            }

            if (con.HasPosTrack)
            {
                if (!keyTimeAccessors.TryGetValue(con.PosKeyTimeTrack, out var input))
                    Log.W<bool>("Animation[{0}] Controller[0x{1:X08}]: PosKeyTimeTrack #{2} not found.",
                        anim.Name, con.ControllerID, con.PosKeyTimeTrack);
                else if (!keyPositionAccessors.TryGetValue(con.PosTrack, out var output))
                    Log.W<bool>("Animation[{0}] Controller[0x{1:X08}]: PosTrack #{2} not found.",
                        anim.Name, con.ControllerID, con.PosTrack);
                else
                {
                    newAnimation.Samplers.Add(new GltfAnimationSampler
                    {
                        Input = input,
                        Output = output,
                        Interpolation = GltfAnimationSamplerInterpolation.Linear,
                    });
                    newAnimation.Channels.Add(new GltfAnimationChannel
                    {
                        Sampler = newAnimation.Samplers.Count - 1,
                        Target = new GltfAnimationChannelTarget
                        {
                            Node = nodeIndex,
                            Path = GltfAnimationChannelTargetPath.Translation,
                        },
                    });
                }
            }

            if (con.HasRotTrack)
            {
                if (!keyTimeAccessors.TryGetValue(con.RotKeyTimeTrack, out var input))
                    Log.W<bool>("Animation[{0}] Controller[0x{1:X08}]: RotKeyTimeTrack #{2} not found.",
                        anim.Name, con.ControllerID, con.RotKeyTimeTrack);
                else if (!keyRotationAccessors.TryGetValue(con.RotTrack, out var output))
                    Log.W<bool>("Animation[{0}] Controller[0x{1:X08}]: RotTrack #{2} not found.",
                        anim.Name, con.ControllerID, con.RotTrack);
                else
                {
                    newAnimation.Samplers.Add(new GltfAnimationSampler
                    {
                        Input = input,
                        Output = output,
                        Interpolation = GltfAnimationSamplerInterpolation.Linear,
                    });
                    newAnimation.Channels.Add(new GltfAnimationChannel
                    {
                        Sampler = newAnimation.Samplers.Count - 1,
                        Target = new GltfAnimationChannelTarget
                        {
                            Node = nodeIndex,
                            Path = GltfAnimationChannelTargetPath.Rotation,
                        },
                    });
                }
            }
        }

        return newAnimation.Samplers.Any() && newAnimation.Channels.Any();
    }

    private int WriteAnimations(
        IEnumerable<Model> animationContainers,
        IReadOnlyDictionary<uint, int> controllerIdToNodeIndex)
    {
        var numAnimationsWritten = 0;
        var animChunks = animationContainers
            .SelectMany(x => x.ChunkMap.Values.OfType<ChunkController_905>())
            .ToList();
        foreach (var animChunk in animChunks)
        {
            var keyTimeAccessors = animChunk.Animations.SelectMany(x =>
                    x.Controllers
                        .Where(y => y.HasPosTrack)
                        .Select(y => y.PosKeyTimeTrack)
                        .Concat(x.Controllers.Where(y => y.HasRotTrack).Select(y => y.RotKeyTimeTrack)))
                .Distinct()
                .ToDictionary(i => i, i =>
                    AddAccessor(
                        $"animation/time/{i}", -1, null,
                        animChunk.KeyTimes[i].Select(x => (x - animChunk.KeyTimes[i][0]) / 30f).ToArray()));

            var keyPositionAccessors = animChunk.Animations.SelectMany(x =>
                    x.Controllers.Where(y => y.HasPosTrack).Select(y => y.PosTrack))
                .Distinct()
                .ToDictionary(i => i, i =>
                    AddAccessor(
                        $"animation/translation/{i}", -1, null,
                        animChunk.KeyPositions[i].Select(SwapAxesForPosition).ToArray()));

            var keyRotationAccessors = animChunk.Animations.SelectMany(x =>
                    x.Controllers.Where(y => y.HasRotTrack).Select(y => y.RotTrack))
                .Distinct()
                .ToDictionary(i => i, i =>
                    AddAccessor(
                        $"animation/rotation/{i}", -1, null,
                        animChunk.KeyRotations[i].Select(SwapAxesForAnimations).ToArray()));

            // Debug: log turret_arm rotation (CtrlID = 0x9384FC75)
            foreach (var anim in animChunk.Animations)
            {
                foreach (var controller in anim.Controllers)
                {
                    if (controller.ControllerID == 0x9384FC75 && controller.HasRotTrack)
                    {
                        var rawRot = animChunk.KeyRotations[controller.RotTrack][0];
                        var swappedRot = SwapAxesForAnimations(rawRot);
                        Log.I($"glTF DBA turret_arm frame 0: raw quat = ({rawRot.X:F6}, {rawRot.Y:F6}, {rawRot.Z:F6}, {rawRot.W:F6})");
                        Log.I($"glTF DBA turret_arm frame 0: swapped quat = ({swappedRot.X:F6}, {swappedRot.Y:F6}, {swappedRot.Z:F6}, {swappedRot.W:F6})");
                        var angle = 2 * System.Math.Acos(System.Math.Clamp(rawRot.W, -1, 1)) * 180 / System.Math.PI;
                        var axis = new Vector3(rawRot.X, rawRot.Y, rawRot.Z);
                        if (axis.LengthSquared() > 0.0001f)
                            axis = Vector3.Normalize(axis);
                        Log.I($"glTF DBA turret_arm frame 0: raw axis=({axis.X:F3}, {axis.Y:F3}, {axis.Z:F3}), angle={angle:F2}°");
                        goto doneLogging; // Only log once
                    }
                }
            }
            doneLogging:;

            var names = GltfRendererUtilities.StripCommonParentPaths(
                animChunk.Animations.Select(x => Path.ChangeExtension(x.Name, null)).ToList());

            foreach (var (anim, name) in animChunk.Animations.Zip(names)
                         .OrderBy(pair => pair.Second.ToLowerInvariant()))
            {
                if (!CreateAnimation(out var newAnimation,
                        anim,
                        keyTimeAccessors,
                        keyPositionAccessors,
                        keyRotationAccessors,
                        controllerIdToNodeIndex))
                    continue;

                newAnimation.Name = name;
                AddAnimation(newAnimation);
                numAnimationsWritten++;
            }
        }

        return numAnimationsWritten;
    }

    /// <summary>
    /// Writes CAF animations (from .caf files and .cal animation lists) to glTF.
    /// </summary>
    private int WriteCafAnimations(
        IEnumerable<CafAnimation>? cafAnimations,
        IReadOnlyDictionary<uint, int> controllerIdToNodeIndex)
    {
        if (cafAnimations is null)
            return 0;

        var cafList = cafAnimations.ToList();
        if (cafList.Count == 0)
            return 0;

        var numAnimationsWritten = 0;

        foreach (var cafAnim in cafList)
        {
            if (!CreateCafAnimation(out var newAnimation, cafAnim, controllerIdToNodeIndex))
                continue;

            AddAnimation(newAnimation);
            numAnimationsWritten++;
        }

        return numAnimationsWritten;
    }

    /// <summary>
    /// Creates a glTF animation from a CAF animation.
    /// </summary>
    private bool CreateCafAnimation(
        out GltfAnimation newAnimation,
        CafAnimation cafAnim,
        IReadOnlyDictionary<uint, int> controllerIdToNodeIndex)
    {
        var cleanName = Path.GetFileNameWithoutExtension(cafAnim.Name);
        newAnimation = new GltfAnimation { Name = cleanName };

        foreach (var (controllerId, track) in cafAnim.BoneTracks)
        {
            // Try to find node by controller ID
            if (!controllerIdToNodeIndex.TryGetValue(controllerId, out var nodeIndex))
            {
                // Try using bone name from CAF's bone name list
                if (cafAnim.ControllerIdToBoneName.TryGetValue(controllerId, out var boneName))
                {
                    // Search for matching node by name - this requires iterating controllerIdToNodeIndex
                    // Since we don't have reverse mapping, log and skip
                    Log.D("CAF[{0}]: Controller 0x{1:X08} ({2}) not found in skeleton",
                        cafAnim.Name, controllerId, boneName);
                }
                else
                {
                    Log.D("CAF[{0}]: Controller 0x{1:X08} not found in skeleton",
                        cafAnim.Name, controllerId);
                }
                continue;
            }

            // Create position animation channel if we have position data
            if (track.Positions.Count > 0)
            {
                var keyTimes = track.PositionKeyTimes.Count > 0
                    ? track.PositionKeyTimes
                    : track.KeyTimes;

                if (keyTimes.Count > 0)
                {
                    // Normalize key times to seconds (assuming 30fps if times look like frame numbers)
                    var startTime = keyTimes[0];
                    var timeAccessor = AddAccessor(
                        $"caf/{cleanName}/pos_time/{controllerId:X08}", -1, null,
                        keyTimes.Select(t => (t - startTime) / 30f).ToArray());

                    var posAccessor = AddAccessor(
                        $"caf/{cleanName}/pos/{controllerId:X08}", -1, null,
                        track.Positions.Select(SwapAxesForPosition).ToArray());

                    newAnimation.Samplers.Add(new GltfAnimationSampler
                    {
                        Input = timeAccessor,
                        Output = posAccessor,
                        Interpolation = GltfAnimationSamplerInterpolation.Linear,
                    });
                    newAnimation.Channels.Add(new GltfAnimationChannel
                    {
                        Sampler = newAnimation.Samplers.Count - 1,
                        Target = new GltfAnimationChannelTarget
                        {
                            Node = nodeIndex,
                            Path = GltfAnimationChannelTargetPath.Translation,
                        },
                    });
                }
            }

            // Create rotation animation channel if we have rotation data
            if (track.Rotations.Count > 0)
            {
                var keyTimes = track.RotationKeyTimes.Count > 0
                    ? track.RotationKeyTimes
                    : track.KeyTimes;

                if (keyTimes.Count > 0)
                {
                    // Normalize key times to seconds
                    var startTime = keyTimes[0];
                    var timeAccessor = AddAccessor(
                        $"caf/{cleanName}/rot_time/{controllerId:X08}", -1, null,
                        keyTimes.Select(t => (t - startTime) / 30f).ToArray());

                    var rotAccessor = AddAccessor(
                        $"caf/{cleanName}/rot/{controllerId:X08}", -1, null,
                        track.Rotations.Select(SwapAxesForAnimations).ToArray());

                    newAnimation.Samplers.Add(new GltfAnimationSampler
                    {
                        Input = timeAccessor,
                        Output = rotAccessor,
                        Interpolation = GltfAnimationSamplerInterpolation.Linear,
                    });
                    newAnimation.Channels.Add(new GltfAnimationChannel
                    {
                        Sampler = newAnimation.Samplers.Count - 1,
                        Target = new GltfAnimationChannelTarget
                        {
                            Node = nodeIndex,
                            Path = GltfAnimationChannelTargetPath.Rotation,
                        },
                    });
                }
            }
        }

        return newAnimation.Samplers.Count > 0 && newAnimation.Channels.Count > 0;
    }

    /// <summary>
    /// Writes Ivo DBA animations (from Star Citizen #ivo .dba files) to glTF.
    /// Ivo DBA uses ChunkIvoDBAData + ChunkIvoDBAMetadata instead of ChunkController_905.
    ///
    /// IMPORTANT: Ivo animations store values as DELTAS from rest pose, not absolute
    /// local transforms. glTF expects absolute joint-local transforms.
    /// We must convert: absolute = rest + delta (translation), absolute = rest * delta (rotation)
    /// </summary>
    private int WriteIvoDbaAnimations(
        IEnumerable<Model>? animationContainers,
        IReadOnlyDictionary<uint, int> controllerIdToNodeIndex,
        SkinningInfo? skinningInfo = null)
    {
        if (animationContainers is null)
            return 0;

        // Build rest pose mappings for converting Ivo deltas to absolute transforms
        // Ivo animations store deltas from rest pose, but glTF expects absolute local transforms
        var controllerIdToRestTranslation = new Dictionary<uint, Vector3>();
        var controllerIdToRestRotation = new Dictionary<uint, Quaternion>();

        if (skinningInfo?.CompiledBones is not null)
        {
            BuildIvoRestPoseMappings(skinningInfo, controllerIdToRestTranslation, controllerIdToRestRotation);
        }

        var numAnimationsWritten = 0;

        foreach (var animModel in animationContainers)
        {
            var ivoDbaData = animModel.ChunkMap.Values.OfType<ChunkIvoDBAData>().FirstOrDefault();
            var ivoDbaMetadata = animModel.ChunkMap.Values.OfType<ChunkIvoDBAMetadata>().FirstOrDefault();

            if (ivoDbaData is null || ivoDbaData.AnimationBlocks.Count == 0)
                continue;

            Log.D("Found Ivo DBA with {0} animation blocks", ivoDbaData.AnimationBlocks.Count);

            var animNames = ivoDbaMetadata?.AnimPaths ?? [];

            for (int i = 0; i < ivoDbaData.AnimationBlocks.Count; i++)
            {
                var block = ivoDbaData.AnimationBlocks[i];
                var animName = i < animNames.Count
                    ? Path.GetFileNameWithoutExtension(animNames[i])
                    : $"animation_{i}";

                if (!CreateIvoDbaAnimation(out var newAnimation, block, animName, controllerIdToNodeIndex,
                    controllerIdToRestTranslation, controllerIdToRestRotation))
                    continue;

                AddAnimation(newAnimation);
                numAnimationsWritten++;
                Log.D("Created Ivo DBA animation: {0}", animName);
            }
        }

        return numAnimationsWritten;
    }

    /// <summary>
    /// Builds rest pose translation and rotation mappings for Ivo animation conversion.
    /// Rest pose values are in CryEngine space (before axis swap).
    /// </summary>
    private void BuildIvoRestPoseMappings(
        SkinningInfo skinningInfo,
        Dictionary<uint, Vector3> controllerIdToRestTranslation,
        Dictionary<uint, Quaternion> controllerIdToRestRotation)
    {
        foreach (var bone in skinningInfo.CompiledBones)
        {
            // Compute local transform in CryEngine space (same as skeleton creation)
            Matrix4x4 localMatrix;

            if (bone.ParentBone == null)
            {
                if (!Matrix4x4.Invert(bone.BindPoseMatrix, out localMatrix))
                {
                    localMatrix = Matrix4x4.Identity;
                }
            }
            else
            {
                if (Matrix4x4.Invert(bone.BindPoseMatrix, out var childBoneToWorld))
                {
                    localMatrix = bone.ParentBone.BindPoseMatrix * childBoneToWorld;
                }
                else
                {
                    localMatrix = Matrix4x4.Identity;
                }
            }

            // Extract translation from column 4 (CryEngine convention: M14, M24, M34)
            var restTranslation = new Vector3(localMatrix.M14, localMatrix.M24, localMatrix.M34);

            // Extract rotation from the 3x3 rotation part
            // Build a proper rotation matrix by normalizing the 3x3 part
            var rotMatrix = new Matrix4x4(
                localMatrix.M11, localMatrix.M12, localMatrix.M13, 0,
                localMatrix.M21, localMatrix.M22, localMatrix.M23, 0,
                localMatrix.M31, localMatrix.M32, localMatrix.M33, 0,
                0, 0, 0, 1);

            Quaternion restRotation;
            if (Matrix4x4.Decompose(rotMatrix, out _, out restRotation, out _))
            {
                // Success
            }
            else
            {
                restRotation = Quaternion.Identity;
            }

            // Store by controller ID
            controllerIdToRestTranslation[bone.ControllerID] = restTranslation;
            controllerIdToRestRotation[bone.ControllerID] = restRotation;

            // Also store by CRC32 hash of bone name for Ivo matching
            if (!string.IsNullOrEmpty(bone.BoneName))
            {
                var crc32 = Crc32CryEngine.Compute(bone.BoneName);
                controllerIdToRestTranslation.TryAdd(crc32, restTranslation);
                controllerIdToRestRotation.TryAdd(crc32, restRotation);

                var crc32Lower = Crc32CryEngine.Compute(bone.BoneName.ToLowerInvariant());
                if (crc32Lower != crc32)
                {
                    controllerIdToRestTranslation.TryAdd(crc32Lower, restTranslation);
                    controllerIdToRestRotation.TryAdd(crc32Lower, restRotation);
                }
            }
        }
    }

    /// <summary>
    /// Creates a glTF animation from an Ivo DBA animation block.
    /// Converts Ivo delta values to absolute transforms before axis swap.
    /// </summary>
    private bool CreateIvoDbaAnimation(
        out GltfAnimation newAnimation,
        IvoAnimationBlock block,
        string animName,
        IReadOnlyDictionary<uint, int> controllerIdToNodeIndex,
        IReadOnlyDictionary<uint, Vector3> controllerIdToRestTranslation,
        IReadOnlyDictionary<uint, Quaternion> controllerIdToRestRotation)
    {
        newAnimation = new GltfAnimation { Name = animName };

        foreach (var boneHash in block.BoneHashes)
        {
            // Try to find node by bone hash (CRC32 of bone name)
            if (!controllerIdToNodeIndex.TryGetValue(boneHash, out var nodeIndex))
            {
                Log.D("IvoDBA[{0}]: Bone hash 0x{1:X08} not found in skeleton", animName, boneHash);
                continue;
            }

            // Get rest pose for this bone (in CryEngine space)
            var restTranslation = controllerIdToRestTranslation.TryGetValue(boneHash, out var restT)
                ? restT
                : Vector3.Zero;
            var restRotation = controllerIdToRestRotation.TryGetValue(boneHash, out var restR)
                ? restR
                : Quaternion.Identity;

            // Get rotation and position data for this bone
            block.Rotations.TryGetValue(boneHash, out var rotations);
            block.RotationTimes.TryGetValue(boneHash, out var rotTimes);
            block.Positions.TryGetValue(boneHash, out var positions);
            block.PositionTimes.TryGetValue(boneHash, out var posTimes);

            // Create position animation channel if we have position data
            if (positions is not null && positions.Count > 0)
            {
                var keyTimes = posTimes ?? [];
                if (keyTimes.Count == 0 && positions.Count > 0)
                {
                    // Generate uniform time distribution if no explicit times
                    keyTimes = Enumerable.Range(0, positions.Count).Select(t => (float)t).ToList();
                }

                if (keyTimes.Count > 0)
                {
                    var startTime = keyTimes[0];
                    var timeAccessor = AddAccessor(
                        $"ivo_dba/{animName}/pos_time/{boneHash:X08}", -1, null,
                        keyTimes.Select(t => (t - startTime) / 30f).ToArray());

                    // All Ivo position formats decode to absolute local positions.
                    // C0 (float) is stored as a raw Vector3; C1/C2 (SNORM) decode through
                    // a per-bone [rangeMin, rangeMax] header that maps int16 linearly into
                    // absolute world bounds. No rest translation should be added.
                    var absolutePositions = positions
                        .Select(SwapAxesForPosition)
                        .ToArray();

                    var posAccessor = AddAccessor(
                        $"ivo_dba/{animName}/pos/{boneHash:X08}", -1, null,
                        absolutePositions);

                    newAnimation.Samplers.Add(new GltfAnimationSampler
                    {
                        Input = timeAccessor,
                        Output = posAccessor,
                        Interpolation = GltfAnimationSamplerInterpolation.Linear,
                    });
                    newAnimation.Channels.Add(new GltfAnimationChannel
                    {
                        Sampler = newAnimation.Samplers.Count - 1,
                        Target = new GltfAnimationChannelTarget
                        {
                            Node = nodeIndex,
                            Path = GltfAnimationChannelTargetPath.Translation,
                        },
                    });
                }
            }

            // Create rotation animation channel if we have rotation data
            if (rotations is not null && rotations.Count > 0)
            {
                var keyTimes = rotTimes ?? [];
                if (keyTimes.Count == 0 && rotations.Count > 0)
                {
                    // Generate uniform time distribution if no explicit times
                    keyTimes = Enumerable.Range(0, rotations.Count).Select(t => (float)t).ToList();
                }

                if (keyTimes.Count > 0)
                {
                    var startTime = keyTimes[0];
                    var timeAccessor = AddAccessor(
                        $"ivo_dba/{animName}/rot_time/{boneHash:X08}", -1, null,
                        keyTimes.Select(t => (t - startTime) / 30f).ToArray());

                    // Ivo DBA stores ABSOLUTE local rotations, not deltas
                    // Just swap axes for glTF coordinate system
                    var absoluteRotations = rotations
                        .Select(SwapAxesForAnimations)
                        .ToArray();

                    var rotAccessor = AddAccessor(
                        $"ivo_dba/{animName}/rot/{boneHash:X08}", -1, null,
                        absoluteRotations);

                    newAnimation.Samplers.Add(new GltfAnimationSampler
                    {
                        Input = timeAccessor,
                        Output = rotAccessor,
                        Interpolation = GltfAnimationSamplerInterpolation.Linear,
                    });
                    newAnimation.Channels.Add(new GltfAnimationChannel
                    {
                        Sampler = newAnimation.Samplers.Count - 1,
                        Target = new GltfAnimationChannelTarget
                        {
                            Node = nodeIndex,
                            Path = GltfAnimationChannelTargetPath.Rotation,
                        },
                    });
                }
            }
        }

        return newAnimation.Samplers.Count > 0 && newAnimation.Channels.Count > 0;
    }
}
