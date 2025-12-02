using System.IO;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// Animation info chunk version 0x901.
/// Size: 48 bytes (0x30).
/// </summary>
internal sealed class ChunkIvoAnimInfo_901 : ChunkIvoAnimInfo
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        Flags = b.ReadUInt32();
        Unknown1 = b.ReadUInt16();
        NumBones = b.ReadUInt16();
        Unknown2 = b.ReadUInt32();
        NumPositionTracks = b.ReadUInt32();

        BoundMin[0] = b.ReadSingle();
        BoundMin[1] = b.ReadSingle();
        BoundMin[2] = b.ReadSingle();
        Scale = b.ReadSingle();

        Precision = b.ReadDouble();

        Padding[0] = b.ReadUInt32();
        Padding[1] = b.ReadUInt32();
    }

    public override string ToString() =>
        $"ChunkIvoAnimInfo_901: Bones={NumBones}, PosTracks={NumPositionTracks}, Scale={Scale:F4}";
}
