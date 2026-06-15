using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Renderers.Collada.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Animation;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Scene;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;
using CgfConverter.Renderers.Collada.Collada.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using static CgfConverter.Utilities.HelperMethods;

namespace CgfConverter.Renderers.Collada;

/// <summary>
/// ColladaModelRenderer partial class - Animation export support.
/// Handles both DBA animations (ChunkController_905) and CAF animations (CafAnimation model).
/// </summary>
public partial class ColladaModelRenderer
{
    /// <summary>
    /// Populates Library_Animations with all available animations (DBA + CAF).
    /// Each animation clip becomes a root ColladaAnimation containing per-bone sub-animations.
    /// Channel targets use the bone ID and "transform" SID, matching the joint nodes created in Skeleton.cs.
    /// </summary>
    public void WriteLibrary_Animations()
    {
        if (controllerIdToBoneName.Count == 0)
        {
            Log.D("No bone mappings available for animation export");
            return;
        }

        var allAnimations = new List<ColladaAnimation>();

        // --- DBA animations via ChunkController_905 ---
        foreach (var animChunk in _cryData.Animations
            .SelectMany(x => x.ChunkMap.Values.OfType<ChunkController_905>())
            .ToList())
        {
            if (animChunk?.Animations is null || animChunk.Animations.Count == 0)
                continue;

            foreach (var animation in animChunk.Animations)
            {
                var animName = CleanAnimationName(animation.Name);
                var boneAnims = CreateDbaClipAnimations(animation, animChunk, animName);
                if (boneAnims.Count > 0)
                {
                    allAnimations.Add(new ColladaAnimation
                    {
                        Name = animName,
                        ID = SanitizeId($"{animName}_anim"),
                        Animation = boneAnims.ToArray()
                    });
                    Log.D("DBA animation '{0}': {1} bone channels", animName, boneAnims.Count);
                }
            }
        }

        // --- CAF animations via CafAnimation ---
        foreach (var cafAnim in _cryData.CafAnimations)
        {
            var animName = CleanAnimationName(cafAnim.Name);
            var boneAnims = CreateCafClipAnimations(cafAnim, animName);
            if (boneAnims.Count > 0)
            {
                allAnimations.Add(new ColladaAnimation
                {
                    Name = animName,
                    ID = SanitizeId($"{animName}_anim"),
                    Animation = boneAnims.ToArray()
                });
                Log.D("CAF animation '{0}': {1} bone channels", animName, boneAnims.Count);
            }
        }

        if (allAnimations.Count > 0)
        {
            DaeObject.Library_Animations = new ColladaLibraryAnimations
            {
                Animation = allAnimations.ToArray()
            };
            Log.I("Exported {0} animation(s) to Collada library", allAnimations.Count);
        }
    }

    // -----------------------------------------------------------------------
    // DBA path (ChunkController_905)
    // -----------------------------------------------------------------------

    private List<ColladaAnimation> CreateDbaClipAnimations(
        ChunkController_905.Animation animation,
        ChunkController_905 animChunk,
        string animName)
    {
        var boneAnimations = new List<ColladaAnimation>();
        float secsPerTick = animation.MotionParams.SecsPerTick;

        // Fallback: if timing is zero (shouldn't happen but protect against it)
        if (secsPerTick <= 0f)
            secsPerTick = 1f / 4800f;

        foreach (var controller in animation.Controllers)
        {
            if (!controllerIdToBoneName.TryGetValue(controller.ControllerID, out var boneName))
                continue;

            if (!controller.HasPosTrack && !controller.HasRotTrack)
                continue;

            var (restPosition, restRotation) = controllerIdToRestTransform.TryGetValue(controller.ControllerID, out var rest)
                ? rest
                : (Vector3.Zero, Quaternion.Identity);

            var boneAnim = CreateDbaBoneAnimation(controller, animChunk, boneName, animName, secsPerTick, restPosition, restRotation);
            if (boneAnim is not null)
                boneAnimations.Add(boneAnim);
        }

        return boneAnimations;
    }

    private ColladaAnimation? CreateDbaBoneAnimation(
        ChunkController_905.CControllerInfo controller,
        ChunkController_905 animChunk,
        string boneName,
        string animName,
        float secsPerTick,
        Vector3 restPosition,
        Quaternion restRotation)
    {
        // Collect all unique key times from both tracks
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
        var timeValues = new StringBuilder();
        var matrixValues = new StringBuilder();

        foreach (var tick in frameList)
        {
            timeValues.Append($"{(tick - startFrame) * secsPerTick:F6} ");

            var position = SampleDbaPositionAtTick(controller, animChunk, tick, restPosition);
            var rotation = SampleDbaRotationAtTick(controller, animChunk, tick, restRotation);

            AppendMatrix(matrixValues, position, rotation);
        }

        return BuildBoneAnimation(animName, boneName, frameList.Count, timeValues, matrixValues);
    }

    private static Vector3 SampleDbaPositionAtTick(
        ChunkController_905.CControllerInfo controller,
        ChunkController_905 animChunk,
        float tick,
        Vector3 restPosition)
    {
        if (!controller.HasPosTrack || animChunk.KeyTimes is null || animChunk.KeyPositions is null)
            return restPosition;

        var keyTimes = animChunk.KeyTimes[controller.PosKeyTimeTrack];
        var keyPositions = animChunk.KeyPositions[controller.PosTrack];

        if (keyTimes.Count == 0 || keyPositions.Count == 0)
            return restPosition;

        return SampleVector3(keyTimes, keyPositions, tick, restPosition);
    }

    private static Quaternion SampleDbaRotationAtTick(
        ChunkController_905.CControllerInfo controller,
        ChunkController_905 animChunk,
        float tick,
        Quaternion restRotation)
    {
        if (!controller.HasRotTrack || animChunk.KeyTimes is null || animChunk.KeyRotations is null)
            return restRotation;

        var keyTimes = animChunk.KeyTimes[controller.RotKeyTimeTrack];
        var keyRotations = animChunk.KeyRotations[controller.RotTrack];

        if (keyTimes.Count == 0 || keyRotations.Count == 0)
            return restRotation;

        return SampleQuaternion(keyTimes, keyRotations, tick, restRotation);
    }

    // -----------------------------------------------------------------------
    // CAF path (CafAnimation)
    // -----------------------------------------------------------------------

    private List<ColladaAnimation> CreateCafClipAnimations(CafAnimation cafAnim, string animName)
    {
        var boneAnimations = new List<ColladaAnimation>();

        // secsPerFrame: converts frame number → seconds
        float secsPerFrame = cafAnim.TicksPerFrame * cafAnim.SecsPerTick;
        if (secsPerFrame <= 0f)
            secsPerFrame = 1f / 30f; // fallback: 30 fps

        foreach (var (controllerId, track) in cafAnim.BoneTracks)
        {
            if (!controllerIdToBoneName.TryGetValue(controllerId, out var boneName))
                continue;

            if (track.Positions.Count == 0 && track.Rotations.Count == 0)
                continue;

            var (restPosition, restRotation) = controllerIdToRestTransform.TryGetValue(controllerId, out var rest)
                ? rest
                : (Vector3.Zero, Quaternion.Identity);

            var boneAnim = CreateCafBoneAnimation(track, cafAnim, boneName, animName, secsPerFrame, restPosition, restRotation);
            if (boneAnim is not null)
                boneAnimations.Add(boneAnim);
        }

        return boneAnimations;
    }

    private ColladaAnimation? CreateCafBoneAnimation(
        BoneTrack track,
        CafAnimation cafAnim,
        string boneName,
        string animName,
        float secsPerFrame,
        Vector3 restPosition,
        Quaternion restRotation)
    {
        // Collect all unique frame times from both tracks
        var frameTimes = new SortedSet<float>();
        foreach (var t in track.RotationKeyTimes) frameTimes.Add(t);
        foreach (var t in track.PositionKeyTimes) frameTimes.Add(t);

        if (frameTimes.Count == 0)
            return null;

        var frameList = frameTimes.ToList();
        var startFrame = frameList[0];
        var timeValues = new StringBuilder();
        var matrixValues = new StringBuilder();

        foreach (var frame in frameList)
        {
            timeValues.Append($"{(frame - startFrame) * secsPerFrame:F6} ");

            var position = track.Positions.Count > 0
                ? SampleVector3(track.PositionKeyTimes, track.Positions, frame, restPosition)
                : restPosition;

            var rotation = track.Rotations.Count > 0
                ? SampleQuaternion(track.RotationKeyTimes, track.Rotations, frame, restRotation)
                : restRotation;

            // Handle additive animations: convert delta to absolute
            if (cafAnim.IsAdditive)
            {
                position = restPosition + position;
                rotation = Quaternion.Normalize(restRotation * rotation);
            }

            AppendMatrix(matrixValues, position, rotation);
        }

        return BuildBoneAnimation(animName, boneName, frameList.Count, timeValues, matrixValues);
    }

    // -----------------------------------------------------------------------
    // Shared helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Builds a ColladaAnimation for a single bone with float4x4 matrix output.
    /// Channel target is "{boneName}/transform" matching the joint node SID in Skeleton.cs.
    /// </summary>
    private static ColladaAnimation BuildBoneAnimation(
        string animName,
        string boneName,
        int keyframeCount,
        StringBuilder timeValues,
        StringBuilder matrixValues)
    {
        var animId = SanitizeId($"{animName}_{boneName}");

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

        var interpolationSource = new ColladaSource
        {
            ID = $"{animId}_interp",
            Name_Array = new ColladaNameArray
            {
                ID = $"{animId}_interp_array",
                Count = keyframeCount,
                Value_Pre_Parse = string.Join(" ", Enumerable.Repeat("LINEAR", keyframeCount))
            },
            Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = $"#{animId}_interp_array",
                    Count = (uint)keyframeCount,
                    Stride = 1,
                    Param = [new ColladaParam { Name = "INTERPOLATION", Type = "name" }]
                }
            }
        };

        var sampler = new ColladaSampler
        {
            ID = $"{animId}_sampler",
            Input =
            [
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.INPUT,         source = $"#{animId}_time" },
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.OUTPUT,        source = $"#{animId}_output" },
                new ColladaInputUnshared { Semantic = ColladaInputSemantic.INTERPOLATION, source = $"#{animId}_interp" }
            ]
        };

        var channel = new ColladaChannel
        {
            Source = $"#{animId}_sampler",
            Target = $"{boneName}/transform"
        };

        return new ColladaAnimation
        {
            ID = SanitizeId($"{animId}_bone_anim"),
            Name = $"{boneName}_transform",
            Source = [timeSource, outputSource, interpolationSource],
            Sampler = [sampler],
            Channel = [channel]
        };
    }

    /// <summary>
    /// Builds a column-vector transform matrix from position and rotation, then appends it.
    /// Translation is placed in column 4 (M14, M24, M34) — CryEngine / Collada column-vector convention.
    /// </summary>
    private static void AppendMatrix(StringBuilder sb, Vector3 position, Quaternion rotation)
    {
        var m = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(rotation));
        // Place translation in the 4th column (column-vector: v' = M * v)
        m.M14 = position.X;
        m.M24 = position.Y;
        m.M34 = position.Z;
        sb.Append(CreateStringFromMatrix4x4(m));
        sb.Append(' ');
    }

    /// <summary>
    /// Linearly interpolates a Vector3 value from a sorted key-time / key-value array.
    /// </summary>
    private static Vector3 SampleVector3(
        IList<float> keyTimes,
        IList<Vector3> keyValues,
        float t,
        Vector3 fallback)
    {
        if (keyTimes.Count == 0 || keyValues.Count == 0) return fallback;
        if (t <= keyTimes[0]) return keyValues[0];
        if (t >= keyTimes[^1]) return keyValues[Math.Min(keyTimes.Count - 1, keyValues.Count - 1)];

        int i = 0;
        while (i < keyTimes.Count - 1 && keyTimes[i + 1] <= t)
            i++;

        if (i + 1 >= keyValues.Count) return keyValues[^1];

        float t0 = keyTimes[i], t1 = keyTimes[i + 1];
        float alpha = (t - t0) / (t1 - t0);
        return Vector3.Lerp(keyValues[i], keyValues[i + 1], alpha);
    }

    /// <summary>
    /// Spherically interpolates a Quaternion from a sorted key-time / key-value array.
    /// </summary>
    private static Quaternion SampleQuaternion(
        IList<float> keyTimes,
        IList<Quaternion> keyValues,
        float t,
        Quaternion fallback)
    {
        if (keyTimes.Count == 0 || keyValues.Count == 0) return fallback;
        if (t <= keyTimes[0]) return keyValues[0];
        if (t >= keyTimes[^1]) return keyValues[Math.Min(keyTimes.Count - 1, keyValues.Count - 1)];

        int i = 0;
        while (i < keyTimes.Count - 1 && keyTimes[i + 1] <= t)
            i++;

        if (i + 1 >= keyValues.Count) return keyValues[^1];

        float t0 = keyTimes[i], t1 = keyTimes[i + 1];
        float alpha = (t - t0) / (t1 - t0);
        return Quaternion.Slerp(keyValues[i], keyValues[i + 1], alpha);
    }

    /// <summary>
    /// Cleans an animation name for use as a base display name.
    /// </summary>
    private static string CleanAnimationName(string name)
    {
        return Path.GetFileNameWithoutExtension(name);
    }

    /// <summary>
    /// Converts a string into a valid Collada/XML ID (no spaces, no leading digits, no '$').
    /// </summary>
    private static string SanitizeId(string id)
    {
        var sb = new StringBuilder(id.Length);
        for (int i = 0; i < id.Length; i++)
        {
            char c = id[i];
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')
                sb.Append(c);
            else
                sb.Append('_');
        }
        // XML IDs must not start with a digit or hyphen
        if (sb.Length > 0 && (char.IsDigit(sb[0]) || sb[0] == '-'))
            sb.Insert(0, '_');
        return sb.Length > 0 ? sb.ToString() : "_anim";
    }

    // -----------------------------------------------------------------------
    // Per-animation file export (Blender NLA workflow)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Exports one .dae file per animation clip for Blender's NLA workflow.
    /// Blender merges all animations into one action when importing a single file,
    /// so each clip gets its own file containing skeleton + that animation only.
    /// Requires WriteLibrary_Animations() to have run first.
    /// </summary>
    private void ExportAnimationFiles()
    {
        if (DaeObject.Library_Animations?.Animation is not { Length: > 0 } clips)
            return;

        if (DaeObject.Library_Visual_Scene?.Visual_Scene is not { Length: > 0 })
            return;

        var baseName = Path.GetFileNameWithoutExtension(daeOutputFile.FullName);
        var outputDir = daeOutputFile.DirectoryName ?? ".";

        Log.D("Exporting {0} separate animation file(s) for Blender NLA workflow", clips.Length);

        foreach (var clip in clips)
        {
            var animFilePath = Path.Combine(outputDir, $"{baseName}_anim_{clip.Name}.dae");

            var animDoc = new ColladaDoc
            {
                Collada_Version = "1.4.1",
                Asset = new ColladaAsset
                {
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    Up_Axis = "Z_UP",
                    Unit = new ColladaAssetUnit { Meter = 1.0, Name = "meter" },
                    Title = clip.Name
                },
                Scene = new ColladaScene
                {
                    Visual_Scene = new ColladaInstanceVisualScene { URL = "#Scene" }
                },
                Library_Visual_Scene = DaeObject.Library_Visual_Scene,
                Library_Animations = new ColladaLibraryAnimations
                {
                    Animation = [clip]
                }
            };

            using var writer = new System.IO.StreamWriter(animFilePath);
            serializer.Serialize(writer, animDoc);
            Log.D("  {0}", Path.GetFileName(animFilePath));
        }
    }
}
