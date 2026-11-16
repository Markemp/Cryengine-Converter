using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkHeader_900 : ChunkHeader
{
    public override void Read(BinaryReader reader)
    {
        uint headerType = reader.ReadUInt32();
        ChunkType = (ChunkType)headerType;
        VersionRaw = reader.ReadUInt32();
        Offset = (uint)reader.ReadUInt64();  // All other versions use uint.  No idea why uint64 is needed.
        // Version 0x900 has no stored chunk ID. Model assigns a mode-appropriate surrogate after reading.
    }
}
