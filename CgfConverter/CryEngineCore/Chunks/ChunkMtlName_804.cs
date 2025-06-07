using System;
using System.IO;

namespace CgfConverter.CryEngineCore.Chunks;

internal class ChunkMtlName_804 : ChunkMtlName
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        AssetId = Guid.Parse(b.ReadFString(38));
        Name = AssetId?.ToString() ?? "unknown";
        SkipBytes(b, 26);

        // some count followed by 3 emtpy bytes 
        NumChildren = b.ReadUInt32();
        SkipBytes(b, NumChildren * 4);  // Flags.  Usually 0xffffffff

        ChildNames = [b.ReadCString()];
        for (int i = 0; i < NumChildren; i++)
        {
            ChildNames.Add(b.ReadCString());
        }
    }
}
