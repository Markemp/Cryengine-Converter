﻿using System.IO;

namespace CgfConverter.CryEngineCore;

// Mesh chunk for console with swapped endianness.
internal sealed class ChunkMesh_80000800 : ChunkMesh
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        NumVertSubsets = 1;
        SkipBytes(b, 8);
        NumVertices = Utils.SwapIntEndian(b.ReadInt32());
        NumIndices = Utils.SwapIntEndian(b.ReadInt32());   //  Number of indices
        SkipBytes(b, 4);
        MeshSubsetsData = Utils.SwapIntEndian(b.ReadInt32());  // refers to ID in mesh subsets  1d for candle.  Just 1 for 0x800 type
        SkipBytes(b, 4);
        VerticesData = Utils.SwapIntEndian(b.ReadInt32());  // ID of the datastream for the vertices for this mesh
        NormalsData = Utils.SwapIntEndian(b.ReadInt32());   // ID of the datastream for the normals for this mesh
        UVsData = Utils.SwapIntEndian(b.ReadInt32());     // refers to the ID in the Normals datastream?
        ColorsData = Utils.SwapIntEndian(b.ReadInt32());
        Colors2Data = Utils.SwapIntEndian(b.ReadInt32());
        IndicesData = Utils.SwapIntEndian(b.ReadInt32());
        TangentsData = Utils.SwapIntEndian(b.ReadInt32());
        ShCoeffsData = Utils.SwapIntEndian(b.ReadInt32());
        ShapeDeformationData = Utils.SwapIntEndian(b.ReadInt32());
        BoneMapData = Utils.SwapIntEndian(b.ReadInt32());
        FaceMapData = Utils.SwapIntEndian(b.ReadInt32());
        VertMatsData = Utils.SwapIntEndian(b.ReadInt32());
        SkipBytes(b, 16);
        for (int i = 0; i < 4; i++)
        {
            PhysicsData[i] = Utils.SwapIntEndian(b.ReadInt32());
            if (PhysicsData[i] != 0)
                MeshPhysicsData = PhysicsData[i];
        }
        MinBound.X = Utils.SwapSingleEndian(b.ReadSingle());
        MinBound.Y = Utils.SwapSingleEndian(b.ReadSingle());
        MinBound.Z = Utils.SwapSingleEndian(b.ReadSingle());
        MaxBound.X = Utils.SwapSingleEndian(b.ReadSingle());
        MaxBound.Y = Utils.SwapSingleEndian(b.ReadSingle());
        MaxBound.Z = Utils.SwapSingleEndian(b.ReadSingle());
    }
}
