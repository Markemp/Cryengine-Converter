using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkCompiledMorphTargets : Chunk
    {
        public uint NumberOfMorphTargets;
        public MeshMorphTargetVertex[] MorphTargetVertices;

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, NUmber of Morph Targets: {NumberOfMorphTargets}";
        }
    }
}
