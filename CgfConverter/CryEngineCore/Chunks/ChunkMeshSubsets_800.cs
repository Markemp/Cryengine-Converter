using Extensions;
using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkMeshSubsets_800 : ChunkMeshSubsets
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        Flags = b.ReadUInt32();   // Might be a ref to this chunk
        NumMeshSubset = b.ReadUInt32();  // number of mesh subsets
        SkipBytes(b, 8);
        //MeshSubsets = new MeshSubset[NumMeshSubset];
        for (int i = 0; i < NumMeshSubset; i++)
        {
            MeshSubset meshSubset = new()
            {
                FirstIndex = b.ReadInt32(),
                NumIndices = b.ReadInt32(),
                FirstVertex = b.ReadInt32(),
                NumVertices = b.ReadInt32(),
                MatID = b.ReadInt32(),
                Radius = b.ReadSingle(),
                Center = b.ReadVector3(),
            };
            MeshSubsets.Add(meshSubset);
        }
    }
}
