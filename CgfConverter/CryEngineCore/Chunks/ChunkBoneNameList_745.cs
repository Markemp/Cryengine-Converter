using System;
using System.IO;
using System.Linq;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkBoneNameList_745 : ChunkBoneNameList
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        var peek = b.PeekChar();
        
        if (peek == 0x0)
            b.ReadInt32();  // Not sure what this value is.

        BoneNames = b.ReadCString().Split(' ').ToList();
    }
}
