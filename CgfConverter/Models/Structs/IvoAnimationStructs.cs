using System.Numerics;

namespace CgfConverter.Models.Structs;

/// <summary>
/// Block header for #caf and #dba animation blocks (12 bytes).
/// </summary>
public struct IvoAnimBlockHeader
{
    /// <summary>Signature: "#caf" or "#dba".</summary>
    public string Signature { get; set; }

    /// <summary>Number of bones in this animation (CryEngine supports up to 1024).</summary>
    public ushort BoneCount { get; set; }

    /// <summary>Magic number (0xAA55 for DBA, 0xFFFF for CAF).</summary>
    public ushort Magic { get; set; }

    /// <summary>Total size of block data after header.</summary>
    public uint DataSize { get; set; }
}

/// <summary>
/// Controller entry for per-bone animation data (24 bytes).
/// Structure has separate rotation track (12 bytes) and position track (12 bytes).
/// All offsets are relative to the START of this controller entry (not the keyframe data start).
/// </summary>
/// <remarks>
/// Format flags breakdown (low nibble determines time format: 0=ubyte, 2=uint16 header):
/// - Rotation: 0x8040 = ubyte time array, uncompressed quaternions
///             0x8042 = uint16 time header (8 bytes), uncompressed quaternions
/// - Position: 0xC040 = ubyte time, numPosKeys positions
///             0xC142 = uint16 time header (8 bytes), 2 positions
///             0xC242 = uint16 time header (8 bytes), data header (8 bytes), 1 position
///             0x0000 = no position track
/// </remarks>
public struct IvoAnimControllerEntry
{
    // Rotation track info (12 bytes)

    /// <summary>Number of rotation keyframes.</summary>
    public ushort NumRotKeys { get; set; }

    /// <summary>Rotation format flags. 0x8042 = standard rotation track.</summary>
    public ushort RotFormatFlags { get; set; }

    /// <summary>Offset to rotation time keys (relative to controller start).</summary>
    public uint RotTimeOffset { get; set; }

    /// <summary>Offset to rotation quaternion data (relative to controller start).</summary>
    public uint RotDataOffset { get; set; }

    // Position track info (12 bytes)

    /// <summary>Number of position keyframes. Meaning varies by format flag.</summary>
    public ushort NumPosKeys { get; set; }

    /// <summary>Position format flags. See remarks for valid values.</summary>
    public ushort PosFormatFlags { get; set; }

    /// <summary>Offset to position time keys (relative to controller start).</summary>
    public uint PosTimeOffset { get; set; }

    /// <summary>Offset to position vector data (relative to controller start).</summary>
    public uint PosDataOffset { get; set; }

    /// <summary>Returns true if this controller has position data.</summary>
    public readonly bool HasPosition => PosFormatFlags != 0;

    /// <summary>Returns true if this controller has rotation data.</summary>
    public readonly bool HasRotation => RotFormatFlags != 0;
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
