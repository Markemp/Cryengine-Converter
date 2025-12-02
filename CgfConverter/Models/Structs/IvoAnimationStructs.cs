using System.Numerics;

namespace CgfConverter.Models.Structs;

/// <summary>
/// Block header for #caf and #dba animation blocks (12 bytes).
/// </summary>
public struct IvoAnimBlockHeader
{
    /// <summary>Signature: "#caf" or "#dba".</summary>
    public string Signature { get; set; }

    /// <summary>Number of bones in this animation.</summary>
    public byte BoneCount { get; set; }

    /// <summary>Padding byte (always 0).</summary>
    public byte Padding { get; set; }

    /// <summary>Magic number (0xAA55).</summary>
    public ushort Magic { get; set; }

    /// <summary>Total size of block data after header.</summary>
    public uint DataSize { get; set; }
}

/// <summary>
/// Controller entry for per-bone animation data (24 bytes).
/// Format flags: 0x80xx = rotation only, 0xC0xx = has position.
/// Low byte: 0x42 = SmallTree48BitQuat, 0x43 = SmallTree64BitQuat, 0x00 = NoCompressQuat.
/// </summary>
public struct IvoAnimControllerEntry
{
    /// <summary>Number of keyframes.</summary>
    public byte NumKeys { get; set; }

    /// <summary>Padding byte (always 0).</summary>
    public byte Padding { get; set; }

    /// <summary>Format flags (high byte = has pos, low byte = compression format).</summary>
    public ushort FormatFlags { get; set; }

    /// <summary>Offset to rotation data (relative to block start).</summary>
    public uint RotDataOffset { get; set; }

    /// <summary>Offset to rotation time keys.</summary>
    public uint RotTimeOffset { get; set; }

    /// <summary>Offset to position data (0 if rotation only).</summary>
    public uint PosDataOffset { get; set; }

    /// <summary>Offset to position time keys (0 if rotation only).</summary>
    public uint PosTimeOffset { get; set; }

    /// <summary>Reserved/padding.</summary>
    public uint Reserved { get; set; }

    /// <summary>Returns true if this controller has position data.</summary>
    public readonly bool HasPosition => (FormatFlags & 0xC000) == 0xC000;

    /// <summary>Gets the compression format from the low byte.</summary>
    public readonly byte CompressionFormat => (byte)(FormatFlags & 0xFF);
}

/// <summary>
/// DBA metadata entry for a single animation in the library (44 bytes).
/// </summary>
public struct IvoDBAMetaEntry
{
    /// <summary>Number of keyframes.</summary>
    public ushort NumKeys { get; set; }

    /// <summary>Number of bones.</summary>
    public ushort BoneCount { get; set; }

    /// <summary>Animation flags.</summary>
    public uint Flags { get; set; }

    /// <summary>Length of the path string.</summary>
    public uint PathLength { get; set; }

    /// <summary>Start position.</summary>
    public Vector3 StartPosition { get; set; }

    /// <summary>Start rotation.</summary>
    public Quaternion StartRotation { get; set; }

    /// <summary>Padding/reserved.</summary>
    public uint Padding { get; set; }
}

/// <summary>
/// Parsed animation block from #caf or #dba data.
/// </summary>
public class IvoAnimationBlock
{
    /// <summary>Block header.</summary>
    public IvoAnimBlockHeader Header { get; set; }

    /// <summary>Bone hash array (CRC32 of bone names).</summary>
    public uint[] BoneHashes { get; set; } = [];

    /// <summary>Controller entries for each bone.</summary>
    public IvoAnimControllerEntry[] Controllers { get; set; } = [];

    /// <summary>Raw keyframe data.</summary>
    public byte[] KeyframeData { get; set; } = [];
}
