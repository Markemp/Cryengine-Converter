using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkCompiledExtToIntMap : Chunk
    {
        public int Reserved;
        public uint NumExtVertices;
        public UInt16[] Source;

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}";
        }
    }
}
