using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Animation;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Collada.Collada.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CgfConverter.Renderers.Collada;

/// <summary>
/// ColladaModelRenderer partial class - Animation export support
/// </summary>
public partial class ColladaModelRenderer
{
    public void WriteLibrary_Animations()
    {
        if (controllerIdToBoneName.Count == 0)
        {
            Log.D("No bone mappings available for animation export");
            return;
        }

        var animationLibrary = new ColladaLibraryAnimations();
        var allAnimations = new List<ColladaAnimation>();

        // Find all the 905 controller chunks from animation files (.dba)
        foreach (var animChunk in _cryData.Animations
            .SelectMany(x => x.ChunkMap.Values.OfType<ChunkController_905>())
            .ToList())
        {
            if (animChunk?.Animations is null || animChunk.Animations.Count == 0)
                continue;

            foreach (var animation in animChunk.Animations)
            {
                var animationName = CleanAnimationName(animation.Name);
                var boneAnimations = CreateMatrixAnimationsForClip(animation, animChunk, animationName);

                if (boneAnimations.Count > 0)
                {
                    // Create root animation containing all bone animations for this clip
                    var rootAnimation = new ColladaAnimation
                    {
                        Name = animationName,
                        ID = $"{animationName}_animation",
                        Animation = boneAnimations.ToArray()
                    };
                    allAnimations.Add(rootAnimation);
                    Log.D($"Created animation: {animationName} with {boneAnimations.Count} bone channels");
                }
            }
        }

        if (allAnimations.Count > 0)
        {
            animationLibrary.Animation = allAnimations.ToArray();
            DaeObject.Library_Animations = animationLibrary;
            Log.I($"Exported {allAnimations.Count} animation(s)");
        }
    }

    /// <summary>
    /// Creates matrix-based animations for all bones in a single animation clip.
    /// </summary>
    private List<ColladaAnimation> CreateMatrixAnimationsForClip(
        ChunkController_905.Animation animation,
        ChunkController_905 animChunk,
        string animationName)
    {
        var boneAnimations = new List<ColladaAnimation>();

        foreach (var controller in animation.Controllers)
        {
            if (!controllerIdToBoneName.TryGetValue(controller.ControllerID, out var boneName))
            {
                Log.D($"Animation[{animationName}]: Controller 0x{controller.ControllerID:X08} not found in skeleton");
                continue;
            }

            if (!controller.HasPosTrack && !controller.HasRotTrack)
                continue;

            var boneAnim = CreateBoneMatrixAnimation(controller, animChunk, boneName, animationName);
            if (boneAnim is not null)
                boneAnimations.Add(boneAnim);
        }

        return boneAnimations;
    }

    /// <summary>
    /// Creates a single bone's matrix animation by composing position and rotation into 4x4 matrices.
    /// </summary>
    private ColladaAnimation? CreateBoneMatrixAnimation(
        ChunkController_905.CControllerInfo controller,
        ChunkController_905 animChunk,
        string boneName,
        string animationName)
    {
        // Collect all unique frame times from both position and rotation tracks
        var frameTimes = new SortedSet<float>();

        if (controller.HasPosTrack && animChunk.KeyTimes is not null)
        {
            foreach (var t in animChunk.KeyTimes[controller.PosKeyTimeTrack])
                frameTimes.Add(t);
        }

        if (controller.HasRotTrack && animChunk.KeyTimes is not null)
        {
            foreach (var t in animChunk.KeyTimes[controller.RotKeyTimeTrack])
                frameTimes.Add(t);
        }

        if (frameTimes.Count == 0)
            return null;

        var frameList = frameTimes.ToList();
        var startTime = frameList[0];

        // Build matrices for each frame by composing rotation and translation
        var matrices = new List<Matrix4x4>();
        foreach (var frame in frameList)
        {
            var position = SamplePositionAtFrame(controller, animChunk, frame);
            var rotation = SampleRotationAtFrame(controller, animChunk, frame);

            // Compose matrix: rotation * translation (local transform)
            var matrix = Matrix4x4.CreateFromQuaternion(rotation);
            matrix.Translation = position;
            matrices.Add(matrix);
        }

        // Create the animation sources
        var sourceIdBase = $"{animationName}_{boneName}";

        // Time source (convert frames to seconds at 30fps)
        var timeSource = new ColladaSource
        {
            ID = $"{sourceIdBase}_time",
            Name = $"{sourceIdBase}_time",
            Float_Array = new ColladaFloatArray
            {
                ID = $"{sourceIdBase}_time_array",
                Count = frameList.Count,
                Value_As_String = string.Join(" ", frameList.Select(t => ((t - startTime) / 30f).ToString("F6")))
            },
            Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = $"#{sourceIdBase}_time_array",
                    Count = (uint)frameList.Count,
                    Stride = 1,
                    Param = [new ColladaParam { Name = "TIME", Type = "float" }]
                }
            }
        };

        // Matrix output source (16 floats per matrix)
        var matrixSource = new ColladaSource
        {
            ID = $"{sourceIdBase}_matrix",
            Name = $"{sourceIdBase}_matrix",
            Float_Array = new ColladaFloatArray
            {
                ID = $"{sourceIdBase}_matrix_array",
                Count = frameList.Count * 16,
                Value_As_String = string.Join(" ", matrices.Select(CreateStringFromMatrix4x4))
            },
            Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = $"#{sourceIdBase}_matrix_array",
                    Count = (uint)frameList.Count,
                    Stride = 16,
                    Param = [new ColladaParam { Name = "TRANSFORM", Type = "float4x4" }]
                }
            }
        };

        // Interpolation source
        var interpolationSource = new ColladaSource
        {
            ID = $"{sourceIdBase}_interpolation",
            Name = $"{sourceIdBase}_interpolation",
            Name_Array = new ColladaNameArray
            {
                ID = $"{sourceIdBase}_interpolation_array",
                Count = frameList.Count,
                Value_Pre_Parse = string.Join(" ", Enumerable.Repeat("LINEAR", frameList.Count))
            },
            Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = $"#{sourceIdBase}_interpolation_array",
                    Count = (uint)frameList.Count,
                    Stride = 1,
                    Param = [new ColladaParam { Name = "INTERPOLATION", Type = "name" }]
                }
            }
        };

        // Sampler
        var sampler = new ColladaSampler
        {
            ID = $"{sourceIdBase}_sampler",
            Input =
            [
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.INPUT, source = $"#{sourceIdBase}_time" },
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.OUTPUT, source = $"#{sourceIdBase}_matrix" },
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.INTERPOLATION, source = $"#{sourceIdBase}_interpolation" }
            ]
        };

        // Channel targeting the bone's matrix transform
        var channel = new ColladaChannel
        {
            Source = $"#{sourceIdBase}_sampler",
            Target = $"{boneName}/matrix"  // Targets the joint node's matrix sID
        };

        return new ColladaAnimation
        {
            ID = $"{sourceIdBase}_animation",
            Name = $"{boneName}_matrix",
            Source = [timeSource, matrixSource, interpolationSource],
            Sampler = [sampler],
            Channel = [channel]
        };
    }

    /// <summary>
    /// Samples position at a given frame using linear interpolation.
    /// </summary>
    private static Vector3 SamplePositionAtFrame(
        ChunkController_905.CControllerInfo controller,
        ChunkController_905 animChunk,
        float frame)
    {
        if (!controller.HasPosTrack || animChunk.KeyTimes is null || animChunk.KeyPositions is null)
            return Vector3.Zero;

        var keyTimes = animChunk.KeyTimes[controller.PosKeyTimeTrack];
        var keyPositions = animChunk.KeyPositions[controller.PosTrack];

        if (keyTimes.Count == 0 || keyPositions.Count == 0)
            return Vector3.Zero;

        // Find surrounding keyframes
        int i = 0;
        while (i < keyTimes.Count - 1 && keyTimes[i + 1] <= frame)
            i++;

        if (i >= keyPositions.Count)
            return keyPositions[^1];

        if (i == keyTimes.Count - 1 || keyTimes[i] >= frame)
            return keyPositions[i];

        // Linear interpolation
        float t0 = keyTimes[i];
        float t1 = keyTimes[i + 1];
        float alpha = (frame - t0) / (t1 - t0);

        return Vector3.Lerp(keyPositions[i], keyPositions[Math.Min(i + 1, keyPositions.Count - 1)], alpha);
    }

    /// <summary>
    /// Samples rotation at a given frame using spherical linear interpolation.
    /// </summary>
    private static Quaternion SampleRotationAtFrame(
        ChunkController_905.CControllerInfo controller,
        ChunkController_905 animChunk,
        float frame)
    {
        if (!controller.HasRotTrack || animChunk.KeyTimes is null || animChunk.KeyRotations is null)
            return Quaternion.Identity;

        var keyTimes = animChunk.KeyTimes[controller.RotKeyTimeTrack];
        var keyRotations = animChunk.KeyRotations[controller.RotTrack];

        if (keyTimes.Count == 0 || keyRotations.Count == 0)
            return Quaternion.Identity;

        // Find surrounding keyframes
        int i = 0;
        while (i < keyTimes.Count - 1 && keyTimes[i + 1] <= frame)
            i++;

        if (i >= keyRotations.Count)
            return keyRotations[^1];

        if (i == keyTimes.Count - 1 || keyTimes[i] >= frame)
            return keyRotations[i];

        // Spherical linear interpolation
        float t0 = keyTimes[i];
        float t1 = keyTimes[i + 1];
        float alpha = (frame - t0) / (t1 - t0);

        return Quaternion.Slerp(keyRotations[i], keyRotations[Math.Min(i + 1, keyRotations.Count - 1)], alpha);
    }

    /// <summary>
    /// Cleans animation name for use as Collada ID.
    /// </summary>
    private static string CleanAnimationName(string name)
    {
        var cleanName = Path.GetFileNameWithoutExtension(name);
        // Replace invalid characters with underscores
        return cleanName.Replace(' ', '_').Replace('/', '_').Replace('\\', '_');
    }
}
