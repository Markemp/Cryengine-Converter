using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CgfConverter.Services;
using Extensions;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// Controller chunk version 0x827 - Uncompressed CryKeyPQLog format (legacy).
/// Used in old 0x744-format CAF animation files (e.g. ArcheAge).
///
/// Identical to 0x830 but without the Flags field and without an embedded local
/// chunk header — the chunk data starts directly at the offset from the chunk table.
///
/// Binary layout:
///   uint32  numKeys
///   uint32  nControllerId  (CRC32 of bone name)
///   [numKeys × CryKeyPQLog]:
///     int32   nTime   (keyframe tick)
///     float3  vPos    (position)
///     float3  vRotLog (quaternion logarithm: axis × half-angle)
/// </summary>
internal sealed class ChunkController_827 : ChunkController
{
    /// <summary>Number of keyframes.</summary>
    public uint NumKeys { get; internal set; }

    /// <summary>CRC32 of the bone name this controller animates.</summary>
    public uint ControllerId { get; internal set; }

    /// <summary>Keyframe times in ticks.</summary>
    public List<int> KeyTimes { get; internal set; } = [];

    /// <summary>Keyframe positions.</summary>
    public List<Vector3> KeyPositions { get; internal set; } = [];

    /// <summary>Keyframe rotations (converted from log quaternion).</summary>
    public List<Quaternion> KeyRotations { get; internal set; } = [];

    public override void Read(BinaryReader b)
    {
        // 0x827 chunks have no embedded local header — bypass the base.Read() local-header
        // logic and initialise directly from the chunk table header.
        ChunkType = _header.ChunkType;
        VersionRaw = _header.VersionRaw;
        Offset = _header.Offset;
        ID = _header.ID;
        Size = _header.Size;
        DataSize = Size;

        b.BaseStream.Seek(_header.Offset, SeekOrigin.Begin);
        ((EndiannessChangeableBinaryReader)b).IsBigEndian = false;

        NumKeys = b.ReadUInt32();
        ControllerId = b.ReadUInt32();

        for (int i = 0; i < NumKeys; i++)
        {
            int time = b.ReadInt32();
            Vector3 position = b.ReadVector3();
            Vector3 rotLog = b.ReadVector3();

            KeyTimes.Add(time);
            KeyPositions.Add(position);
            KeyRotations.Add(LogToQuaternion(rotLog));
        }
    }

    /// <summary>
    /// Converts a CryKeyPQLog rotation logarithm to a quaternion.
    /// vRotLog = axis × half-angle; mirrors CryEngine's Quat::exp(Vec3).
    /// </summary>
    private static Quaternion LogToQuaternion(Vector3 rotLog)
    {
        float theta = rotLog.Length(); // = half-angle
        if (theta < 0.0001f)
            return Quaternion.Identity;

        float sinTheta = MathF.Sin(theta);
        float cosTheta = MathF.Cos(theta);
        float s = sinTheta / theta;

        return new Quaternion(rotLog.X * s, rotLog.Y * s, rotLog.Z * s, cosTheta);
    }

    public override string ToString() =>
        $"ChunkController_827: ID={ID:X}, ControllerId={ControllerId:X}, NumKeys={NumKeys}";
}
