using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CgfConverter.Services;
using Extensions;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// Controller chunk version 0x832 — CryEngine 5.x compressed PQS.
/// Extends 0x829 by adding a scale track. No Flags field (unlike 0x831).
/// CE5 will fatal on 0x831; Lumberyard will fatal on 0x832.
///
/// Binary layout (20 bytes after standard chunk header):
///   uint32  controllerId
///   uint16  numRotationKeys
///   uint16  numPositionKeys
///   uint16  numScaleKeys
///   uint8   rotationFormat      // ECompressionFormat
///   uint8   rotationTimeFormat  // EKeyTimesFormat
///   uint8   positionFormat
///   uint8   positionKeyInfo     // 0=shares rot times, 1=own time array
///   uint8   positionTimeFormat
///   uint8   scaleFormat
///   uint8   scaleKeysInfo       // 0=shares rot times, 1=shares pos times, 2=own time array
///   uint8   scaleTimeFormat
///   char[2] padding
///
/// Track order (each block 4-byte aligned):
///   rot values → rot times → pos values → pos times (if positionKeyInfo≠0) →
///   scale values → scale times (if scaleKeysInfo==2)
/// </summary>
internal sealed class ChunkController_832 : ChunkController
{
    public uint ControllerId { get; internal set; }
    public ushort NumRotationKeys { get; internal set; }
    public ushort NumPositionKeys { get; internal set; }
    public ushort NumScaleKeys { get; internal set; }
    public byte RotationFormat { get; internal set; }
    public byte RotationTimeFormat { get; internal set; }
    public byte PositionFormat { get; internal set; }
    public byte PositionKeyInfo { get; internal set; }
    public byte PositionTimeFormat { get; internal set; }
    public byte ScaleFormat { get; internal set; }
    public byte ScaleKeysInfo { get; internal set; }
    public byte ScaleTimeFormat { get; internal set; }

    public List<float> RotationKeyTimes { get; internal set; } = [];
    public List<float> PositionKeyTimes { get; internal set; } = [];
    public List<float> ScaleKeyTimes { get; internal set; } = [];
    public List<Quaternion> KeyRotations { get; internal set; } = [];
    public List<Vector3> KeyPositions { get; internal set; } = [];
    public List<Vector3> KeyScales { get; internal set; } = [];

    public override void Read(BinaryReader b)
    {
        base.Read(b);

        ((EndiannessChangeableBinaryReader)b).IsBigEndian = false;

        ControllerId = b.ReadUInt32();
        NumRotationKeys = b.ReadUInt16();
        NumPositionKeys = b.ReadUInt16();
        NumScaleKeys = b.ReadUInt16();
        RotationFormat = b.ReadByte();
        RotationTimeFormat = b.ReadByte();
        PositionFormat = b.ReadByte();
        PositionKeyInfo = b.ReadByte();
        PositionTimeFormat = b.ReadByte();
        ScaleFormat = b.ReadByte();
        ScaleKeysInfo = b.ReadByte();
        ScaleTimeFormat = b.ReadByte();
        b.ReadBytes(2); // padding

        // Rotation track
        if (NumRotationKeys > 0)
        {
            KeyRotations = ReadRotations(b, NumRotationKeys, RotationFormat);
            AlignTo4Bytes(b);
            RotationKeyTimes = ReadKeyTimes(b, NumRotationKeys, RotationTimeFormat);
            AlignTo4Bytes(b);
        }

        // Position track
        if (NumPositionKeys > 0)
        {
            KeyPositions = ReadPositions(b, NumPositionKeys, PositionFormat);
            AlignTo4Bytes(b);

            if (PositionKeyInfo != 0)
            {
                PositionKeyTimes = ReadKeyTimes(b, NumPositionKeys, PositionTimeFormat);
                AlignTo4Bytes(b);
            }
            else
            {
                PositionKeyTimes = RotationKeyTimes;
            }
        }

        // Scale track — same Vec3-based formats as position
        if (NumScaleKeys > 0)
        {
            KeyScales = ReadPositions(b, NumScaleKeys, ScaleFormat);
            AlignTo4Bytes(b);

            ScaleKeyTimes = ScaleKeysInfo switch
            {
                0 => RotationKeyTimes,
                1 => PositionKeyTimes,
                2 => ReadKeyTimes(b, NumScaleKeys, ScaleTimeFormat),
                _ => RotationKeyTimes
            };
        }
    }

    private static void AlignTo4Bytes(BinaryReader b)
    {
        var remainder = b.BaseStream.Position % 4;
        if (remainder != 0)
            b.ReadBytes((int)(4 - remainder));
    }

    private static List<float> ReadKeyTimes(BinaryReader b, int count, byte format)
    {
        var times = new List<float>(count);
        for (int i = 0; i < count; i++)
        {
            times.Add(format switch
            {
                0 => b.ReadSingle(),
                1 => b.ReadUInt16(),
                2 => b.ReadByte(),
                3 => b.ReadSingle(),
                4 => b.ReadUInt16(),
                5 => b.ReadByte(),
                6 => b.ReadUInt16(),
                _ => b.ReadSingle()
            });
        }
        return times;
    }

    private static List<Quaternion> ReadRotations(BinaryReader b, int count, byte format)
    {
        var rotations = new List<Quaternion>(count);
        for (int i = 0; i < count; i++)
        {
            rotations.Add((ECompressionFormat)format switch
            {
                ECompressionFormat.eNoCompressQuat     => b.ReadQuaternion(),
                ECompressionFormat.eShotInt3Quat        => b.ReadShortInt3Quat(),
                ECompressionFormat.eSmallTreeDWORDQuat  => b.ReadSmallTreeDWORDQuat(),
                ECompressionFormat.eSmallTree48BitQuat  => b.ReadSmallTree48BitQuat(),
                ECompressionFormat.eSmallTree64BitQuat  => b.ReadSmallTree64BitQuat(),
                ECompressionFormat.eSmallTree64BitExtQuat => b.ReadSmallTree64BitExtQuat(),
                _ => Quaternion.Identity
            });
        }
        return rotations;
    }

    private static List<Vector3> ReadPositions(BinaryReader b, int count, byte format)
    {
        var positions = new List<Vector3>(count);
        for (int i = 0; i < count; i++)
        {
            positions.Add((ECompressionFormat)format switch
            {
                ECompressionFormat.eNoCompress     => b.ReadVector3(),
                ECompressionFormat.eNoCompressVec3 => b.ReadVector3(),
                _ => Vector3.Zero
            });
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
        $"ChunkController_832: ID={ID:X}, ControllerId={ControllerId:X}, RotKeys={NumRotationKeys}, PosKeys={NumPositionKeys}, ScaleKeys={NumScaleKeys}";
}
