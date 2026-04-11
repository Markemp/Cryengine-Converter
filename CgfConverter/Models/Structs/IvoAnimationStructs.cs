using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace CgfConverter.Models.Structs;

/// <summary>
/// Position data format types for Ivo animations.
/// Determined by the high byte of PosFormatFlags.
/// </summary>
public enum IvoPositionFormat : byte
{
    /// <summary>No position data.</summary>
    None = 0x00,

    /// <summary>Float Vector3 (12 bytes per key), no header.</summary>
    FloatVector3 = 0xC0,

    /// <summary>SNORM with 24-byte header, all channels present (6 bytes per key).</summary>
    SNormFull = 0xC1,

    /// <summary>SNORM with 24-byte header, packed active channels only.</summary>
    SNormPacked = 0xC2
}

/// <summary>
/// Helper methods for Ivo animation data processing.
/// </summary>
public static class IvoAnimationHelpers
{
    /// <summary>FLT_MAX value used as sentinel for inactive channels.</summary>
    public const float FltMaxSentinel = 3.4028235e+38f;

    /// <summary>
    /// Gets the position format type from format flags.
    /// </summary>
    public static IvoPositionFormat GetPositionFormat(ushort posFormatFlags)
    {
        if (posFormatFlags == 0)
            return IvoPositionFormat.None;

        byte highByte = (byte)((posFormatFlags >> 8) & 0xFF);
        return highByte switch
        {
            0xC0 => IvoPositionFormat.FloatVector3,
            0xC1 => IvoPositionFormat.SNormFull,
            0xC2 => IvoPositionFormat.SNormPacked,
            _ => IvoPositionFormat.None
        };
    }

    /// <summary>
    /// Gets the time format from format flags (low nibble).
    /// 0x00 = ubyte time array, 0x02 = uint16 time with 8-byte header.
    /// </summary>
    public static byte GetTimeFormat(ushort formatFlags) => (byte)(formatFlags & 0x0F);

    /// <summary>
    /// Decompresses a SNORM int16 value to float using scale factor.
    /// Formula: (snormValue / 32767.0f) * scale
    /// </summary>
    public static float DecompressSNorm(short snormValue, float scale)
        => (snormValue / 32767.0f) * scale;

    /// <summary>
    /// Checks if a channel is active (not masked with FLT_MAX sentinel).
    /// </summary>
    public static bool IsChannelActive(float channelMaskValue)
        => channelMaskValue < FltMaxSentinel;

    /// <summary>
    /// Reads time keys from a binary reader based on format flags.
    /// </summary>
    /// <param name="b">Binary reader positioned at time data.</param>
    /// <param name="count">Number of keys.</param>
    /// <param name="formatFlags">Format flags (low nibble determines time format).</param>
    /// <returns>List of time values.</returns>
    public static List<float> ReadTimeKeys(BinaryReader b, int count, ushort formatFlags)
    {
        var times = new List<float>(count);
        byte timeFormat = GetTimeFormat(formatFlags);

        if (timeFormat == 0x00)
        {
            // 0x40: ubyte time array
            for (int t = 0; t < count; t++)
                times.Add(b.ReadByte());
        }
        else
        {
            // 0x42: 8-byte time header (startTime uint16, endTime uint16, marker uint32)
            ushort startTime = b.ReadUInt16();
            ushort endTime = b.ReadUInt16();
            b.ReadUInt32(); // marker

            // For single-key animations, just use start time
            if (count == 1)
            {
                times.Add(startTime);
            }
            else
            {
                // Interpolate times between start and end
                for (int t = 0; t < count; t++)
                {
                    float normalized = count > 1 ? (float)t / (count - 1) : 0;
                    times.Add(startTime + normalized * (endTime - startTime));
                }
            }
        }

        return times;
    }

    /// <summary>
    /// Reads rotation keyframes (uncompressed quaternions).
    /// </summary>
    /// <param name="b">Binary reader positioned at rotation data.</param>
    /// <param name="count">Number of rotation keys.</param>
    /// <returns>List of quaternion rotations.</returns>
    public static List<Quaternion> ReadRotationKeys(BinaryReader b, int count)
    {
        var rotations = new List<Quaternion>(count);

        // Ivo CAF/DBA uses uncompressed quaternions (16 bytes each: x, y, z, w)
        for (int i = 0; i < count; i++)
        {
            float x = b.ReadSingle();
            float y = b.ReadSingle();
            float z = b.ReadSingle();
            float w = b.ReadSingle();
            rotations.Add(new Quaternion(x, y, z, w));
        }

        return rotations;
    }

    /// <summary>
    /// Reads position keyframes based on format flags.
    /// </summary>
    /// <param name="b">Binary reader positioned at position data.</param>
    /// <param name="count">Number of position keys.</param>
    /// <param name="formatFlags">Position format flags (high byte determines format).</param>
    /// <returns>List of position vectors, or empty list if format unknown.</returns>
    public static List<Vector3> ReadPositionKeys(BinaryReader b, int count, ushort formatFlags)
    {
        var positions = new List<Vector3>(count);
        var format = GetPositionFormat(formatFlags);

        switch (format)
        {
            case IvoPositionFormat.FloatVector3:
                // 0xC0xx: Float Vector3 (12 bytes per key), no header
                for (int i = 0; i < count; i++)
                {
                    float x = b.ReadSingle();
                    float y = b.ReadSingle();
                    float z = b.ReadSingle();
                    positions.Add(new Vector3(x, y, z));
                }
                break;

            case IvoPositionFormat.SNormFull:
                // 0xC1xx: SNORM with 24-byte header, all channels (6 bytes per key)
                {
                    // Read 24-byte header: channelMask (12 bytes) + scale (12 bytes)
                    Vector3 channelMask = ReadVector3(b);
                    Vector3 scale = ReadVector3(b);

                    for (int i = 0; i < count; i++)
                    {
                        short sx = b.ReadInt16();
                        short sy = b.ReadInt16();
                        short sz = b.ReadInt16();

                        float x = DecompressSNorm(sx, scale.X);
                        float y = DecompressSNorm(sy, scale.Y);
                        float z = DecompressSNorm(sz, scale.Z);

                        positions.Add(new Vector3(x, y, z));
                    }
                }
                break;

            case IvoPositionFormat.SNormPacked:
                // 0xC2xx: SNORM with 24-byte header, packed active channels only
                {
                    // Read 24-byte header: channelMask (12 bytes) + scale (12 bytes)
                    Vector3 channelMask = ReadVector3(b);
                    Vector3 scale = ReadVector3(b);

                    bool xActive = IsChannelActive(channelMask.X);
                    bool yActive = IsChannelActive(channelMask.Y);
                    bool zActive = IsChannelActive(channelMask.Z);

                    for (int i = 0; i < count; i++)
                    {
                        float x = 0, y = 0, z = 0;

                        if (xActive)
                        {
                            short sx = b.ReadInt16();
                            x = DecompressSNorm(sx, scale.X);
                        }
                        if (yActive)
                        {
                            short sy = b.ReadInt16();
                            y = DecompressSNorm(sy, scale.Y);
                        }
                        if (zActive)
                        {
                            short sz = b.ReadInt16();
                            z = DecompressSNorm(sz, scale.Z);
                        }

                        positions.Add(new Vector3(x, y, z));
                    }
                }
                break;

            default:
                // Unknown format - return empty list
                break;
        }

        return positions;
    }

    /// <summary>
    /// Reads a Vector3 from binary reader (3 floats).
    /// </summary>
    private static Vector3 ReadVector3(BinaryReader b)
    {
        float x = b.ReadSingle();
        float y = b.ReadSingle();
        float z = b.ReadSingle();
        return new Vector3(x, y, z);
    }
}

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
/// <remarks>
/// Layout per 010 template:
/// - Flags (4 bytes)
/// - FramesPerSecond (2 bytes)
/// - NumControllers (2 bytes)
/// - Unknown1 (4 bytes)
/// - Unknown2 (4 bytes)
/// - StartRotation (16 bytes)
/// - StartPosition (12 bytes)
/// Total: 44 bytes
/// </remarks>
public struct IvoDBAMetaEntry
{
    /// <summary>Animation flags (usually 2).</summary>
    public uint Flags { get; set; }

    /// <summary>Frames per second (typically 30).</summary>
    public ushort FramesPerSecond { get; set; }

    /// <summary>Number of bone controllers.</summary>
    public ushort NumControllers { get; set; }

    /// <summary>Unknown value (often 0).</summary>
    public uint Unknown1 { get; set; }

    /// <summary>Unknown value (varies: 17, 25, etc.).</summary>
    public uint Unknown2 { get; set; }

    /// <summary>Unknown value (v902+).</summary>
    public uint Unknown3 { get; set; }

    /// <summary>Reference pose rotation.</summary>
    public Quaternion StartRotation { get; set; }

    /// <summary>Reference pose position.</summary>
    public Vector3 StartPosition { get; set; }
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

    /// <summary>File offsets where each controller starts (for relative offset calculations).</summary>
    public long[] ControllerOffsets { get; set; } = [];

    /// <summary>Parsed rotation keyframes per bone (bone hash -> rotations).</summary>
    public Dictionary<uint, List<Quaternion>> Rotations { get; set; } = [];

    /// <summary>Parsed position keyframes per bone (bone hash -> positions).</summary>
    public Dictionary<uint, List<Vector3>> Positions { get; set; } = [];

    /// <summary>Parsed rotation key times per bone (bone hash -> times).</summary>
    public Dictionary<uint, List<float>> RotationTimes { get; set; } = [];

    /// <summary>Parsed position key times per bone (bone hash -> times).</summary>
    public Dictionary<uint, List<float>> PositionTimes { get; set; } = [];
}
