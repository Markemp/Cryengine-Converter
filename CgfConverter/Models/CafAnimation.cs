using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.Models;

/// <summary>
/// Represents a single animation from a CAF file.
/// Aggregates all bone controllers into a unified animation structure.
/// </summary>
public class CafAnimation
{
    /// <summary>Name of the animation (from chrparams or filename).</summary>
    public required string Name { get; init; }

    /// <summary>Source file path.</summary>
    public required string FilePath { get; init; }

    /// <summary>Seconds per tick from timing chunk.</summary>
    public float SecsPerTick { get; set; } = 1f / 4800f; // Default: 4800 ticks/sec

    /// <summary>Ticks per frame from timing chunk.</summary>
    public int TicksPerFrame { get; set; } = 160; // Default: 30fps

    /// <summary>Start frame of the animation.</summary>
    public int StartFrame { get; set; }

    /// <summary>End frame of the animation.</summary>
    public int EndFrame { get; set; }

    /// <summary>
    /// Whether this is an additive animation (stores deltas from rest pose).
    /// Additive animations need to be converted to absolute transforms for export.
    /// </summary>
    public bool IsAdditive { get; set; }

    /// <summary>
    /// Per-bone animation tracks, keyed by controller ID (CRC32 of bone name).
    /// </summary>
    public Dictionary<uint, BoneTrack> BoneTracks { get; } = [];

    /// <summary>
    /// Mapping from controller ID to bone name (from BoneNameList chunk).
    /// </summary>
    public Dictionary<uint, string> ControllerIdToBoneName { get; } = [];

    /// <summary>
    /// Gets the duration in frames.
    /// </summary>
    public int DurationFrames => EndFrame - StartFrame;

    /// <summary>
    /// Gets the duration in seconds.
    /// </summary>
    public float DurationSeconds => DurationFrames * TicksPerFrame * SecsPerTick;
}

/// <summary>
/// Animation track for a single bone.
/// </summary>
public class BoneTrack
{
    /// <summary>Controller ID (CRC32 of bone name).</summary>
    public uint ControllerId { get; init; }

    /// <summary>Rotation keyframe times (in ticks or frames depending on source).</summary>
    public List<float> RotationKeyTimes { get; init; } = [];

    /// <summary>Position keyframe times (in ticks or frames depending on source).</summary>
    public List<float> PositionKeyTimes { get; init; } = [];

    /// <summary>Position keyframes.</summary>
    public List<Vector3> Positions { get; init; } = [];

    /// <summary>Rotation keyframes.</summary>
    public List<Quaternion> Rotations { get; init; } = [];

    /// <summary>Scale keyframe times (in ticks or frames depending on source).</summary>
    public List<float> ScaleKeyTimes { get; init; } = [];

    /// <summary>Scale keyframes (diagonal scale as Vector3 x, y, z).</summary>
    public List<Vector3> Scales { get; init; } = [];

    /// <summary>
    /// Legacy property for backward compatibility. Returns rotation key times.
    /// </summary>
    public List<float> KeyTimes => RotationKeyTimes.Count > 0 ? RotationKeyTimes : PositionKeyTimes;
}
