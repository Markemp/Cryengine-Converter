using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    class ChunkMeshPhysicsData : Chunk
    {
          
        // Collision mesh or something like that.  TODO

        public UInt32 PhysicsDataSize { get; internal set; }  //Size of the physical data at the end of the chunk.
        public uint Flags { get; internal set; } // not used?
        public uint TetrahedraDataSize { get; internal set; } // Bytes per data entry
        public ChunkDataStream Tetrahedra { get; internal set; } // not sure the type yet
        public uint Reserved1 { get; internal set; }
        public uint Reserved2 { get; internal set; }

        public PhysicsData physicsData { get; internal set; }  // if physicsdatasize != 0
        public byte[] TetrahedraData { get; internal set; } // Array length TetrahedraDataSize.  

        public override void WriteChunk()
        {
            base.WriteChunk();
        }
    }

}
