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
    /// </summary>
    private List<UsdPrim> CreateAnimations(Dictionary<uint, string> controllerIdToJointPath)
    {
        var animations = new List<UsdPrim>();

        if (_cryData.Animations is null || _cryData.Animations.Count == 0)
        {
            Log.D("No animations found in CryEngine data");
            return animations;
        }

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
                    var skelAnim = CreateSkelAnimation(anim, animChunk, controllerIdToJointPath, name);
                    if (skelAnim is not null)
                    {
                        animations.Add(skelAnim);
                        Log.D($"Created animation: {name}");
                    }
                }
            }
        }

        Log.D($"Created {animations.Count} animation(s)");
        return animations;
    }

    /// <summary>
    /// Creates a single SkelAnimation prim from a CryEngine Animation struct.
    /// </summary>
    private UsdSkelAnimation? CreateSkelAnimation(
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
            return null;
        }

        // Collect all unique time values across all tracks (normalized to seconds at 30fps)
        var allTimes = new SortedSet<float>();
        foreach (var controller in anim.Controllers)
        {
            if (controller.HasPosTrack && animChunk.KeyTimes is not null)
            {
                var times = animChunk.KeyTimes[controller.PosKeyTimeTrack];
                var startTime = times.Count > 0 ? times[0] : 0;
                foreach (var t in times)
                    allTimes.Add((t - startTime) / 30f);
            }
            if (controller.HasRotTrack && animChunk.KeyTimes is not null)
            {
                var times = animChunk.KeyTimes[controller.RotKeyTimeTrack];
                var startTime = times.Count > 0 ? times[0] : 0;
                foreach (var t in times)
                    allTimes.Add((t - startTime) / 30f);
            }
        }

        if (allTimes.Count == 0)
        {
            Log.W($"Animation[{animName}]: No keyframes found");
            return null;
        }

        // Build time-sampled arrays
        // USD expects all joints' values at each time sample
        var translationSamples = new SortedDictionary<float, List<Vector3>>();
        var rotationSamples = new SortedDictionary<float, List<Quaternion>>();

        foreach (var time in allTimes)
        {
            var translations = new List<Vector3>();
            var rotations = new List<Quaternion>();

            foreach (var jointPath in animatedJoints)
            {
                var controller = controllersByJointPath[jointPath];

                // Get translation for this joint at this time
                Vector3 translation = Vector3.Zero;
                if (controller.HasPosTrack && animChunk.KeyTimes is not null && animChunk.KeyPositions is not null)
                {
                    translation = SamplePosition(
                        animChunk.KeyTimes[controller.PosKeyTimeTrack],
                        animChunk.KeyPositions[controller.PosTrack],
                        time);
                }
                translations.Add(translation);

                // Get rotation for this joint at this time
                Quaternion rotation = Quaternion.Identity;
                if (controller.HasRotTrack && animChunk.KeyTimes is not null && animChunk.KeyRotations is not null)
                {
                    rotation = SampleRotation(
                        animChunk.KeyTimes[controller.RotKeyTimeTrack],
                        animChunk.KeyRotations[controller.RotTrack],
                        time);
                }
                rotations.Add(rotation);
            }

            translationSamples[time] = translations;
            rotationSamples[time] = rotations;
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

        return skelAnim;
    }

    /// <summary>
    /// Samples position at a given time using linear interpolation.
    /// </summary>
    private Vector3 SamplePosition(List<float> keyTimes, List<Vector3> keyPositions, float time)
    {
        if (keyTimes.Count == 0 || keyPositions.Count == 0)
            return Vector3.Zero;

        // Normalize times (same as glTF renderer)
        float startTime = keyTimes[0];
        var normalizedTimes = keyTimes.Select(t => (t - startTime) / 30f).ToList();

        // Find surrounding keyframes
        int i = 0;
        while (i < normalizedTimes.Count - 1 && normalizedTimes[i + 1] <= time)
            i++;

        if (i >= keyPositions.Count)
            return keyPositions[^1];

        if (i == normalizedTimes.Count - 1 || normalizedTimes[i] >= time)
            return keyPositions[i];

        // Linear interpolation
        float t0 = normalizedTimes[i];
        float t1 = normalizedTimes[i + 1];
        float alpha = (time - t0) / (t1 - t0);

        return Vector3.Lerp(keyPositions[i], keyPositions[Math.Min(i + 1, keyPositions.Count - 1)], alpha);
    }

    /// <summary>
    /// Samples rotation at a given time using spherical linear interpolation.
    /// </summary>
    private Quaternion SampleRotation(List<float> keyTimes, List<Quaternion> keyRotations, float time)
    {
        if (keyTimes.Count == 0 || keyRotations.Count == 0)
            return Quaternion.Identity;

        // Normalize times (same as glTF renderer)
        float startTime = keyTimes[0];
        var normalizedTimes = keyTimes.Select(t => (t - startTime) / 30f).ToList();

        // Find surrounding keyframes
        int i = 0;
        while (i < normalizedTimes.Count - 1 && normalizedTimes[i + 1] <= time)
            i++;

        if (i >= keyRotations.Count)
            return keyRotations[^1];

        if (i == normalizedTimes.Count - 1 || normalizedTimes[i] >= time)
            return keyRotations[i];

        // Spherical linear interpolation
        float t0 = normalizedTimes[i];
        float t1 = normalizedTimes[i + 1];
        float alpha = (time - t0) / (t1 - t0);

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
}
