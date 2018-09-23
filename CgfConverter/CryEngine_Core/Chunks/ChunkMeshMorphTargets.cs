using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    /// <summary>
    /// Legacy class.  No longer used.
    /// </summary>
    public abstract class ChunkMeshMorphTargets : Chunk
    {
        public uint ChunkIDMesh;
        public uint NumMorphVertices;

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Debug, "*** START MorphTargets Chunk ***");
            Utils.Log(LogLevelEnum.Debug, "    ChunkType:           {0}", ChunkType);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", ID);
            Utils.Log(LogLevelEnum.Debug, "    Chunk ID Mesh:       {0:X}", ChunkIDMesh);
        }

    }
}
