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

            this.ReadCompiledBones(b, 0);                        // Start reading at the root bone.  First bone found is root, then recursively get all the remaining ones.
            this.NumBones = this.BoneDictionary.Count();

            // Add the ChildID to the parent bone.  This will help with navigation. Also set up the TransformSoFar
            foreach (CompiledBone bone in BoneList)
            {
                AddChildIDToParent(bone);
            }
            // Setting the local transform matrices moved to when the bones are read.
                // Grab each of the children of the Root bone and calculate the LocalTransform matrix.  https://gamedev.stackexchange.com/questions/34076/global-transform-to-local-transform
                //SetRootBoneLocalTransformMatrix();            
                //CalculateLocalTransformMatrix(RootBone);
        }

        private void ReadCompiledBones(BinaryReader b, uint parentControllerID)        // Recursive call to read the bone at the current seek, and all children.
        {
            // Start reading all the properties of this bone.
            CompiledBone tempBone = new CompiledBone();
            // Utils.Log(LogLevelEnum.Debug, "** Current offset {0:X}", b.BaseStream.Position);
            tempBone.offset = b.BaseStream.Position;
            
            tempBone.ReadCompiledBone(b);
            tempBone.parentID = parentControllerID;
            tempBone.WriteCompiledBone();
            tempBone.ParentBone = GetParentBone(tempBone);
            // Calculate the LocalTransform matrix.
            tempBone.LocalTranslation = tempBone.boneToWorld.GetBoneToWorldTranslationVector();       // World positions of the bone
            tempBone.LocalRotation = tempBone.boneToWorld.GetBoneToWorldRotationMatrix();            // World rotation of the bone.
                                                                                                     //Vector3 localTranslation = tempBone.LocalTranslation - GetTranslation(GetParentBone(tempBone));
                                                                                                     //Matrix33 localRotation = tempBone.LocalRotation * GetRotation(GetParentBone(tempBone));
            Vector3 localTranslation;
            Matrix33 localRotation;
            if (tempBone.parentID != 0)
            {
                localRotation = GetParentBone(tempBone).boneToWorld.GetBoneToWorldRotationMatrix().ConjugateTransposeThisAndMultiply(tempBone.boneToWorld.GetBoneToWorldRotationMatrix());
                localTranslation = GetParentBone(tempBone).LocalRotation * (tempBone.LocalTranslation - GetParentBone(tempBone).boneToWorld.GetBoneToWorldTranslationVector());
            }
            else
            {
                localTranslation = tempBone.boneToWorld.GetBoneToWorldTranslationVector();
                localRotation = tempBone.boneToWorld.GetBoneToWorldRotationMatrix();
            }
            tempBone.LocalTransform = GetTransformFromParts(localTranslation, localRotation);
            this.BoneDictionary[tempBone.boneName] = tempBone;          // Add this bone to the dictionary.
            this.BoneList.Add(tempBone);                                // Add bone to list

            // Get the child bones.
            for (int i = 0; i < tempBone.numChildren; i++)
            {
                // If child offset is 1, then we're at the right position anyway.  If it's 2, you want to 584 bytes.  3 is (584*2)...
                // Move to the offset of child.  If there are no children, we shouldn't move at all.
                long nextBone = tempBone.offset + 584 * tempBone.offsetChild + (i * 584);
                b.BaseStream.Seek(nextBone, 0);
                this.ReadCompiledBones(b, tempBone.ControllerID);
            }
            // set root bone
            if (parentControllerID == 0)
            {
                this.RootBone = tempBone;
                this.RootBoneName = tempBone.boneName;
            }
        }

        /// <summary>
        /// Writes the results of common matrix math.  For testing purposes.
        /// </summary>
        /// <param name="localRotation">The matrix that the math functions will be applied to.</param>
        private void WriteMatrices(Matrix33 localRotation)
        {
            localRotation.WriteMatrix33("Regular");
            localRotation.Inverse().WriteMatrix33("Inverse");
            localRotation.Conjugate().WriteMatrix33("Conjugate");
            localRotation.ConjugateTranspose().WriteMatrix33("Conjugate Transpose");
        }
    }
}
