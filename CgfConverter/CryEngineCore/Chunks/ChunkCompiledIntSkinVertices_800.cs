using Extensions;
using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkCompiledIntSkinVertices_800 : ChunkCompiledIntSkinVertices
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
            ushort[] boneIndices = new ushort[4];
            for (int j = 0; j < 4; j++)
            {
                boneIndices[j] = b.ReadUInt16();
            }

            // Read the weights for those bone IDs
            float[] weights = new float[4];
            for (int j = 0; j < 4; j++)
            {
                weights[j] = b.ReadSingle();
            }

            // Create MeshBoneMapping with required properties
            IntSkinVertices[i].BoneMapping = new MeshBoneMapping
            {
                BoneIndex = boneIndices,
                Weight = weights
            };

            // Read the color
            IntSkinVertices[i].Color = b.ReadIRGBA();
        }
    }
}
