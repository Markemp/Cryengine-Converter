using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkCompiledMorphTargets_800 : ChunkCompiledMorphTargets
    {
        // TODO:  Implement this.
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
            SkinningInfo skin = GetSkinningInfo();
            //skin.MorphTargets = MorphTargetVertices.ToList();
        }
    }
}
