using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CgfConverter.Services;
using Extensions;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// Controller chunk version 0x829 - Compressed format with separate rotation/position tracks.
/// Used in CAF animation files (Armored Warfare). Each chunk contains animation data for a single bone.
/// Similar to 0x831 but without the Flags field (16-byte header with 2-byte padding vs 18-byte).
///
/// Vetted: Armored Warfare chicken walk/idle animations export correctly to USD/Blender.
/// </summary>
internal sealed class ChunkController_829 : ChunkController
{
    /// <summary>CRC32 of the bone name this controller animates.</summary>
    public uint ControllerId { get; internal set; }

    /// <summary>Number of rotation keyframes.</summary>
    public ushort NumRotationKeys { get; internal set; }

    /// <summary>Number of position keyframes.</summary>
    public ushort NumPositionKeys { get; internal set; }

    /// <summary>Compression format for rotation data.</summary>
    public byte RotationFormat { get; internal set; }

    /// <summary>Time encoding format for rotation keys.</summary>
    public byte RotationTimeFormat { get; internal set; }

    /// <summary>Compression format for position data.</summary>
    public byte PositionFormat { get; internal set; }

    /// <summary>Position keys info byte.</summary>
    public byte PositionKeysInfo { get; internal set; }

    /// <summary>Time encoding format for position keys.</summary>
    public byte PositionTimeFormat { get; internal set; }

    /// <summary>Tracks alignment flag.</summary>
    public byte TracksAligned { get; internal set; }

    /// <summary>Rotation keyframe times.</summary>
    public List<float> RotationKeyTimes { get; internal set; } = [];

    /// <summary>Position keyframe times.</summary>
    public List<float> PositionKeyTimes { get; internal set; } = [];

    /// <summary>Keyframe rotations.</summary>
    public List<Quaternion> KeyRotations { get; internal set; } = [];

    /// <summary>Keyframe positions.</summary>
    public List<Vector3> KeyPositions { get; internal set; } = [];

    public override void Read(BinaryReader b)
    {
        base.Read(b);

        ((EndiannessChangeableBinaryReader)b).IsBigEndian = false;

        // 829 format differs from 831 - no Flags field
        ControllerId = b.ReadUInt32();
        NumRotationKeys = b.ReadUInt16();
        NumPositionKeys = b.ReadUInt16();
        RotationFormat = b.ReadByte();
        RotationTimeFormat = b.ReadByte();
        PositionFormat = b.ReadByte();
        PositionKeysInfo = b.ReadByte();
        PositionTimeFormat = b.ReadByte();
        TracksAligned = b.ReadByte();

        // Per 010 template: 2 bytes of padding after header (unconditional)
        b.ReadUInt16(); // padding

        // Data layout (v0829) - 16 byte header, then:
        // [Rotation Values] -> [conditional padding] -> [Rotation Times] -> [conditional padding] ->
        // [Position Values] -> [conditional padding] -> [Position Times (if PositionKeysInfo != 0)]

        // Read rotation values
        if (NumRotationKeys > 0)
        {
            KeyRotations = ReadRotations(b, NumRotationKeys, RotationFormat);
            AlignTo4Bytes(b);

            // Read rotation time keys
            RotationKeyTimes = ReadKeyTimes(b, NumRotationKeys, RotationTimeFormat);
            AlignTo4Bytes(b);
        }

        // Read position values
        if (NumPositionKeys > 0)
        {
            KeyPositions = ReadPositions(b, NumPositionKeys, PositionFormat);
            AlignTo4Bytes(b);

            // Position time keys only if PositionKeysInfo != 0, otherwise shares rotation times
            if (PositionKeysInfo != 0)
            {
                PositionKeyTimes = ReadKeyTimes(b, NumPositionKeys, PositionTimeFormat);
                AlignTo4Bytes(b);
            }
            else
            {
                PositionKeyTimes = RotationKeyTimes;
            }
        }
    }

    private void AlignTo4Bytes(BinaryReader b)
    {
        if (TracksAligned != 0)
        {
            var pos = b.BaseStream.Position;
            var remainder = pos % 4;
            if (remainder != 0)
                b.ReadBytes((int)(4 - remainder));
        }
    }

    private List<float> ReadKeyTimes(BinaryReader b, int count, byte format)
    {
        var times = new List<float>(count);

        // EKeyTimesFormat: 0 = eF32 (float), 1 = eUINT16, 2 = eByte
        for (int i = 0; i < count; i++)
        {
            float time = format switch
            {
                0 => b.ReadSingle(),      // eF32 - 4 bytes
                1 => b.ReadUInt16(),      // eUINT16 - 2 bytes
                2 => b.ReadByte(),        // eByte - 1 byte
                3 => b.ReadSingle(),      // eF32StartStop
                4 => b.ReadUInt16(),      // eUINT16StartStop
                5 => b.ReadByte(),        // eByteStartStop
                6 => b.ReadUInt16(),      // eBitset
                _ => b.ReadSingle()       // Default to float
            };
            times.Add(time);
        }

        return times;
    }

    private List<Quaternion> ReadRotations(BinaryReader b, int count, byte format)
    {
        var rotations = new List<Quaternion>(count);

        for (int i = 0; i < count; i++)
        {
            Quaternion rot = (ECompressionFormat)format switch
            {
                ECompressionFormat.eNoCompressQuat => b.ReadQuaternion(),
                ECompressionFormat.eShotInt3Quat => b.ReadShortInt3Quat(),
                ECompressionFormat.eSmallTreeDWORDQuat => b.ReadSmallTreeDWORDQuat(),
                ECompressionFormat.eSmallTree48BitQuat => b.ReadSmallTree48BitQuat(),
                ECompressionFormat.eSmallTree64BitQuat => b.ReadSmallTree64BitQuat(),
                ECompressionFormat.eSmallTree64BitExtQuat => b.ReadSmallTree64BitExtQuat(),
                _ => Quaternion.Identity
            };
            rotations.Add(rot);
        }

        return rotations;
    }

    private List<Vector3> ReadPositions(BinaryReader b, int count, byte format)
    {
        var positions = new List<Vector3>(count);

        for (int i = 0; i < count; i++)
        {
            // Per 010 template: positions only use eNoCompress or eNoCompressVec3 (both read as Vector3)
            Vector3 pos = (ECompressionFormat)format switch
            {
                ECompressionFormat.eNoCompress => b.ReadVector3(),
                ECompressionFormat.eNoCompressVec3 => b.ReadVector3(),
                _ => Vector3.Zero
            };
            positions.Add(pos);
        }

        return positions;
    }

    private enum ECompressionFormat
    {
        eNoCompress = 0,
        eNoCompressQuat = 1,
        eNoCompressVec3 = 2,
        eShotInt3Quat = 3,
        eSmallTreeDWORDQuat = 4,
        eSmallTree48BitQuat = 5,
        eSmallTree64BitQuat = 6,
        ePolarQuat = 7,
        eSmallTree64BitExtQuat = 8,
        eAutomaticQuat = 9
    }

    public override string ToString() =>
        $"ChunkController_829: ID={ID:X}, ControllerId={ControllerId:X}, RotKeys={NumRotationKeys}, PosKeys={NumPositionKeys}";
}
