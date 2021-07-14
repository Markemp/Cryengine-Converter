﻿using Extensions;
using System.IO;
using System.Linq;

namespace CgfConverter.CryEngineCore
{
    public class ChunkCompiledIntSkinVertices_800 : ChunkCompiledIntSkinVertices
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            NumIntVertices = (int)((Size - 32) / 64);
            IntSkinVertices = new IntSkinVertex[NumIntVertices];
            SkipBytes(b, 32);          // Padding between the chunk header and the first IntVertex.
            // Size of the IntSkinVertex is 64 bytes
            for (int i = 0; i < NumIntVertices; i++)
            {
                IntSkinVertices[i].Obsolete0 = b.ReadVector3();
                IntSkinVertices[i].Position = b.ReadVector3();
                IntSkinVertices[i].Obsolete2 = b.ReadVector3();
                // Read 4 bone IDs
                IntSkinVertices[i].BoneIDs = new ushort[4];
                for (int j = 0; j < 4; j++)
                {
                    IntSkinVertices[i].BoneIDs[j] = b.ReadUInt16();
                }
                // Read the weights for those bone IDs
                IntSkinVertices[i].Weights = new float[4];
                for (int j = 0; j < 4; j++)
                {
                    IntSkinVertices[i].Weights[j] = b.ReadSingle();
                }
                // Read the color
                IntSkinVertices[i].Color.Read(b);
            }
            SkinningInfo skin = GetSkinningInfo();
            skin.IntVertices = IntSkinVertices.ToList();
        }
    }
}
