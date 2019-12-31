using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using CgfConverter.CryEngineCore;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkCompiledPhysicalBones : Chunk     //  0xACDC0000:  Bones info
    {
        public char[] Reserved;             // 32 byte array
        public CompiledPhysicalBone RootPhysicalBone;       // First bone in the data structure.  Usually Bip01
        public int NumBones;                // Number of bones in the chunk

        public Dictionary<uint, CompiledPhysicalBone> PhysicalBoneDictionary = new Dictionary<uint, CompiledPhysicalBone>();  // Dictionary of all the CompiledBone objects based on bone name.
        public List<CompiledPhysicalBone> PhysicalBoneList = new List<CompiledPhysicalBone>();

        protected void AddChildIDToParent(CompiledPhysicalBone bone)
        {
            // Root bone parent ID will be zero.
            if (bone.parentID != 0)
            {
                CompiledPhysicalBone parent = PhysicalBoneList.Where(a => a.ControllerID == bone.parentID).FirstOrDefault();  // Should only be one parent.
                parent.childIDs.Add(bone.ControllerID);
            }
        }

        public List<CompiledPhysicalBone> GetAllChildBones(CompiledPhysicalBone bone)
        {
            if (bone.NumChildren > 0)
            {
                return PhysicalBoneList.Where(a => bone.childIDs.Contains(a.ControllerID)).ToList();
            }
            else
                return null;
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
