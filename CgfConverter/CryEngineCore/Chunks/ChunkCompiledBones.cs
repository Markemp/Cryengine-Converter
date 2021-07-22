using CgfConverter.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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

        public List<CompiledBone> GetAllChildBones(CompiledBone bone)
        {
            return BoneList.Where(a => bone.childIDs.Contains(a.ControllerID)).ToList();
        }

        public List<string> GetBoneNames()
        {
            return BoneList.Select(a => a.boneName).ToList();
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

        protected Matrix4x4 GetTransformFromParts(Vector3 localTranslation, Matrix3x3 localRotation)
        {
            Matrix4x4 transform = new Matrix4x4
            {
                // Translation part
                M41 = localTranslation.X,
                M42 = localTranslation.Y,
                M43 = localTranslation.Z,
                // Rotation part
                M11 = localRotation.M11,
                M12 = localRotation.M12,
                M13 = localRotation.M13,
                M21 = localRotation.M21,
                M22 = localRotation.M22,
                M23 = localRotation.M23,
                M31 = localRotation.M31,
                M32 = localRotation.M32,
                M33 = localRotation.M33,
                // Set final row
                M14 = 0,
                M24 = 0,
                M34 = 0,
                M44 = 1
            };
            return transform;
        }

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}";
        }
    }
}
