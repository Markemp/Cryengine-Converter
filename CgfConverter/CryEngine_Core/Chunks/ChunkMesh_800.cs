using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public class ChunkMesh_800 : ChunkMesh
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.NumVertSubsets = 1;
            this.SkipBytes(b, 8);
            this.NumVertices = b.ReadInt32();
            this.NumIndices = b.ReadInt32();   //  Number of indices
            this.SkipBytes(b, 4);
            this.MeshSubsets = b.ReadInt32();  // refers to ID in mesh subsets  1d for candle.  Just 1 for 0x800 type
            this.SkipBytes(b, 4);
            this.VerticesData = b.ReadInt32();  // ID of the datastream for the vertices for this mesh
            this.NormalsData = b.ReadInt32();   // ID of the datastream for the normals for this mesh
            this.UVsData = b.ReadInt32();     // refers to the ID in the Normals datastream?
            this.ColorsData = b.ReadInt32();
            this.Colors2Data = b.ReadInt32();
            this.IndicesData = b.ReadInt32();
            this.TangentsData = b.ReadInt32();
            this.ShCoeffsData = b.ReadInt32();
            this.ShapeDeformationData = b.ReadInt32();
            this.BoneMapData = b.ReadInt32();
            this.FaceMapData = b.ReadInt32();
            this.VertMatsData = b.ReadInt32();
            this.SkipBytes(b, 16);
            for (Int32 i = 0; i < 4; i++)
            {
                this.PhysicsData[i] = b.ReadInt32();
                if (this.PhysicsData[i] != 0)
                    this.MeshPhysicsData = this.PhysicsData[i];
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
