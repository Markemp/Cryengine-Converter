using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkMesh_801 : ChunkMesh
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
            this.VertsAnimID = b.ReadInt32();
            this.VerticesData = b.ReadInt32();
            this.NormalsData = b.ReadInt32();           // Chunk ID of the datastream for the normals for this mesh
            this.UVsData = b.ReadInt32();               // Chunk ID of the Normals datastream
            this.ColorsData = b.ReadInt32();
            this.Colors2Data = b.ReadInt32();
            this.IndicesData = b.ReadInt32();
            this.TangentsData = b.ReadInt32();
            this.SkipBytes(b, 16);
            for (Int32 i = 0; i < 4; i++)
            {
                this.PhysicsData[i] = b.ReadInt32();
            }
            this.VertsUVsData = b.ReadInt32();          // This should be a vertsUV Chunk ID.
            this.ShCoeffsData = b.ReadInt32();
            this.ShapeDeformationData = b.ReadInt32();
            this.BoneMapData = b.ReadInt32();
            this.FaceMapData = b.ReadInt32();
            this.MinBound.x = b.ReadSingle();
            this.MinBound.y = b.ReadSingle();
            this.MinBound.z = b.ReadSingle();
            this.MaxBound.x = b.ReadSingle();
            this.MaxBound.y = b.ReadSingle();
            this.MaxBound.z = b.ReadSingle();
        }
    }
}
