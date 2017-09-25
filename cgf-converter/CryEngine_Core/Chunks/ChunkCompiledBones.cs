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
        
        // Bones are a bit different than Node Chunks, since there is only one CompiledBones Chunk, and it contains all the bones in the model.
        public Dictionary<int, CompiledBone> BoneDictionary = new Dictionary<int, CompiledBone>();  // Dictionary of all the CompiledBone objects based on parent offset(?).
        public List<CompiledBone> BoneList = new List<CompiledBone>();

        public CompiledBone GetParentBone(CompiledBone bone, int boneIndex)
        {
            // Should only be one parent.
            if (bone.offsetParent != 0)
            {
                return BoneDictionary[boneIndex + bone.offsetParent];
                //return BoneList.Where(a => a.ControllerID == bone.parentID).First();
            }
            else
                return null;
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

        public List<string> GetBoneNames()
        {
            return BoneList.Select(a => a.boneName).ToList();  // May need to replace space in bone names with _.
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
