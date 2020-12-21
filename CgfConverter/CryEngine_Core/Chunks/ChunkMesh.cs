namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkMesh : Chunk      //  cccc0000:  Object that points to the datastream chunk.
    {
        // public UInt32 Version;  // 623 Far Cry, 744 Far Cry, Aion, 800 Crysis
        //public bool HasVertexWeights; // for 744
        //public bool HasVertexColors; // 744
        //public bool InWorldSpace; // 623
        //public byte Reserved1;  // padding byte, 744
        //public byte Reserved2;  // padding byte, 744
        public int Flags1 { get; set; }  // 800  Offset of this chunk. 
        public int Flags2 { get; set; }  // 801 and 802
        // public UInt32 ID;  // 800  Chunk ID
        public int NumVertices { get; set; } // 
        public int NumIndices { get; set; }  // Number of indices (each triangle has 3 indices, so this is the number of triangles times 3).
        //public UInt32 NumUVs; // 744
        //public UInt32 NumFaces; // 744
        // Pointers to various Chunk types
        //public ChunkMtlName Material; // 623, Material Chunk, never encountered?
        public int NumVertSubsets { get; set; } // 801, Number of vert subsets
        public int VertsAnimID { get; set; }
        public int MeshSubsets { get; set; } // 800  Reference of the mesh subsets
        // public ChunkVertAnim VertAnims; // 744.  not implemented
        //public Vertex[] Vertices; // 744.  not implemented
        //public Face[,] Faces; // 744.  Not implemented
        //public UV[] UVs; // 744 Not implemented
        //public UVFace[] UVFaces; // 744 not implemented
        // public VertexWeight[] VertexWeights; // 744 not implemented
        //public IRGB[] VertexColors; // 744 not implemented
        public int VerticesData { get; set; } // 800, 801.  Need an array because some 801 files have NumVertSubsets
        public int NumBuffs { get; set; }
        public int NormalsData { get; set; } // 800
        public int UVsData { get; set; } // 800
        public int ColorsData { get; set; } // 800
        public int Colors2Data { get; set; } // 800 
        public int IndicesData { get; set; } // 800
        public int TangentsData { get; set; } // 800
        public int ShCoeffsData { get; set; } // 800
        public int ShapeDeformationData { get; set; } //800
        public int BoneMapData { get; set; } //800
        public int FaceMapData { get; set; } // 800
        public int VertMatsData { get; set; } // 800
        public int MeshPhysicsData { get; set; } // 801
        public int VertsUVsData { get; set; }    // 801
        public int[] PhysicsData = new int[4]; // 800
        public Vector3 MinBound; // 800 minimum coordinate values
        public Vector3 MaxBound; // 800 Max coord values

        /// <summary>
        /// The actual geometry info for this mesh.
        /// </summary>
        public GeometryInfo GeometryInfo { get; set; }

        //public ChunkMeshSubsets chunkMeshSubset; // pointer to the mesh subset that belongs to this mesh
        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";
        }

        public void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Verbose, "*** START MESH CHUNK ***");
            Utils.Log(LogLevelEnum.Verbose, "    ChunkType:           {0}", ChunkType);
            Utils.Log(LogLevelEnum.Verbose, "    Chunk ID:            {0:X}", ID);
            Utils.Log(LogLevelEnum.Verbose, "    MeshSubSetID:        {0:X}", MeshSubsets);
            Utils.Log(LogLevelEnum.Verbose, "    Vertex Datastream:   {0:X}", VerticesData);
            Utils.Log(LogLevelEnum.Verbose, "    Normals Datastream:  {0:X}", NormalsData);
            Utils.Log(LogLevelEnum.Verbose, "    UVs Datastream:      {0:X}", UVsData);
            Utils.Log(LogLevelEnum.Verbose, "    Indices Datastream:  {0:X}", IndicesData);
            Utils.Log(LogLevelEnum.Verbose, "    Tangents Datastream: {0:X}", TangentsData);
            Utils.Log(LogLevelEnum.Verbose, "    Mesh Physics Data:   {0:X}", MeshPhysicsData);
            Utils.Log(LogLevelEnum.Verbose, "    VertUVs:             {0:X}", VertsUVsData);
            Utils.Log(LogLevelEnum.Verbose, "    MinBound:            {0:F7}, {1:F7}, {2:F7}", MinBound.x, MinBound.y, MinBound.z);
            Utils.Log(LogLevelEnum.Verbose, "    MaxBound:            {0:F7}, {1:F7}, {2:F7}", MaxBound.x, MaxBound.y, MaxBound.z);
            Utils.Log(LogLevelEnum.Verbose, "*** END MESH CHUNK ***");
        }
    }
}
