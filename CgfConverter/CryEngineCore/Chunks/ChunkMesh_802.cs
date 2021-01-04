using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkMesh_802 : ChunkMesh
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.Flags1 = b.ReadInt32();
            this.Flags2 = b.ReadInt32();
            this.NumVertices = b.ReadInt32();
            this.NumIndices = b.ReadInt32();
            this.NumVertSubsets = b.ReadInt32();
            this.MeshSubsets = b.ReadInt32();           // Chunk ID of mesh subsets 
            this.SkipBytes(b, 4);
            this.VerticesData = b.ReadInt32();
            this.SkipBytes(b, 28);
            this.NormalsData = b.ReadInt32();           // Chunk ID of the datastream for the normals for this mesh
            this.SkipBytes(b, 28);
            this.UVsData = b.ReadInt32();               // Chunk ID of the Normals datastream
            this.SkipBytes(b, 28);
            this.ColorsData = b.ReadInt32();
            this.SkipBytes(b, 28);
            this.Colors2Data = b.ReadInt32();
            this.SkipBytes(b, 28);
            this.IndicesData = b.ReadInt32();
            this.SkipBytes(b, 28);
            this.TangentsData = b.ReadInt32();
            this.SkipBytes(b, 28);
            this.SkipBytes(b, 16);
            for (Int32 i = 0; i < 4; i++)
            {
                this.PhysicsData[i] = b.ReadInt32();
            }
            this.VertsUVsData = b.ReadInt32();          // This should be a vertsUV Chunk ID.
            this.SkipBytes(b, 28);
            this.ShCoeffsData = b.ReadInt32();
            this.SkipBytes(b, 28);
            this.ShapeDeformationData = b.ReadInt32();
            this.SkipBytes(b, 28);
            this.BoneMapData = b.ReadInt32();
            this.SkipBytes(b, 28);
            this.FaceMapData = b.ReadInt32();
            this.SkipBytes(b, 28);
            this.SkipBytes(b, 16);
            this.SkipBytes(b, 96);                      // Lots of unknown data here.
            this.MinBound.x = b.ReadSingle();
            this.MinBound.y = b.ReadSingle();
            this.MinBound.z = b.ReadSingle();
            this.MaxBound.x = b.ReadSingle();
            this.MaxBound.y = b.ReadSingle();
            this.MaxBound.z = b.ReadSingle();
        }
    }
}
