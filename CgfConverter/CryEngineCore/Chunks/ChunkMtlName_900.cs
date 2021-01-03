using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkMtlName_900 : ChunkMtlName
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            Name = b.ReadFString(128);
            NumChildren = 0;
        }
    }
}
