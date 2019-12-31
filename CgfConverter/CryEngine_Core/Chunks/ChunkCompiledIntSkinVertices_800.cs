using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public class ChunkCompiledIntSkinVertices_800 : ChunkCompiledIntSkinVertices
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            NumIntVertices = (int)((this.Size - 32) / 64);
            IntSkinVertices = new IntSkinVertex[NumIntVertices];
            this.SkipBytes(b, 32);          // Padding between the chunk header and the first IntVertex.
            // Size of the IntSkinVertex is 64 bytes
            for (int i = 0; i < NumIntVertices; i++)
            {
                IntSkinVertices[i].Obsolete0.ReadVector3(b);
                IntSkinVertices[i].Position.ReadVector3(b);
                IntSkinVertices[i].Obsolete2.ReadVector3(b);
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
