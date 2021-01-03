using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkMeshSubsets_800 : ChunkMeshSubsets
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.Flags = b.ReadUInt32();   // Might be a ref to this chunk
            this.NumMeshSubset = b.ReadUInt32();  // number of mesh subsets
            this.SkipBytes(b, 8);
            this.MeshSubsets = new MeshSubset[NumMeshSubset];
            for (Int32 i = 0; i < NumMeshSubset; i++)
            {
                this.MeshSubsets[i].FirstIndex = b.ReadInt32();
                this.MeshSubsets[i].NumIndices = b.ReadInt32();
                this.MeshSubsets[i].FirstVertex = b.ReadInt32();
                this.MeshSubsets[i].NumVertices = b.ReadInt32();
                this.MeshSubsets[i].MatID = b.ReadInt32();
                this.MeshSubsets[i].Radius = b.ReadSingle();
                this.MeshSubsets[i].Center.x = b.ReadSingle();
                this.MeshSubsets[i].Center.y = b.ReadSingle();
                this.MeshSubsets[i].Center.z = b.ReadSingle();
            }
        }
    }
}
