using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using CgfConverter.Structs;
using Extensions;

namespace CgfConverter
{
    public class CompiledBone
    {
        public uint ControllerID { get; set; }
        public PhysicsGeometry[] physicsGeometry;  // 2 of these.  One for live objects, other for dead (ragdoll?)
        public double mass;                        // 0xD8 ?
        public Matrix3x4 WorldToBone;              // 4x3 matrix   WORLDTOBONE is also the Bind Pose Matrix (BPM)
        public Matrix3x4 BoneToWorld;              // 4x3 matrix of world translations/rotations of the bones.
        public string boneName;                    // String256 in old terms; convert to a real null terminated string.
        public int limbID;                         // ID of this limb... usually just 0xFFFFFFFF
        public int offsetParent;                   // offset to the parent in number of CompiledBone structs (584 bytes)
        public int offsetChild;                    // Offset to the first child to this bone in number of CompiledBone structs
        public uint numChildren;                   // Number of children to this bone

        public Matrix4x4 BindPoseMatrix;           // This is the WorldToBone matrix for library_controllers
        
        public long offset;                        // Calculated position in the file where this bone started.
        
        public uint parentID;                           // Calculated controllerID of the parent bone put into the Bone Dictionary (the key)
        public List<uint> childIDs = new List<uint>();  // Calculated controllerIDs of the children to this bone.

        public CompiledBone ParentBone;

        public Matrix4x4 LocalTransform
        {
            get 
            {
                //if (offsetParent == 0) // No parent
                if (ParentBone == null) // No parent
                {
                    return Matrix4x4Extensions.CreateFromMatrix3x4(BoneToWorld);
                }
                else 
                {
                    return Matrix4x4Extensions.CreateLocalTransformFromB2W(
                        Matrix4x4Extensions.CreateFromMatrix3x4(ParentBone.BoneToWorld),
                        Matrix4x4Extensions.CreateFromMatrix3x4(BoneToWorld));
                }
            }
        }

        public void ReadCompiledBone_800(BinaryReader b)
        {
            // Reads just a single 584 byte entry of a bone.
            ControllerID = b.ReadUInt32();                 // unique id of bone (generated from bone name)
            physicsGeometry = new PhysicsGeometry[2];
            physicsGeometry[0].ReadPhysicsGeometry(b);     // LOD 0 is the physics of alive body, 
            physicsGeometry[1].ReadPhysicsGeometry(b);     // LOD 1 is the physics of a dead body
            mass = b.ReadSingle();
            WorldToBone = b.ReadMatrix3x4();
            BindPoseMatrix = WorldToBone.ConvertToTransformMatrix();
            BoneToWorld = b.ReadMatrix3x4();
            boneName = b.ReadFString(256);
            limbID = b.ReadInt32();
            offsetParent = b.ReadInt32();
            numChildren = b.ReadUInt32();
            offsetChild = b.ReadInt32();
        }

        public void ReadCompiledBone_801(BinaryReader b)
        {
            // Reads just a single 3xx byte entry of a bone.
            ControllerID = b.ReadUInt32();                 // unique id of bone (generated from bone name)
            limbID = b.ReadInt32();
            b.BaseStream.Seek(208, SeekOrigin.Current);
            boneName = b.ReadFString(48);
            offsetParent = b.ReadInt32();
            numChildren = b.ReadUInt32();
            offsetChild = b.ReadInt32();
            // TODO:  This may be quaternion and translation vectors. 
            WorldToBone = b.ReadMatrix3x4();
            BindPoseMatrix = WorldToBone.ConvertToTransformMatrix();
            BoneToWorld = new(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0);

            childIDs = new List<uint>();                    // Calculated
        }

        public void ReadCompiledBone_900(BinaryReader b)
        {
            ControllerID = b.ReadUInt32();                 // unique id of bone (generated from bone name)
            limbID = b.ReadInt32();
            offsetParent = b.ReadInt32();
            Quaternion relativeQuat = new()
            {
                X = b.ReadSingle(),
                Y = b.ReadSingle(),
                Z = b.ReadSingle(),
                W = b.ReadSingle()
            };
            Vector3 relativeTranslation = new()
            {
                Z = b.ReadSingle(),
                Y = b.ReadSingle(),
                X = -b.ReadSingle()
            };
            Quaternion worldQuat = new()
            {
                X = b.ReadSingle(),
                Y = b.ReadSingle(),
                Z = b.ReadSingle(),
                W = b.ReadSingle()
            };
            Vector3 worldTranslation = new()
            {
                Z = b.ReadSingle(), 
                Y = b.ReadSingle(),
                X = -b.ReadSingle()
            };
            BindPoseMatrix = Matrix4x4.CreateFromQuaternion(relativeQuat);
            BindPoseMatrix.M14 = relativeTranslation.X;
            BindPoseMatrix.M24 = relativeTranslation.Y;
            BindPoseMatrix.M34 = relativeTranslation.Z;
            BindPoseMatrix.M41 = 0;
            BindPoseMatrix.M42 = 0;
            BindPoseMatrix.M43 = 0;
            BindPoseMatrix.M44 = 1.0f;

            BoneToWorld = Matrix3x4.CreateFromParts(worldQuat, worldTranslation);
        }
    }
}
