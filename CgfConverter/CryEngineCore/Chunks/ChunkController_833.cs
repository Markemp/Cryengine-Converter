using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CgfConverter.Services;
using Extensions;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// Controller chunk version 0x833 - Uncompressed unified PQS keyframes.
/// CryEngine 5.x only; found in intermediate .i_caf files, not runtime .caf.
///
/// Unlike 0x829/0x831 (compressed, separate rot/pos tracks), this format stores
/// one fully uncompressed position+rotation+scale keyframe per frame.
/// No time data is stored — time is implicit: time[i] = i (frame index).
///
/// Binary layout:
///   uint32  numKeys
///   uint32  controllerId  (CRC32 of bone name)
///   [numKeys × CryKeyPQS (40 bytes each)]:
///     float3  position  (stored at 100× scale — divided by 100 on read)
///     float4  rotation  (xyzw unit quaternion, not log-compressed)
///     float3  scale     (diagonal scale matrix x, y, z)
/// </summary>
internal sealed class ChunkController_833 : ChunkController
{
    /// <summary>Number of keyframes (one per frame).</summary>
    public uint NumKeys { get; internal set; }

    /// <summary>CRC32 of the bone name this controller animates.</summary>
    public uint ControllerId { get; internal set; }

    /// <summary>Keyframe positions (raw values divided by 100).</summary>
    public List<Vector3> KeyPositions { get; internal set; } = [];

    /// <summary>Keyframe rotations (unit quaternions, xyzw).</summary>
    public List<Quaternion> KeyRotations { get; internal set; } = [];

    /// <summary>Keyframe scales (diagonal scale as x, y, z).</summary>
    public List<Vector3> KeyScales { get; internal set; } = [];

    public override void Read(BinaryReader b)
    {
        base.Read(b);

        ((EndiannessChangeableBinaryReader)b).IsBigEndian = false;

        NumKeys = b.ReadUInt32();
        ControllerId = b.ReadUInt32();

        for (int i = 0; i < NumKeys; i++)
        {
            // Position: Vec3 (3× float) stored at 100× scale
            KeyPositions.Add(b.ReadVector3() / 100.0f);

            // Rotation: Quat (xyzw, 4× float) — unit quaternion, not log-compressed
            KeyRotations.Add(b.ReadQuaternion());

            // Scale: Diag33 (3× float) — diagonal scale matrix
            KeyScales.Add(b.ReadVector3());
        }
    }

    public override string ToString() =>
        $"ChunkController_833: ID={ID:X}, ControllerId={ControllerId:X}, NumKeys={NumKeys}";
}
