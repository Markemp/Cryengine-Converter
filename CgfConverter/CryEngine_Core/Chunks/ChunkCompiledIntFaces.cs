using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkCompiledIntFaces : Chunk
    {
        public int Reserved;
        public uint NumIntFaces;
        public TFace[] Faces;

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}";
        }
    }
}
