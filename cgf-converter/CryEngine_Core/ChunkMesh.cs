using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkMesh : Chunk      //  cccc0000:  Object that points to the datastream chunk.
    {
        // public UInt32 Version;  // 623 Far Cry, 744 Far Cry, Aion, 800 Crysis
        //public bool HasVertexWeights; // for 744
        //public bool HasVertexColors; // 744
        //public bool InWorldSpace; // 623
        //public byte Reserved1;  // padding byte, 744
        //public byte Reserved2;  // padding byte, 744
        public int Flags1;  // 800  Offset of this chunk. 
        // public UInt32 ID;  // 800  Chunk ID
        public int NumVertices; // 
        public int NumIndices;  // Number of indices (each triangle has 3 indices, so this is the number of triangles times 3).
        //public UInt32 NumUVs; // 744
        //public UInt32 NumFaces; // 744
        // Pointers to various Chunk types
        //public ChunkMtlName Material; // 623, Material Chunk, never encountered?
        public int NumVertSubsets; // 801, Number of vert subsets
        public int MeshSubsets; // 800  Reference of the mesh subsets
        // public ChunkVertAnim VertAnims; // 744.  not implemented
        //public Vertex[] Vertices; // 744.  not implemented
        //public Face[,] Faces; // 744.  Not implemented
        //public UV[] UVs; // 744 Not implemented
        //public UVFace[] UVFaces; // 744 not implemented
        // public VertexWeight[] VertexWeights; // 744 not implemented
        //public IRGB[] VertexColors; // 744 not implemented
        public int VerticesData; // 800, 801.  Need an array because some 801 files have NumVertSubsets
        public int NumBuffs;
        public int NormalsData; // 800
        public int UVsData; // 800
        public int ColorsData; // 800
        public int Colors2Data; // 800 
        public int IndicesData; // 800
        public int TangentsData; // 800
        public int ShCoeffsData; // 800
        public int ShapeDeformationData; //800
        public int BoneMapData; //800
        public int FaceMapData; // 800
        public int VertMatsData; // 800
        public int MeshPhysicsData; // 801
        public int VertsUVsData;    // 801
        public int[] PhysicsData = new int[4]; // 800
        public Vector3 MinBound; // 800 minimum coordinate values
        public Vector3 MaxBound; // 800 Max coord values

        //public ChunkMeshSubsets chunkMeshSubset; // pointer to the mesh subset that belongs to this mesh

        public override void WriteChunk()
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
