using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    class ChunkMeshPhysicsData : Chunk
    {

        // Collision mesh or something like that.  TODO

        public int PhysicsDataSize;             //Size of the physical data at the end of the chunk.
        public int Flags; 
        public int TetrahedraDataSize;          // Bytes per data entry
        public int TetrahedraID;                // Chunk ID of the data stream
        public ChunkDataStream Tetrahedra;
        public uint Reserved1;
        public uint Reserved2;

        public PhysicsData physicsData { get; internal set; }  // if physicsdatasize != 0
        public byte[] TetrahedraData { get; internal set; } // Array length TetrahedraDataSize.  

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";
        }

        public void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Debug, "*** START CompiledBone Chunk ***");
            Utils.Log(LogLevelEnum.Debug, "    ChunkType:           {0}", ChunkType);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", ID);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", PhysicsDataSize);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", TetrahedraDataSize);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", TetrahedraID);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", ID);
        }
    }
}
