using System.Collections.Generic;
using System.IO;
using System.Linq;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Gltf.Models;

namespace CgfConverter.Renderers.Gltf;

public partial class GltfRendererCommon
{
    private void WriteAnimation(
        string name,
        ChunkController_905.Animation anim,
        IReadOnlyDictionary<int, int> keyTimeAccessors,
        IReadOnlyDictionary<int, int> keyPositionAccessors,
        IReadOnlyDictionary<int, int> keyRotationAccessors,
        IReadOnlyDictionary<uint, int> controllerIdToNodeIndex)
    {
        var animationNode = new GltfAnimation
        {
            Name = name,
        };
        _gltf.Add(animationNode);

        foreach (var con in anim.Controllers)
        {
            if (con.HasPosTrack)
            {
                animationNode.Samplers.Add(new GltfAnimationSampler
                {
                    Input = keyTimeAccessors[con.PosKeyTimeTrack],
                    Output = keyPositionAccessors[con.PosTrack],
                    Interpolation = GltfAnimationSamplerInterpolation.Linear,
                });
                animationNode.Channels.Add(new GltfAnimationChannel
                {
                    Sampler = animationNode.Samplers.Count - 1,
                    Target = new GltfAnimationChannelTarget
                    {
                        Node = controllerIdToNodeIndex[con.ControllerID],
                        Path = GltfAnimationChannelTargetPath.Translation,
                    },
                });
            }

            if (con.HasRotTrack)
            {
                animationNode.Samplers.Add(new GltfAnimationSampler
                {
                    Input = keyTimeAccessors[con.RotKeyTimeTrack],
                    Output = keyRotationAccessors[con.RotTrack],
                    Interpolation = GltfAnimationSamplerInterpolation.Linear,
                });
                animationNode.Channels.Add(new GltfAnimationChannel
                {
                    Sampler = animationNode.Samplers.Count - 1,
                    Target = new GltfAnimationChannelTarget
                    {
                        Node = controllerIdToNodeIndex[con.ControllerID],
                        Path = GltfAnimationChannelTargetPath.Rotation,
                    },
                });
            }
        }
    }

    private void WriteAnimations(
        IEnumerable<Model> animationContainers,
        IReadOnlyDictionary<uint, int> controllerIdToNodeIndex)
    {
        var animChunks = animationContainers
            .SelectMany(x => x.ChunkMap.Values.OfType<ChunkController_905>())
            .ToList();
        foreach (var animChunk in animChunks)
        {
            var keyTimeAccessors = animChunk.Animations.SelectMany(x =>
                    x.Controllers.Where(y => y.HasPosTrack).Select(y => y.PosKeyTimeTrack)
                        .Concat(x.Controllers.Where(y => y.HasRotTrack).Select(y => y.RotKeyTimeTrack)))
                .Distinct()
                .ToDictionary(i => i, i =>
                    _gltf.AddAccessor(
                        $"animation/time/{i}", -1,
                        animChunk.KeyTimes[i].Select(x => (x - animChunk.KeyTimes[i][0]) / 30f).ToArray()));

            var keyPositionAccessors = animChunk.Animations.SelectMany(x =>
                    x.Controllers.Where(y => y.HasPosTrack).Select(y => y.PosTrack))
                .Distinct()
                .ToDictionary(i => i, i =>
                    _gltf.AddAccessor(
                        $"animation/translation/{i}", -1,
                        animChunk.KeyPositions[i].Select(SwapAxesForPosition).ToArray()));

            var keyRotationAccessors = animChunk.Animations.SelectMany(x =>
                    x.Controllers.Where(y => y.HasRotTrack).Select(y => y.RotTrack))
                .Distinct()
                .ToDictionary(i => i, i =>
                    _gltf.AddAccessor(
                        $"animation/rotation/{i}", -1,
                        animChunk.KeyRotations[i].Select(SwapAxesForAnimations).ToArray()));

            var names = GltfRendererUtilities.StripCommonParentPaths(
                animChunk.Animations.Select(x => Path.ChangeExtension(x.Name, null)).ToList());

            foreach (var (anim, name) in animChunk.Animations.Zip(names)
                         .OrderBy(pair => pair.Second.ToLowerInvariant()))
                WriteAnimation(name, anim, keyTimeAccessors, keyPositionAccessors, keyRotationAccessors,
                    controllerIdToNodeIndex);
        }
    }
}