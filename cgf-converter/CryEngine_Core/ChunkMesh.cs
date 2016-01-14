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
        public UInt32 Flags1;  // 800  Offset of this chunk. 
        // public UInt32 ID;  // 800  Chunk ID
        public UInt32 NumVertices; // 
        public UInt32 NumIndices;  // Number of indices (each triangle has 3 indices, so this is the number of triangles times 3).
        //public UInt32 NumUVs; // 744
        //public UInt32 NumFaces; // 744
        // Pointers to various Chunk types
        //public ChunkMtlName Material; // 623, Material Chunk, never encountered?
        public UInt32 NumVertSubsets; // 801, Number of vert subsets
        public UInt32 MeshSubsets; // 800  Reference of the mesh subsets
        // public ChunkVertAnim VertAnims; // 744.  not implemented
        //public Vertex[] Vertices; // 744.  not implemented
        //public Face[,] Faces; // 744.  Not implemented
        //public UV[] UVs; // 744 Not implemented
        //public UVFace[] UVFaces; // 744 not implemented
        // public VertexWeight[] VertexWeights; // 744 not implemented
        //public IRGB[] VertexColors; // 744 not implemented
        public UInt32 VerticesData; // 800, 801.  Need an array because some 801 files have NumVertSubsets
        public UInt32 NumBuffs;
        public UInt32 NormalsData; // 800
        public UInt32 UVsData; // 800
        public UInt32 ColorsData; // 800
        public UInt32 Colors2Data; // 800 
        public UInt32 IndicesData; // 800
        public UInt32 TangentsData; // 800
        public UInt32 ShCoeffsData; // 800
        public UInt32 ShapeDeformationData; //800
        public UInt32 BoneMapData; //800
        public UInt32 FaceMapData; // 800
        public UInt32 VertMatsData; // 800
        public UInt32 MeshPhysicsData; // 801
        public UInt32 VertsUVsData;    // 801
        public UInt32[] PhysicsData = new uint[4]; // 800
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
