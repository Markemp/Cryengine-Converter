using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkMesh_800 : ChunkMesh
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.NumVertSubsets = 1;
            this.SkipBytes(b, 8);
            this.NumVertices = b.ReadUInt32();
            this.NumIndices = b.ReadUInt32();   //  Number of indices
            this.SkipBytes(b, 4);
            this.MeshSubsets = b.ReadUInt32();  // refers to ID in mesh subsets  1d for candle.  Just 1 for 0x800 type
            this.SkipBytes(b, 4);
            this.VerticesData = b.ReadUInt32();  // ID of the datastream for the vertices for this mesh
            this.NormalsData = b.ReadUInt32();   // ID of the datastream for the normals for this mesh
            this.UVsData = b.ReadUInt32();     // refers to the ID in the Normals datastream?
            this.ColorsData = b.ReadUInt32();
            this.Colors2Data = b.ReadUInt32();
            this.IndicesData = b.ReadUInt32();
            this.TangentsData = b.ReadUInt32();
            this.ShCoeffsData = b.ReadUInt32();
            this.ShapeDeformationData = b.ReadUInt32();
            this.BoneMapData = b.ReadUInt32();
            this.FaceMapData = b.ReadUInt32();
            this.VertMatsData = b.ReadUInt32();
            this.SkipBytes(b, 16);
            for (Int32 i = 0; i < 4; i++)
            {
                this.PhysicsData[i] = b.ReadUInt32();
            }
            this.MinBound.x = b.ReadSingle();
            this.MinBound.y = b.ReadSingle();
            this.MinBound.z = b.ReadSingle();
            this.MaxBound.x = b.ReadSingle();
            this.MaxBound.y = b.ReadSingle();
            this.MaxBound.z = b.ReadSingle();
        }
    }
}
