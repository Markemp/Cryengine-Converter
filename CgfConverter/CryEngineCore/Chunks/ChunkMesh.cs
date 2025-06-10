using CgfConverter.Models;
using CgfConverter.Models.Structs;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

public abstract class ChunkMesh : Chunk
{
    // public UInt32 Version;  // 623 Far Cry, 744 Far Cry, Aion, 800 Crysis
    public MeshChunkFlag Flags1 { get; set; }  // 800
    public int Flags2 { get; set; }  // 801 and 802
    public int NumVertices { get; set; } // 
    public int NumIndices { get; set; }  // Number of indices (each triangle has 3 indices, so this is the number of triangles times 3).
    // Pointers to various Chunk types
    //public ChunkMtlName Material; // 623, Material Chunk, never encountered?
    public uint NumVertSubsets { get; set; } // 801, Number of vert subsets
    public int VertsAnimID { get; set; }
    public int MeshSubsetsData { get; set; } // 800  Reference of the mesh subsets
    public int VerticesData { get; set; }
    public int NumBuffs { get; set; }
    public int NormalsData { get; set; }
    public int UVsData { get; set; }
    public int ColorsData { get; set; }
    public int Colors2Data { get; set; }
    public int IndicesData { get; set; }
    public int TangentsData { get; set; }
    public int ShCoeffsData { get; set; }
    public int ShapeDeformationData { get; set; }
    public int BoneMapData { get; set; }
    public int FaceMapData { get; set; }
    public int VertMatsData { get; set; }
    public int QTangentsData { get; set; }
    public int SkinData { get; set; }
    public int Dummy2Data { get; set; }
    public int MeshPhysicsData { get; set; }
    public int VertsUVsData { get; set; }
    public int[] PhysicsData = new int[4];
    public BoundingBox BoundingBox { get; set; }
    public Vector3 MinBound { get; set; }
    public Vector3 MaxBound { get; set; }

    // For Ivo files
    public BoundingBox? ScalingVectors { get; set; }

    // Computed properties
    public GeometryInfo? GeometryInfo { get; set; }

    public override string ToString() => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";
}
