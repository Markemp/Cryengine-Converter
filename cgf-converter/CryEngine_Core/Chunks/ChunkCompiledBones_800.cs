﻿using System;
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
            Vector3 localTranslation;
            Matrix33 localRotation;

            //  Read the first bone with ReadCompiledBone, then recursively grab all the children for each bone you find.
            //  Each bone structure is 584 bytes, so will need to seek childOffset * 584 each time, and go back.
            NumBones = (int)((this.Size - 32) / 584);
            for (int i = 0; i < NumBones; i++)
            {
                CompiledBone tempBone = new CompiledBone();
                tempBone.ReadCompiledBone(b);
                if (RootBone == null)
                {
                    this.RootBone = tempBone;
                }
                tempBone.LocalTranslation = tempBone.boneToWorld.GetBoneToWorldTranslationVector();       // World positions of the bone
                tempBone.LocalRotation = tempBone.boneToWorld.GetBoneToWorldRotationMatrix();            // World rotation of the bone.
                tempBone.ParentBone = GetParentBone(tempBone);
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

                BoneList.Add(tempBone);
                BoneDictionary[i] = tempBone;
            }

            // Add the ChildID to the parent bone.  This will help with navigation. Also set up the TransformSoFar
            foreach (CompiledBone bone in BoneList)
            {
                AddChildIDToParent(bone);
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
