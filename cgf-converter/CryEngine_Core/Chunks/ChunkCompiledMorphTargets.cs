using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkCompiledMorphTargets : Chunk
    {
        public uint NumberOfMorphTargets;

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Debug, "*** START MorphTargets Chunk ***");
            Utils.Log(LogLevelEnum.Debug, "    ChunkType:           {0}", ChunkType);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", ID);
            Utils.Log(LogLevelEnum.Debug, "    Number of Targets:   {0:X}", NumberOfMorphTargets);
        }

    }
}
