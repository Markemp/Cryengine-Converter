using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkCompiledPhysicalBones_800 : ChunkCompiledPhysicalBones     //  0xACDC0000:  Bones info
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            this.SkipBytes(b, 32);  // Padding between the chunk header and the first bone.
            this.NumBones = (int)((this.Size - 32) / 152);

            for (uint i = 0; i < NumBones; i++)
            {
                // Start reading at the root bone.  First bone found is root, then read until no more bones.
                CompiledPhysicalBone tmpBone = new CompiledPhysicalBone();
                tmpBone.ReadCompiledPhysicalBone(b);
                // Set root bone if not already set
                if (RootPhysicalBone != null)
                {
                    RootPhysicalBone = tmpBone;
                }
                PhysicalBoneList.Add(tmpBone);
                PhysicalBoneDictionary[i] = tmpBone;
            }

            // Add the ChildID to the parent bone.  This will help with navigation. Also set up the TransformSoFar
            foreach (CompiledPhysicalBone bone in PhysicalBoneList)
            {
                AddChildIDToParent(bone);
            }
        }

        public override void WriteChunk()
        {
            base.WriteChunk();
        }
    }

}
