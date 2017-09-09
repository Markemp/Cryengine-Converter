using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkCompiledBones : Chunk     //  0xACDC0000:  Bones info
    {
        public String RootBoneName;         // Controller ID?  Name?  Not sure yet.
        public CompiledBone RootBone;       // First bone in the data structure.  Usually Bip01
        public int NumBones;                // Number of bones in the chunk
        
        // Bone info
        // Bones are a bit different than Node Chunks, since there is only one CompiledBones Chunk, and it contains all the bones in the model.
        public Dictionary<String, CompiledBone> BoneDictionary = new Dictionary<String, CompiledBone>();  // Dictionary of all the CompiledBone objects based on bone name.
        public List<CompiledBone> BoneList = new List<CompiledBone>();

        public CompiledBone GetRootBone(CompiledBone bone)
        {
            if (bone.parentID != 0)
            {
                return BoneList.Where(a => a.parentID == bone.parentID).FirstOrDefault();
            }
            else 
                return bone;                // No parent bone found, so just returning itself.  CompiledBone is non-nullable.
        }

        public CompiledBone GetParentBone(CompiledBone bone)
        {
            // Should only be one parent.
            if (bone.parentID != 0)
            {
                return BoneList.Where(a => a.ControllerID == bone.parentID).First();
            }
            else
                return bone;
        }

        public List<CompiledBone> GetAllChildBones(CompiledBone bone)
        {
            if (bone.numChildren > 0)
            {
                return BoneList.Where(a => bone.childIDs.Contains(a.ControllerID)).ToList();
            }
            else
                return null;
        }

        protected void AddChildIDToParent(CompiledBone bone)
        {
            // Root bone parent ID will be zero.
            if (bone.parentID != 0)
            {
                CompiledBone parent = BoneList.Where(a => a.ControllerID == bone.parentID).FirstOrDefault();  // Should only be one parent.
                parent.childIDs.Add(bone.ControllerID);
            }
        }

        protected Vector3 CalculateLocalTransformMatrix(CompiledBone bone)
        {
            // The boneToWorld matrix is the world space of the bone.  We need the object space transform matrix for Collada.
            if (bone.parentID != 0)
            {
                return CalculateLocalTransformMatrix(GetParentBone(bone)).Add(bone.boneToWorld.GetBoneToWorldTranslationVector());
            }
            else
            {
                // TODO: What should this be?
                // return this._model.RootNode.Transform.GetTranslation();
                return bone.boneToWorld.GetBoneToWorldTranslationVector();
            }
        }
        /*
            //CompiledBone parentBone = GetParentBone(bone);
            //// Take the boneToWorld info and put it into the Matrix44 structure for the bone.
            //PopulateTransformMatrix(bone);
            //// Calculate translation (m14, m24, m34).
            //bone.LocalTransform.m14 = bone.boneToWorld.boneToWorld[0, 3] - parentBone.boneToWorld.boneToWorld[0, 3];
            //bone.LocalTransform.m24 = bone.boneToWorld.boneToWorld[1, 3] - parentBone.boneToWorld.boneToWorld[1, 3];
            //bone.LocalTransform.m34 = bone.boneToWorld.boneToWorld[2, 3] - parentBone.boneToWorld.boneToWorld[2, 3];
            ////bone.LocalTranslation = bone.LocalTransform.GetBoneTranslation().ToMathVector3();
            //bone.LocalTranslation[0] = bone.LocalTransform.m14;
            //bone.LocalTranslation[1] = bone.LocalTransform.m24;
            //bone.LocalTranslation[2] = bone.LocalTransform.m34;
            //// Calculate scale (m41, m42, m43)
            //// This will always be 0, 0, 0 for bones.
            //bone.LocalTransform.m41 = 0;
            //bone.LocalTransform.m42 = 0;
            //bone.LocalTransform.m43 = 0;
            // calculate rotation
            //bone.LocalRotation =  parentBone.LocalTransform.GetRotation().ConjugateTransposeAndMultiply(bone.LocalTransform.GetRotation());
            //bone.LocalRotation = bone.LocalTransform.GetRotation().ConjugateTransposeAndMultiply(parentBone.LocalTransform.GetRotation());
            //bone.LocalRotation = bone.LocalTransform.GetRotation() * parentBone.LocalTransform.GetRotation().Inverse();
            //bone.LocalRotation = bone.LocalTransform.GetRotation().Inverse() * parentBone.LocalTransform.GetRotation();
            //bone.LocalRotation = bone.LocalTransform.GetRotation() * parentBone.LocalTransform.GetRotation().Inverse();
            //bone.LocalRotation = bone.LocalTransform.GetRotation() * parentBone.LocalTransform.GetRotation().Conjugate();
            //bone.LocalRotation = bone.LocalTransform.GetRotationMath().Inverse() * parentBone.LocalTransform.GetRotationMath().Inverse().Conjugate();
            //bone.LocalRotation = bone.boneToWorld.GetBoneToWorldRotationMatrix() * parentBone.boneToWorld.GetBoneToWorldRotationMatrix().Conjugate();
        //else
        //{
        //    // Root bone.  Set the bone.LocalRotation to the boneToWorld values
        //    bone.LocalRotation[0, 0] = bone.boneToWorld.boneToWorld[0, 0];
        //    bone.LocalRotation[0, 1] = bone.boneToWorld.boneToWorld[0, 1];
        //    bone.LocalRotation[0, 2] = bone.boneToWorld.boneToWorld[0, 2];
        //    bone.LocalRotation[1, 0] = bone.boneToWorld.boneToWorld[1, 0];
        //    bone.LocalRotation[1, 1] = bone.boneToWorld.boneToWorld[1, 1];
        //    bone.LocalRotation[1, 2] = bone.boneToWorld.boneToWorld[1, 2];
        //    bone.LocalRotation[2, 0] = bone.boneToWorld.boneToWorld[2, 0];
        //    bone.LocalRotation[2, 1] = bone.boneToWorld.boneToWorld[2, 1];
        //    bone.LocalRotation[2, 2] = bone.boneToWorld.boneToWorld[2, 2];
        //} // Root bone.
        //if (bone.numChildren > 0)
        //{
        //    foreach (CompiledBone childBone in GetAllChildBones(bone))
        //    {
        //        CalculateLocalTransformMatrix(childBone);
        //    }
    //    }
    //}*/


        /// <summary>
        /// Gets the transform of the Bone.  This will be both the rotation and translation of the bone, plus all the parents.
        /// 
        /// The transform matrix is a 4x4 matrix.  Vector3 is a 3x1.  We need to convert vector3 to vector4, multiply the matrix, then convert back to vector3.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Vector3 GetBoneTransform(Vector3 transform)
        {
            Vector3 vec3 = transform;

            // if (this.id != 0xFFFFFFFF)
            // {

            // Apply the local transforms (rotation and translation) to the vector
            // Do rotations.  Rotations must come first, then translate.
            //vec3 = this.RotSoFar.Mult3x1(vec3);
            // Do translations.  I think this is right.  Objects in right place, not rotated right.
            //vec3 = vec3.Add(this.TransformSoFar);
            //}

            return vec3;
        }

        protected Matrix44 GetTransformFromParts(Vector3 localTranslation, Matrix33 localRotation)
        {
            Matrix44 transform = new Matrix44
            {
                // Translation part
                m14 = localTranslation.x,
                m24 = localTranslation.y,
                m34 = localTranslation.z,
                // Rotation part
                m11 = localRotation.m11,
                m12 = localRotation.m12,
                m13 = localRotation.m13,
                m21 = localRotation.m21,
                m22 = localRotation.m22,
                m23 = localRotation.m23,
                m31 = localRotation.m31,
                m32 = localRotation.m32,
                m33 = localRotation.m33,
                // Set final row
                m41 = 0,
                m42 = 0,
                m43 = 0,
                m44 = 1
            };
            return transform;
        }

        protected void SetRootBoneLocalTransformMatrix()
        {
            RootBone.LocalTransform.m11 = RootBone.boneToWorld.boneToWorld[0, 0];
            RootBone.LocalTransform.m12 = RootBone.boneToWorld.boneToWorld[0, 1];
            RootBone.LocalTransform.m13 = RootBone.boneToWorld.boneToWorld[0, 2];
            RootBone.LocalTransform.m14 = RootBone.boneToWorld.boneToWorld[0, 3];
            RootBone.LocalTransform.m21 = RootBone.boneToWorld.boneToWorld[1, 0];
            RootBone.LocalTransform.m22 = RootBone.boneToWorld.boneToWorld[1, 1];
            RootBone.LocalTransform.m23 = RootBone.boneToWorld.boneToWorld[1, 2];
            RootBone.LocalTransform.m24 = RootBone.boneToWorld.boneToWorld[1, 3];
            RootBone.LocalTransform.m31 = RootBone.boneToWorld.boneToWorld[2, 0];
            RootBone.LocalTransform.m32 = RootBone.boneToWorld.boneToWorld[2, 1];
            RootBone.LocalTransform.m33 = RootBone.boneToWorld.boneToWorld[2, 2];
            RootBone.LocalTransform.m34 = RootBone.boneToWorld.boneToWorld[2, 3];
            RootBone.LocalTransform.m41 = 0;
            RootBone.LocalTransform.m42 = 0;
            RootBone.LocalTransform.m43 = 0;
            RootBone.LocalTransform.m44 = 1;
        }

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Debug, "*** START CompiledBone Chunk ***");
            Utils.Log(LogLevelEnum.Debug, "    ChunkType:           {0}", ChunkType);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", ID);
        }
    }

}
