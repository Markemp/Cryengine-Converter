using System.Numerics;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// Animation info chunk for Star Citizen #ivo CAF files.
/// Chunk ID: 0x4733C6ED (IvoAnimInfo)
/// Contains animation metadata: FPS, bone count, end frame, and reference pose.
/// Size: 48 bytes.
/// </summary>
public class ChunkIvoAnimInfo : Chunk
{
    /// <summary>Animation flags (0=normal, 2=pose?).</summary>
    public uint Flags { get; set; }

    /// <summary>Frames per second - typically 30.</summary>
    public ushort FramesPerSecond { get; set; }

    /// <summary>Number of bones in the animation.</summary>
    public ushort NumBones { get; set; }

    /// <summary>Reserved/unknown value (usually 0).</summary>
    public uint Reserved { get; set; }

    /// <summary>Last frame number (matches timeEnd in time keys).</summary>
    public uint EndFrame { get; set; }

    /// <summary>Reference pose rotation.</summary>
    public Quaternion StartRotation { get; set; }

    /// <summary>Reference pose position.</summary>
    public Vector3 StartPosition { get; set; }

    /// <summary>Padding/reserved.</summary>
    public uint Padding { get; set; }
}
