using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CgfConverter.Services;
using Extensions;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// Controller chunk version 0x830 - Uncompressed CryKeyPQLog format.
/// Used in CAF animation files. Each chunk contains animation data for a single bone.
/// </summary>
internal sealed class ChunkController_830 : ChunkController
{
    /// <summary>Number of keyframes in this controller.</summary>
    public uint NumKeys { get; internal set; }

    /// <summary>CRC32 of the bone name this controller animates.</summary>
    public uint ControllerId { get; internal set; }

    /// <summary>Controller flags.</summary>
    public uint Flags { get; internal set; }

    /// <summary>Keyframe times in ticks.</summary>
    public List<int> KeyTimes { get; internal set; } = [];

    /// <summary>Keyframe positions.</summary>
    public List<Vector3> KeyPositions { get; internal set; } = [];

    /// <summary>Keyframe rotations (converted from log quaternion).</summary>
    public List<Quaternion> KeyRotations { get; internal set; } = [];

    public override void Read(BinaryReader b)
    {
        base.Read(b);

        ((EndiannessChangeableBinaryReader)b).IsBigEndian = false;

        NumKeys = b.ReadUInt32();
        ControllerId = b.ReadUInt32();
        Flags = b.ReadUInt32();

        // Read CryKeyPQLog data: 28 bytes per keyframe
        // struct CryKeyPQLog {
        //     int nTime;      // 4 bytes - Time in ticks
        //     Vec3 vPos;      // 12 bytes - Position (x, y, z)
        //     Vec3 vRotLog;   // 12 bytes - Logarithm of quaternion rotation
        // };
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
    /// Converts a logarithmic quaternion representation to a standard quaternion.
    /// The vRotLog is the axis of rotation scaled by the rotation angle.
    /// </summary>
    private static Quaternion LogToQuaternion(Vector3 rotLog)
    {
        float theta = rotLog.Length();

        if (theta < 0.0001f)
        {
            // Very small rotation, return identity
            return Quaternion.Identity;
        }

        float halfTheta = theta * 0.5f;
        float sinHalfTheta = MathF.Sin(halfTheta);
        float cosHalfTheta = MathF.Cos(halfTheta);

        // Normalize the axis
        Vector3 axis = rotLog / theta;

        // Build quaternion: w = cos(theta/2), xyz = axis * sin(theta/2)
        return new Quaternion(
            axis.X * sinHalfTheta,
            axis.Y * sinHalfTheta,
            axis.Z * sinHalfTheta,
            cosHalfTheta
        );
    }

    public override string ToString() =>
        $"ChunkController_830: ID={ID:X}, ControllerId={ControllerId:X}, NumKeys={NumKeys}, Flags={Flags:X}";
}
