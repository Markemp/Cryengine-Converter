using System.Collections.Generic;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    class ChunkCompiledBones_801 : ChunkCompiledBones
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            SkipBytes(b, 32);  // Padding between the chunk header and the first bone.
            Vector3 localTranslation;
            Matrix3x3 localRotation;

            //  Read the first bone with ReadCompiledBone, then recursively grab all the children for each bone you find.
            //  Each bone structure is 324 bytes, so will need to seek childOffset * 324 each time, and go back.
            NumBones = (int)((Size - 48) / 324);
            for (int i = 0; i < NumBones; i++)
            {
                var tempBone = new CompiledBone();
                tempBone.ReadCompiledBone_801(b);
                if (RootBone == null)  // First bone read is root bone
                    RootBone = tempBone;

                tempBone.LocalTranslation = tempBone.boneToWorld.GetBoneToWorldTranslationVector();       // World positions of the bone
                tempBone.LocalRotation = tempBone.boneToWorld.GetBoneToWorldRotationMatrix();            // World rotation of the bone.
                //tempBone.ParentBone = BoneMap[i + tempBone.offsetParent];
                tempBone.ParentBone = GetParentBone(tempBone);
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
