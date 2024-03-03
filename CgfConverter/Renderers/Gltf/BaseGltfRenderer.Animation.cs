using System.Collections.Generic;
using System.IO;
using System.Linq;
using CgfConverter.CryEngineCore;
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
        IReadOnlyDictionary<int, int> controllerIdToNodeIndex)
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
        IReadOnlyDictionary<int, int> controllerIdToNodeIndex)
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
}
