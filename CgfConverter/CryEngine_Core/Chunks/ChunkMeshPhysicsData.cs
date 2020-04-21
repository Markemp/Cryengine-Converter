namespace CgfConverter.CryEngineCore
{
    class ChunkMeshPhysicsData : Chunk
    {

        // Collision mesh or something like that.  TODO

#pragma warning disable CS0649 // Field 'ChunkMeshPhysicsData.PhysicsDataSize' is never assigned to, and will always have its default value 0
        public int PhysicsDataSize;             //Size of the physical data at the end of the chunk.
#pragma warning restore CS0649 // Field 'ChunkMeshPhysicsData.PhysicsDataSize' is never assigned to, and will always have its default value 0
#pragma warning disable CS0649 // Field 'ChunkMeshPhysicsData.Flags' is never assigned to, and will always have its default value 0
        public int Flags;
#pragma warning restore CS0649 // Field 'ChunkMeshPhysicsData.Flags' is never assigned to, and will always have its default value 0
#pragma warning disable CS0649 // Field 'ChunkMeshPhysicsData.TetrahedraDataSize' is never assigned to, and will always have its default value 0
        public int TetrahedraDataSize;          // Bytes per data entry
#pragma warning restore CS0649 // Field 'ChunkMeshPhysicsData.TetrahedraDataSize' is never assigned to, and will always have its default value 0
#pragma warning disable CS0649 // Field 'ChunkMeshPhysicsData.TetrahedraID' is never assigned to, and will always have its default value 0
        public int TetrahedraID;                // Chunk ID of the data stream
#pragma warning restore CS0649 // Field 'ChunkMeshPhysicsData.TetrahedraID' is never assigned to, and will always have its default value 0
#pragma warning disable CS0649 // Field 'ChunkMeshPhysicsData.Tetrahedra' is never assigned to, and will always have its default value null
        public ChunkDataStream Tetrahedra;
#pragma warning restore CS0649 // Field 'ChunkMeshPhysicsData.Tetrahedra' is never assigned to, and will always have its default value null
#pragma warning disable CS0649 // Field 'ChunkMeshPhysicsData.Reserved1' is never assigned to, and will always have its default value 0
        public uint Reserved1;
#pragma warning restore CS0649 // Field 'ChunkMeshPhysicsData.Reserved1' is never assigned to, and will always have its default value 0
#pragma warning disable CS0649 // Field 'ChunkMeshPhysicsData.Reserved2' is never assigned to, and will always have its default value 0
        public uint Reserved2;
#pragma warning restore CS0649 // Field 'ChunkMeshPhysicsData.Reserved2' is never assigned to, and will always have its default value 0

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
