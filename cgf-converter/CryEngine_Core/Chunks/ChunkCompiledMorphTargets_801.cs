using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkCompiledMorphTargets_801 : ChunkCompiledMorphTargets
    {
        // TODO:  Implement this.
        public override void Read(BinaryReader b)
        {
            //base.Read(b);
            //NumberOfMorphTargets = b.ReadUInt32();
            //if (NumberOfMorphTargets > 0)
            //{
            //    MorphTargetVertices = new MeshMorphTargetVertex[NumberOfMorphTargets];
            //    for (int i = 0; i < NumberOfMorphTargets; i++)
            //    {
            //        MorphTargetVertices[i] = MeshMorphTargetVertex.Read(b);
            //    }

            //}
            //SkinningInfo skin = GetSkinningInfo();
            //skin.MorphTargets = MorphTargetVertices.ToList();
        }

        public override void WriteChunk()
        {
            base.WriteChunk();
        }

    }
}
