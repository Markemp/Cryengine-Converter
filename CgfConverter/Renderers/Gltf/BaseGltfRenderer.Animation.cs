using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Renderers.Gltf.Models;

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
}
