using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkCompiledBones_800 : ChunkCompiledBones
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.SkipBytes(b, 32);  // No idea what these are for, but it's padding between the chunk header and the first bone.

            //  Read the first bone with ReadCompiledBone, then recursively grab all the children for each bone you find.
            //  Each bone structure is 584 bytes, so will need to seek childOffset * 584 each time, and go back.

            this.GetCompiledBones(b, 0);                        // Start reading at the root bone.  First bone found is root, then recursively get all the remaining ones.
            this.NumBones = this.BoneDictionary.Count();

            // Add the ChildID to the parent bone.  This will help with navigation. Also set up the TransformSoFar
            foreach (CompiledBone bone in BoneList)
            {
                AddChildIDToParent(bone);
                // Calculate the TransformSoFar for each bone.
                if (bone.parentID != 0)
                {
                    Matrix44 testmatrix =  GetParentBone(bone).BoneTransform * bone.BoneTransform ;
                    //bone.TransformSoFar = bone.BoneTransform * GetParentBone(bone).BoneTransform;
                }

            }
        }

        private void GetCompiledBones(BinaryReader b, uint parentControllerID)        // Recursive call to read the bone at the current seek, and all children.
        {
            // Start reading all the properties of this bone.
            CompiledBone tempBone = new CompiledBone();
            // Utils.Log(LogLevelEnum.Debug, "** Current offset {0:X}", b.BaseStream.Position);
            tempBone.offset = b.BaseStream.Position;
            
            tempBone.ReadCompiledBone(b);
            tempBone.parentID = parentControllerID;
            tempBone.WriteCompiledBone();
            //tempBone.childIDs = new UInt32[tempBone.numChildren];
            this.BoneDictionary[tempBone.boneName] = tempBone;         // Add this bone to the dictionary.

            for (int i = 0; i < tempBone.numChildren; i++)
            {
                // If child offset is 1, then we're at the right position anyway.  If it's 2, you want to 584 bytes.  3 is (584*2)...
                // Move to the offset of child.  If there are no children, we shouldn't move at all.
                long nextBone = tempBone.offset + 584 * tempBone.offsetChild + (i * 584);
                b.BaseStream.Seek(nextBone, 0);
                this.GetCompiledBones(b, tempBone.controllerID);
            }
            // set root bone
            if (parentControllerID == 0)
            {
                this.RootBone = tempBone;
                this.RootBoneName = tempBone.boneName;
            }
                
            // Add bone to list
            this.BoneList.Add(tempBone);

            

        }


    }
}
