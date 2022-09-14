﻿using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkMeshSubsets_80000800 : ChunkMeshSubsets
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        Flags = Utils.SwapUIntEndian(b.ReadUInt32());   // Might be a ref to this chunk
        NumMeshSubset = Utils.SwapUIntEndian(b.ReadUInt32());  // number of mesh subsets
        SkipBytes(b, 8);
        MeshSubsets = new MeshSubset[NumMeshSubset];
        for (int i = 0; i < NumMeshSubset; i++)
        {
            MeshSubsets[i].FirstIndex = Utils.SwapIntEndian(b.ReadInt32());
            MeshSubsets[i].NumIndices = Utils.SwapIntEndian(b.ReadInt32());
            MeshSubsets[i].FirstVertex = Utils.SwapIntEndian(b.ReadInt32());
            MeshSubsets[i].NumVertices = Utils.SwapIntEndian(b.ReadInt32());
            MeshSubsets[i].MatID = Utils.SwapIntEndian(b.ReadInt32());
            MeshSubsets[i].Radius = Utils.SwapSingleEndian(b.ReadSingle());
            MeshSubsets[i].Center.X = Utils.SwapSingleEndian(b.ReadSingle());
            MeshSubsets[i].Center.Y = Utils.SwapSingleEndian(b.ReadSingle());
            MeshSubsets[i].Center.Z = Utils.SwapSingleEndian(b.ReadSingle());
        }
    }
}
