using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkCompiledExtToIntMap_800 : ChunkCompiledExtToIntMap
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            NumExtVertices = this.DataSize / sizeof(UInt16);
            Source = new UInt16[NumExtVertices];
            for (int i = 0; i < NumExtVertices; i++)
            {
                Source[i] = b.ReadUInt16();
            }
        }

        public override void WriteChunk()
        {
            base.WriteChunk();
        }
    }
}
