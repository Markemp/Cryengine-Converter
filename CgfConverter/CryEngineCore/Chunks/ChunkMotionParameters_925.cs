using System.IO;
using Extensions;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// Motion Parameters chunk version 0x925 (2341 decimal).
/// 0x84 bytes (132 decimal) of motion parameter data.
/// </summary>
internal sealed class ChunkMotionParameters_925 : ChunkMotionParameters
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        AssetFlags = b.ReadUInt32();
        Compression = b.ReadUInt32();
        TicksPerFrame = b.ReadInt32();
        SecsPerTick = b.ReadSingle();
        Start = b.ReadInt32();
        End = b.ReadInt32();
        MoveSpeed = b.ReadSingle();
        TurnSpeed = b.ReadSingle();
        AssetTurn = b.ReadSingle();
        Distance = b.ReadSingle();
        Slope = b.ReadSingle();

        // StartLocation (QuatT: Quaternion 16 bytes + Vector3 12 bytes = 28 bytes)
        StartLocationQ = b.ReadQuaternion();
        StartLocationT = b.ReadVector3();

        // EndLocation (QuatT: Quaternion 16 bytes + Vector3 12 bytes = 28 bytes)
        EndLocationQ = b.ReadQuaternion();
        EndLocationT = b.ReadVector3();

        // Foot plant timing
        LHeelStart = b.ReadSingle();
        LHeelEnd = b.ReadSingle();
        LToe0Start = b.ReadSingle();
        LToe0End = b.ReadSingle();
        RHeelStart = b.ReadSingle();
        RHeelEnd = b.ReadSingle();
        RToe0Start = b.ReadSingle();
        RToe0End = b.ReadSingle();
    }
}
