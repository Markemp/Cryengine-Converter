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
        var animationClips = new List<ColladaAnimationClip>();

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
                var (boneAnimations, startTime, endTime) = CreateDecomposedAnimationsForClip(animation, animChunk, animationName);

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

                    // Create animation clip that references this animation
                    var clip = new ColladaAnimationClip
                    {
                        ID = $"{animationName}_clip",
                        Name = animationName,
                        Start = startTime,
                        End = endTime,
                        Instance_Animation = [new ColladaInstanceAnimation { URL = $"#{animationName}_animation" }]
                    };
                    animationClips.Add(clip);

                    Log.D($"Created animation: {animationName} with {boneAnimations.Count} bone channels ({startTime:F2}s - {endTime:F2}s)");
                }
            }
        }

        if (allAnimations.Count > 0)
        {
            animationLibrary.Animation = allAnimations.ToArray();
            DaeObject.Library_Animations = animationLibrary;

            // Add animation clips library so Blender can recognize separate actions
            DaeObject.Library_Animation_Clips = new ColladaLibraryAnimationClips
            {
                Animation_Clip = animationClips.ToArray()
            };

            Log.I($"Exported {allAnimations.Count} animation(s)");
        }
    }

    /// <summary>
    /// Creates decomposed animations for all bones in a single animation clip.
    /// Returns the bone animations along with the start and end times in seconds.
    /// </summary>
    private (List<ColladaAnimation> animations, double startTime, double endTime) CreateDecomposedAnimationsForClip(
        ChunkController_905.Animation animation,
        ChunkController_905 animChunk,
        string animationName)
    {
        var boneAnimations = new List<ColladaAnimation>();
        double minTime = double.MaxValue;
        double maxTime = double.MinValue;

        foreach (var controller in animation.Controllers)
        {
            if (!controllerIdToBoneName.TryGetValue(controller.ControllerID, out var boneName))
            {
                Log.D($"Animation[{animationName}]: Controller 0x{controller.ControllerID:X08} not found in skeleton");
                continue;
            }

            if (!controller.HasPosTrack && !controller.HasRotTrack)
                continue;

            var (componentAnims, boneStartTime, boneEndTime) = CreateBoneDecomposedAnimations(controller, animChunk, boneName, animationName);
            if (componentAnims.Count > 0)
            {
                boneAnimations.AddRange(componentAnims);
                minTime = Math.Min(minTime, boneStartTime);
                maxTime = Math.Max(maxTime, boneEndTime);
            }
        }

        // Default to 0 if no animations found
        if (minTime == double.MaxValue) minTime = 0;
        if (maxTime == double.MinValue) maxTime = 0;

        return (boneAnimations, minTime, maxTime);
    }

    /// <summary>
    /// Creates decomposed animations for a single bone (location + rotation channels).
    /// Blender's Collada importer requires decomposed transforms, not matrices.
    /// Returns the animations and the start/end times in seconds.
    /// </summary>
    private (List<ColladaAnimation> animations, double startTime, double endTime) CreateBoneDecomposedAnimations(
        ChunkController_905.CControllerInfo controller,
        ChunkController_905 animChunk,
        string boneName,
        string animationName)
    {
        var animations = new List<ColladaAnimation>();

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
            return (animations, 0, 0);

        var frameList = frameTimes.ToList();
        var startFrame = frameList[0];
        var endFrame = frameList[^1];

        // Convert frames to seconds at 30fps
        double startTimeSeconds = 0;  // Animation starts at 0
        double endTimeSeconds = (endFrame - startFrame) / 30.0;

        // Sample position and rotation at each frame
        var positions = new List<Vector3>();
        var rotations = new List<Vector3>();  // Euler angles in degrees

        foreach (var frame in frameList)
        {
            var position = SamplePositionAtFrame(controller, animChunk, frame);
            var rotation = SampleRotationAtFrame(controller, animChunk, frame);
            var euler = QuaternionToEulerDegrees(rotation);

            positions.Add(position);
            rotations.Add(euler);
        }

        var sourceIdBase = $"{animationName}_{boneName}";

        // Create shared time source
        var timeValues = string.Join(" ", frameList.Select(t => ((t - startFrame) / 30f).ToString("F6")));

        // Create location animations (X, Y, Z)
        if (controller.HasPosTrack)
        {
            animations.Add(CreateComponentAnimation(sourceIdBase, "location_X", boneName, "location.X",
                frameList.Count, timeValues, positions.Select(p => p.X)));
            animations.Add(CreateComponentAnimation(sourceIdBase, "location_Y", boneName, "location.Y",
                frameList.Count, timeValues, positions.Select(p => p.Y)));
            animations.Add(CreateComponentAnimation(sourceIdBase, "location_Z", boneName, "location.Z",
                frameList.Count, timeValues, positions.Select(p => p.Z)));
        }

        // Create rotation animations (rotationX.ANGLE, rotationY.ANGLE, rotationZ.ANGLE)
        if (controller.HasRotTrack)
        {
            animations.Add(CreateComponentAnimation(sourceIdBase, "rotationX", boneName, "rotationX.ANGLE",
                frameList.Count, timeValues, rotations.Select(r => r.X)));
            animations.Add(CreateComponentAnimation(sourceIdBase, "rotationY", boneName, "rotationY.ANGLE",
                frameList.Count, timeValues, rotations.Select(r => r.Y)));
            animations.Add(CreateComponentAnimation(sourceIdBase, "rotationZ", boneName, "rotationZ.ANGLE",
                frameList.Count, timeValues, rotations.Select(r => r.Z)));
        }

        return (animations, startTimeSeconds, endTimeSeconds);
    }

    /// <summary>
    /// Creates a single-component animation (e.g., location.X or rotationZ.ANGLE).
    /// </summary>
    private static ColladaAnimation CreateComponentAnimation(
        string sourceIdBase,
        string componentName,
        string boneName,
        string targetPath,
        int keyframeCount,
        string timeValues,
        IEnumerable<float> outputValues)
    {
        var componentId = $"{sourceIdBase}_{componentName}";

        // Time source
        var timeSource = new ColladaSource
        {
            ID = $"{componentId}_time",
            Name = $"{componentId}_time",
            Float_Array = new ColladaFloatArray
            {
                ID = $"{componentId}_time_array",
                Count = keyframeCount,
                Value_As_String = timeValues
            },
            Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = $"#{componentId}_time_array",
                    Count = (uint)keyframeCount,
                    Stride = 1,
                    Param = [new ColladaParam { Name = "TIME", Type = "float" }]
                }
            }
        };

        // Output source (single float per keyframe)
        var outputSource = new ColladaSource
        {
            ID = $"{componentId}_output",
            Name = $"{componentId}_output",
            Float_Array = new ColladaFloatArray
            {
                ID = $"{componentId}_output_array",
                Count = keyframeCount,
                Value_As_String = string.Join(" ", outputValues.Select(v => v.ToString("F6")))
            },
            Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = $"#{componentId}_output_array",
                    Count = (uint)keyframeCount,
                    Stride = 1,
                    Param = [new ColladaParam { Name = "VALUE", Type = "float" }]
                }
            }
        };

        // Interpolation source
        var interpolationSource = new ColladaSource
        {
            ID = $"{componentId}_interpolation",
            Name = $"{componentId}_interpolation",
            Name_Array = new ColladaNameArray
            {
                ID = $"{componentId}_interpolation_array",
                Count = keyframeCount,
                Value_Pre_Parse = string.Join(" ", Enumerable.Repeat("LINEAR", keyframeCount))
            },
            Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = $"#{componentId}_interpolation_array",
                    Count = (uint)keyframeCount,
                    Stride = 1,
                    Param = [new ColladaParam { Name = "INTERPOLATION", Type = "name" }]
                }
            }
        };

        // Sampler
        var sampler = new ColladaSampler
        {
            ID = $"{componentId}_sampler",
            Input =
            [
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.INPUT, source = $"#{componentId}_time" },
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.OUTPUT, source = $"#{componentId}_output" },
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.INTERPOLATION, source = $"#{componentId}_interpolation" }
            ]
        };

        // Channel targeting the bone's decomposed transform
        var channel = new ColladaChannel
        {
            Source = $"#{componentId}_sampler",
            Target = $"{boneName}/{targetPath}"
        };

        return new ColladaAnimation
        {
            ID = $"{componentId}_animation",
            Name = $"{boneName}_{componentName}",
            Source = [timeSource, outputSource, interpolationSource],
            Sampler = [sampler],
            Channel = [channel]
        };
    }

    /// <summary>
    /// Converts a quaternion to Euler angles in degrees (XYZ order).
    /// </summary>
    private static Vector3 QuaternionToEulerDegrees(Quaternion q)
    {
        // Convert quaternion to Euler angles (radians)
        // Using ZYX rotation order (which corresponds to XYZ when reading the angles)
        float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
        float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        float rollX = MathF.Atan2(sinr_cosp, cosr_cosp);

        float sinp = 2 * (q.W * q.Y - q.Z * q.X);
        float pitchY;
        if (MathF.Abs(sinp) >= 1)
            pitchY = MathF.CopySign(MathF.PI / 2, sinp); // Use 90 degrees if out of range
        else
            pitchY = MathF.Asin(sinp);

        float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
        float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        float yawZ = MathF.Atan2(siny_cosp, cosy_cosp);

        // Convert to degrees
        const float radToDeg = 180f / MathF.PI;
        return new Vector3(rollX * radToDeg, pitchY * radToDeg, yawZ * radToDeg);
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
