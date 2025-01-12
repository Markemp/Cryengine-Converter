using Extensions;
using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkMeshSubsets_900 : ChunkMeshSubsets
{
    public ChunkMeshSubsets_900(uint numVertSubsets)
    {
        NumMeshSubset = numVertSubsets;
    }

    public override void Read(BinaryReader b)
    {
        base.Read(b);

        //MeshSubsets = new MeshSubset[NumMeshSubset];
        for (int i = 0; i < NumMeshSubset; i++)
        {
            MeshSubset meshSubset = new()
            {
                MatID = b.ReadInt16(),
                Unknown = b.ReadUInt16(),  // 2 unknowns; possibly padding;
                FirstIndex = b.ReadInt32(),
                NumIndices = b.ReadInt32(),
                FirstVertex = b.ReadInt32(),
                NumVertices = b.ReadInt32(),
                Radius = b.ReadSingle(),
                Center = b.ReadVector3(),
                Unknown0 = b.ReadInt32(),
                Unknown1 = b.ReadInt32(),
                Unknown2 = b.ReadInt32()
            };
            MeshSubsets.Add(meshSubset);
        }
    }
}
