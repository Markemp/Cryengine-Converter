using Extensions;
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
        FramesPerSecond = b.ReadUInt16();
        NumBones = b.ReadUInt16();
        Reserved = b.ReadUInt32();
        EndFrame = b.ReadUInt32();
        StartRotation = b.ReadQuaternion();
        StartPosition = b.ReadVector3();
        Padding = b.ReadUInt32();
    }

    public override string ToString() =>
        $"ChunkIvoAnimInfo_901: Bones={NumBones}, FPS={FramesPerSecond}, EndFrame={EndFrame}";
}
