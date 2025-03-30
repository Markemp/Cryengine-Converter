using System;
using System.IO;

namespace CgfConverter.CryEngineCore.Chunks;

internal class ChunkMtlName_804 : ChunkMtlName
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        AssetId = Guid.Parse(b.ReadFString(38));

        SkipBytes(b, 26);

        // some count followed by 3 emtpy bytes 
        NumChildren = b.ReadUInt32();
        SkipBytes(b, NumChildren);  // Flags.  Usually 0xffffffff

        Name = b.ReadCString();  // this is an array of strings
    }
}
