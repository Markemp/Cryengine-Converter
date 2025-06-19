using System.IO;
using CgfConverter.Models.Structs;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkCompiledMorphTargets_802 : ChunkCompiledMorphTargets
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        NumberOfMorphTargets = b.ReadUInt32();
        if (NumberOfMorphTargets > 0)
        {
            MorphTargetVertices = new MeshMorphTargetVertex[NumberOfMorphTargets];
            for (int i = 0; i < NumberOfMorphTargets; i++)
            {
                MorphTargetVertices[i] = MeshMorphTargetVertex.Read(b);
            }
        }
    }
}