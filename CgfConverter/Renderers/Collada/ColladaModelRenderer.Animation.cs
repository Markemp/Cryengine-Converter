using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Collada.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Animation;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Scene;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Collada.Collada.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using static CgfConverter.Utilities.HelperMethods;

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
    /// Uses float4x4 matrices with channel target "/transform" for Blender compatibility.
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

            // Get rest transform for this bone (fallback when no position/rotation track exists)
            var (restPosition, restRotation) = controllerIdToRestTransform.TryGetValue(controller.ControllerID, out var rest)
                ? rest
                : (Vector3.Zero, Quaternion.Identity);

            var boneAnim = CreateBoneMatrixAnimation(controller, animChunk, boneName, animationName, restPosition, restRotation);
            if (boneAnim is not null)
            {
                boneAnimations.Add(boneAnim);
            }
        }

        return boneAnimations;
    }

    /// <summary>
    /// Creates a matrix-based animation for a single bone.
    /// Outputs float4x4 matrices targeting the bone's "transform" SID.
    /// </summary>
    private ColladaAnimation? CreateBoneMatrixAnimation(
        ChunkController_905.CControllerInfo controller,
        ChunkController_905 animChunk,
        string boneName,
        string animationName,
        Vector3 restPosition,
        Quaternion restRotation)
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
        var startFrame = frameList[0];
        var keyframeCount = frameList.Count;

        // Build time and matrix output strings
        var timeValues = new StringBuilder();
        var matrixValues = new StringBuilder();

        foreach (var frame in frameList)
        {
            // Time in seconds (normalized to start at 0)
            var timeInSeconds = (frame - startFrame) / 30f;
            timeValues.Append($"{timeInSeconds:F6} ");

            // Sample position and rotation at this frame, using rest pose as fallback
            var position = SamplePositionAtFrame(controller, animChunk, frame, restPosition);
            var rotation = SampleRotationAtFrame(controller, animChunk, frame, restRotation);

            // Build transform matrix from position and rotation
            // IMPORTANT: CryEngine stores translation in column 4 (M14, M24, M34), NOT row 4
            // Do NOT use matrix.Translation which sets M41, M42, M43
            var matrix = Matrix4x4.CreateFromQuaternion(rotation);
            matrix.M14 = position.X;
            matrix.M24 = position.Y;
            matrix.M34 = position.Z;

            // Append matrix values (row-major for Collada)
            matrixValues.Append(CreateStringFromMatrix4x4(matrix) + " ");
        }

        var animId = $"{animationName}_{boneName}";

        // Time source
        var timeSource = new ColladaSource
        {
            ID = $"{animId}_time",
            Float_Array = new ColladaFloatArray
            {
                ID = $"{animId}_time_array",
                Count = keyframeCount,
                Value_As_String = timeValues.ToString().TrimEnd()
            },
            Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = $"#{animId}_time_array",
                    Count = (uint)keyframeCount,
                    Stride = 1,
                    Param = [new ColladaParam { Name = "TIME", Type = "float" }]
                }
            }
        };

        // Output source (float4x4 matrices)
        var outputSource = new ColladaSource
        {
            ID = $"{animId}_output",
            Float_Array = new ColladaFloatArray
            {
                ID = $"{animId}_output_array",
                Count = keyframeCount * 16,
                Value_As_String = matrixValues.ToString().TrimEnd()
            },
            Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = $"#{animId}_output_array",
                    Count = (uint)keyframeCount,
                    Stride = 16,
                    Param = [new ColladaParam { Name = "TRANSFORM", Type = "float4x4" }]
                }
            }
        };

        // Interpolation source
        var interpolationSource = new ColladaSource
        {
            ID = $"{animId}_interpolation",
            Name_Array = new ColladaNameArray
            {
                ID = $"{animId}_interpolation_array",
                Count = keyframeCount,
                Value_Pre_Parse = string.Join(" ", Enumerable.Repeat("LINEAR", keyframeCount))
            },
            Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = $"#{animId}_interpolation_array",
                    Count = (uint)keyframeCount,
                    Stride = 1,
                    Param = [new ColladaParam { Name = "INTERPOLATION", Type = "name" }]
                }
            }
        };

        // Sampler
        var sampler = new ColladaSampler
        {
            ID = $"{animId}_sampler",
            Input =
            [
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.INPUT, source = $"#{animId}_time" },
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.OUTPUT, source = $"#{animId}_output" },
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.INTERPOLATION, source = $"#{animId}_interpolation" }
            ]
        };

        // Channel targeting the bone's transform SID (critical: use /transform not /matrix)
        var channel = new ColladaChannel
        {
            Source = $"#{animId}_sampler",
            Target = $"{boneName}/transform"
        };

        return new ColladaAnimation
        {
            ID = $"{animId}_animation",
            Name = $"{boneName}_transform",
            Source = [timeSource, outputSource, interpolationSource],
            Sampler = [sampler],
            Channel = [channel]
        };
    }

    /// <summary>
    /// Samples position at a given frame using linear interpolation.
    /// Falls back to rest position if no position track exists.
    /// </summary>
    private static Vector3 SamplePositionAtFrame(
        ChunkController_905.CControllerInfo controller,
        ChunkController_905 animChunk,
        float frame,
        Vector3 restPosition)
    {
        if (!controller.HasPosTrack || animChunk.KeyTimes is null || animChunk.KeyPositions is null)
            return restPosition;

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
    /// Falls back to rest rotation if no rotation track exists.
    /// </summary>
    private static Quaternion SampleRotationAtFrame(
        ChunkController_905.CControllerInfo controller,
        ChunkController_905 animChunk,
        float frame,
        Quaternion restRotation)
    {
        if (!controller.HasRotTrack || animChunk.KeyTimes is null || animChunk.KeyRotations is null)
            return restRotation;

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

    /// <summary>
    /// Exports individual animation files for Blender NLA workflow.
    /// Blender's Collada importer merges all animations into one action, so we export
    /// each animation as a separate file with skeleton + that animation only.
    /// </summary>
    private void ExportAnimationFiles()
    {
        if (_cryData.Animations is null || _cryData.Animations.Count == 0)
            return;

        if (controllerIdToBoneName.Count == 0)
            return;

        // Collect all animations with their data
        var allAnimations = new List<(string name, ColladaAnimation animation)>();

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
                    var rootAnimation = new ColladaAnimation
                    {
                        Name = animationName,
                        ID = $"{animationName}_animation",
                        Animation = boneAnimations.ToArray()
                    };
                    allAnimations.Add((animationName, rootAnimation));
                }
            }
        }

        // Skip if no animations
        if (allAnimations.Count == 0)
            return;

        Log.D($"Exporting {allAnimations.Count} separate animation files for Blender NLA workflow...");

        var baseFileName = Path.GetFileNameWithoutExtension(daeOutputFile.FullName);
        var outputDir = daeOutputFile.DirectoryName ?? ".";

        foreach (var (animName, animation) in allAnimations)
        {
            var animFileName = $"{baseFileName}_anim_{animName}.dae";
            var animFilePath = Path.Combine(outputDir, animFileName);

            var animDoc = CreateAnimationOnlyDoc(animation);

            using (var writer = new StreamWriter(animFilePath))
            {
                serializer.Serialize(writer, animDoc);
            }

            Log.D($"  Exported: {animFileName}");
        }
    }

    /// <summary>
    /// Creates a minimal Collada document containing only skeleton + single animation.
    /// This is for Blender import workflow where each animation needs its own file.
    /// </summary>
    private ColladaDoc CreateAnimationOnlyDoc(ColladaAnimation animation)
    {
        var animDoc = new ColladaDoc
        {
            Collada_Version = "1.4.1"
        };

        // Asset metadata
        animDoc.Asset = new ColladaAsset
        {
            Created = DateTime.Now,
            Modified = DateTime.Now,
            Up_Axis = "Z_UP",
            Unit = new ColladaAssetUnit { Meter = 1.0, Name = "meter" },
            Title = animation.Name
        };

        // Scene reference
        animDoc.Scene = new ColladaScene
        {
            Visual_Scene = new ColladaInstanceVisualScene { URL = "#Scene" }
        };

        // Copy the skeleton structure from the main document's visual scene
        // We need the skeleton nodes for the animation to target
        if (DaeObject.Library_Visual_Scene?.Visual_Scene?.Length > 0)
        {
            animDoc.Library_Visual_Scene = new ColladaLibraryVisualScenes
            {
                Visual_Scene = DaeObject.Library_Visual_Scene.Visual_Scene
            };
        }

        // Add just this animation
        animDoc.Library_Animations = new ColladaLibraryAnimations
        {
            Animation = new[] { animation }
        };

        return animDoc;
    }
}
