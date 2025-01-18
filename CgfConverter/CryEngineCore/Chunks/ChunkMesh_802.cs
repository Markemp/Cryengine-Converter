using Extensions;
using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkMesh_802 : ChunkMesh
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        Flags1 = (MeshChunkFlag)b.ReadInt32();
        Flags2 = b.ReadInt32();
        NumVertices = b.ReadInt32();
        NumIndices = b.ReadInt32();
        NumVertSubsets = b.ReadUInt32();
        MeshSubsetsData = b.ReadInt32();           // Chunk ID of MeshSubsets 
        VertsAnimID = b.ReadInt32();
        VerticesData = b.ReadInt32();
        SkipBytes(b, 28);           // These are stream index values.  Should be 8 ints (including the data stream)
        NormalsData = b.ReadInt32();           // Chunk ID of the datastream for the normals for this mesh
        SkipBytes(b, 28);
        UVsData = b.ReadInt32();               // Chunk ID of the Normals datastream
        SkipBytes(b, 28);
        ColorsData = b.ReadInt32();
        SkipBytes(b, 28);
        Colors2Data = b.ReadInt32();
        SkipBytes(b, 28);
        IndicesData = b.ReadInt32();
        SkipBytes(b, 28);
        TangentsData = b.ReadInt32();
        SkipBytes(b, 28);
        ShCoeffsData = b.ReadInt32();
        SkipBytes(b, 28);
        ShapeDeformationData = b.ReadInt32();
        SkipBytes(b, 28);
        BoneMapData = b.ReadInt32();
        SkipBytes(b, 28);
        FaceMapData = b.ReadInt32();
        SkipBytes(b, 28);
        VertMatsData = b.ReadInt32();
        SkipBytes(b, 28);
        QTangentsData = b.ReadInt32();
        SkipBytes(b, 28);
        SkinData = b.ReadInt32();
        SkipBytes(b, 28);
        Dummy2Data = b.ReadInt32();
        SkipBytes(b, 28);
        VertsUVsData = b.ReadInt32();          // This should be a vertsUV Chunk ID.
        SkipBytes(b, 28);
        for (int i = 0; i < 4; i++)
        {
            PhysicsData[i] = b.ReadInt32();
        }
        MinBound = b.ReadVector3();
        MaxBound = b.ReadVector3();
    }
}
