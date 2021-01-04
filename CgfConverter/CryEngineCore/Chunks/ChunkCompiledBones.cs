using System;
using System.Collections.Generic;
using System.Linq;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkCompiledBones : Chunk     //  0xACDC0000:  Bones info
    {
        public string RootBoneName;         // Controller ID?  Name?  Not sure yet.
        public CompiledBone RootBone;       // First bone in the data structure.  Usually Bip01
        public int NumBones;                // Number of bones in the chunk

        // Bones are a bit different than Node Chunks, since there is only one CompiledBones Chunk, and it contains all the bones in the model.
        public Dictionary<int, CompiledBone> BoneDictionary = new Dictionary<int, CompiledBone>();  // Dictionary of all the CompiledBone objects based on parent offset(?).
        public List<CompiledBone> BoneList = new List<CompiledBone>();

        public virtual CompiledBone GetParentBone(CompiledBone bone)
        {
            // Should only be one parent.
            if (bone.offsetParent != 0)
            {
                return BoneList.Where(a => a.ControllerID == bone.parentID).FirstOrDefault();
                //return BoneDictionary[boneIndex + bone.offsetParent];
            }
            else
                return null;
        }

        public List<CompiledBone> GetAllChildBones(CompiledBone bone)
        {
            return BoneList.Where(a => bone.childIDs.Contains(a.ControllerID)).ToList();
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

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}";
        }
    }
}
