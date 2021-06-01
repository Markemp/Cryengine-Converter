using System.IO;

namespace CgfConverter.CryEngineCore
{
    class ChunkMeshSubsets_900 : ChunkMeshSubsets
    {
        public ChunkMeshSubsets_900(uint numVertSubsets)
        {
            NumMeshSubset = numVertSubsets;
        }

        public override void Read(BinaryReader b)
        {
            base.Read(b);

            // NumMeshSubset = b.ReadUInt32();  // number of mesh subsets
            MeshSubsets = new MeshSubset[NumMeshSubset];
            for (int i = 0; i < NumMeshSubset; i++)
            {
                MeshSubsets[i].MatID = b.ReadInt32();
                MeshSubsets[i].FirstIndex = b.ReadInt32();
                MeshSubsets[i].NumIndices = b.ReadInt32();
                MeshSubsets[i].FirstVertex = b.ReadInt32();
                MeshSubsets[i].NumVertices = b.ReadInt32();
                MeshSubsets[i].Radius = b.ReadSingle();
                MeshSubsets[i].Center.x = b.ReadSingle();
                MeshSubsets[i].Center.y = b.ReadSingle();
                MeshSubsets[i].Center.z = b.ReadSingle();
                SkipBytes(b, 12);  // 3 unknowns; possibly floats;
            }
        }
    
    }
}
