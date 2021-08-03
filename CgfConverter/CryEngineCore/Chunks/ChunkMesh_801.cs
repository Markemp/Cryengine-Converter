using Extensions;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkMesh_801 : ChunkMesh
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            Flags1 = b.ReadInt32();
            Flags2 = b.ReadInt32();
            NumVertices = b.ReadInt32();
            NumIndices = b.ReadInt32();
            NumVertSubsets = b.ReadUInt32();
            MeshSubsetsData = b.ReadInt32();           // Chunk ID of mesh subsets 
            VertsAnimID = b.ReadInt32();
            VerticesData = b.ReadInt32();
            NormalsData = b.ReadInt32();           // Chunk ID of the datastream for the normals for this mesh
            UVsData = b.ReadInt32();               // Chunk ID of the Normals datastream
            ColorsData = b.ReadInt32();
            Colors2Data = b.ReadInt32();
            IndicesData = b.ReadInt32();
            TangentsData = b.ReadInt32();
            ShCoeffsData = b.ReadInt32();
            ShapeDeformationData = b.ReadInt32();
            BoneMapData = b.ReadInt32();
            FaceMapData = b.ReadInt32();
            b.ReadInt32();                          // VertsMat
            QTangentsData = b.ReadInt32();          // QTangents
            b.ReadInt32();                          // Skin Data
            b.ReadInt32();                          // Dummy2 (obsolete console data)
            VertsUVsData = b.ReadInt32();           // This should be a vertsUV Chunk ID.
            ShCoeffsData = b.ReadInt32();
            ShapeDeformationData = b.ReadInt32();
            BoneMapData = b.ReadInt32();
            FaceMapData = b.ReadInt32();
            MinBound = b.ReadVector3();
            MaxBound = b.ReadVector3();
        }
    }
}
