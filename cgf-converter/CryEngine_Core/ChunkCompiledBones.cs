using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected void CalculateLocalTransformMatrix(CompiledBone bone)
        {
            // The boneToWorld matrix is the world space of the bone.  We need the object space transform matrix for Collada.
            if (bone.parentID != 0)
            {
                CompiledBone parentBone = GetParentBone(bone);
                // Calculate translation (m14, m24, m34).
                bone.LocalTransform.m14 = bone.boneToWorld.boneToWorld[0, 3] - parentBone.boneToWorld.boneToWorld[0, 3];
                bone.LocalTransform.m24 = bone.boneToWorld.boneToWorld[1, 3] - parentBone.boneToWorld.boneToWorld[1, 3];
                bone.LocalTransform.m34 = bone.boneToWorld.boneToWorld[2, 3] - parentBone.boneToWorld.boneToWorld[2, 3];

                // Calculate scale (m41, m42, m43)
                // This will always be 0, 0, 0 for bones.
                bone.LocalTransform.m41 = 0;
                bone.LocalTransform.m42 = 0;
                bone.LocalTransform.m43 = 0;
                // calculate rotation

            }
            if (bone.numChildren > 0)
            {
                foreach (CompiledBone childBone in GetAllChildBones(bone))
                {
                    CalculateLocalTransformMatrix(childBone);
                }
            }
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
