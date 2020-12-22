using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkMeshSubsets_80000800 : ChunkMeshSubsets
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.Flags = b.ReadUInt32();   // Might be a ref to this chunk
            this.NumMeshSubset = Utils.SwapUIntEndian(b.ReadUInt32());  // number of mesh subsets
            this.SkipBytes(b, 8);
            this.MeshSubsets = new MeshSubset[NumMeshSubset];
            for (Int32 i = 0; i < NumMeshSubset; i++)
            {
                this.MeshSubsets[i].FirstIndex = Utils.SwapUIntEndian(b.ReadUInt32());
                this.MeshSubsets[i].NumIndices = Utils.SwapUIntEndian(b.ReadUInt32());
                this.MeshSubsets[i].FirstVertex = Utils.SwapUIntEndian(b.ReadUInt32());
                this.MeshSubsets[i].NumVertices = Utils.SwapUIntEndian(b.ReadUInt32());
                this.MeshSubsets[i].MatID = Utils.SwapUIntEndian(b.ReadUInt32());
                this.MeshSubsets[i].Radius = Utils.SwapSingleEndian(b.ReadSingle());
                this.MeshSubsets[i].Center.x = Utils.SwapSingleEndian(b.ReadSingle());
                this.MeshSubsets[i].Center.y = Utils.SwapSingleEndian(b.ReadSingle());
                this.MeshSubsets[i].Center.z = Utils.SwapSingleEndian(b.ReadSingle());
            }
        }
    }
}
