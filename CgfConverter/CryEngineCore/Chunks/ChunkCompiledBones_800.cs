using System.Collections.Generic;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkCompiledBones_800 : ChunkCompiledBones
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            SkipBytes(b, 32);  // Padding between the chunk header and the first bone.
            Vector3 localTranslation;
            Matrix33 localRotation;

            //  Read the first bone with ReadCompiledBone, then recursively grab all the children for each bone you find.
            //  Each bone structure is 584 bytes, so will need to seek childOffset * 584 each time, and go back.
            NumBones = (int)((Size - 32) / 584);
            for (int i = 0; i < NumBones; i++)
            {
                CompiledBone tempBone = new CompiledBone();
                tempBone.ReadCompiledBone(b);

                if (RootBone == null)  // First bone read is root bone
                    RootBone = tempBone;

                tempBone.LocalTranslation = tempBone.boneToWorld.GetBoneToWorldTranslationVector();       // World positions of the bone
                tempBone.LocalRotation = tempBone.boneToWorld.GetBoneToWorldRotationMatrix();            // World rotation of the bone.
                
                if (tempBone.offsetParent != 0)
                    tempBone.ParentBone = BoneList[i + tempBone.offsetParent];
                
                if (tempBone.ParentBone != null)
                {
                    tempBone.parentID = tempBone.ParentBone.ControllerID;
                }
                else
                {
                    tempBone.parentID = 0;
                }

                if (tempBone.parentID != 0)
                {
                    localRotation = GetParentBone(tempBone).boneToWorld
                        .GetBoneToWorldRotationMatrix()
                        .ConjugateTransposeThisAndMultiply(tempBone.boneToWorld.GetBoneToWorldRotationMatrix());
                    localTranslation = GetParentBone(tempBone)
                        .LocalRotation * (tempBone.LocalTranslation - GetParentBone(tempBone)
                        .boneToWorld.GetBoneToWorldTranslationVector());
                }
                else
                {
                    localTranslation = tempBone.boneToWorld.GetBoneToWorldTranslationVector();
                    localRotation = tempBone.boneToWorld.GetBoneToWorldRotationMatrix();
                }
                tempBone.LocalTransform = GetTransformFromParts(localTranslation, localRotation);

                BoneList.Add(tempBone);
                BoneDictionary[i] = tempBone;
            }

            // Add the ChildID to the parent bone.  This will help with navigation. Also set up the TransformSoFar
            foreach (CompiledBone bone in BoneList)
            {
                AddChildIDToParent(bone);
            }

            SkinningInfo skin = GetSkinningInfo();
            skin.CompiledBones = new List<CompiledBone>();
            skin.HasSkinningInfo = true;
            skin.CompiledBones = BoneList;
        }
    }
}
