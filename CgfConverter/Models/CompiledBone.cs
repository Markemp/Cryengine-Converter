using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using CgfConverter.Structs;

namespace CgfConverter
{
    public class CompiledBone
    {
        public uint ControllerID { get; set; }
        public PhysicsGeometry[] physicsGeometry;  // 2 of these.  One for live objects, other for dead (ragdoll?)
        public double mass;                        // 0xD8 ?
        public WORLDTOBONE worldToBone;            // 4x3 matrix
        public BONETOWORLD boneToWorld;            // 4x3 matrix of world translations/rotations of the bones.
        public string boneName;                    // String256 in old terms; convert to a real null terminated string.
        public int limbID;                         // ID of this limb... usually just 0xFFFFFFFF
        public int offsetParent;                   // offset to the parent in number of CompiledBone structs (584 bytes)
        public int offsetChild;                    // Offset to the first child to this bone in number of CompiledBone structs
        public uint numChildren;                   // Number of children to this bone

        public Matrix4x4 BindPoseMatrix;           // Use the inverse of this to place in Collada file
        public long offset;                        // Calculated position in the file where this bone started.
        
        public uint parentID;                           // Calculated controllerID of the parent bone put into the Bone Dictionary (the key)
        public List<uint> childIDs = new List<uint>();  // Calculated controllerIDs of the children to this bone.

        public Matrix4x4 LocalTransform = new Matrix4x4();
        public Vector3 LocalTranslation { get; set; } = new Vector3();            // To hold the local rotation vector
        public Matrix3x3 LocalRotation = new Matrix3x3();             // to hold the local rotation matrix

        public CompiledBone ParentBone { get; set; }

        public void ReadCompiledBone_800(BinaryReader b)
        {
            // Reads just a single 584 byte entry of a bone.
            ControllerID = b.ReadUInt32();                 // unique id of bone (generated from bone name)
            physicsGeometry = new PhysicsGeometry[2];
            physicsGeometry[0].ReadPhysicsGeometry(b);     // LOD 0 is the physics of alive body, 
            physicsGeometry[1].ReadPhysicsGeometry(b);     // LOD 1 is the physics of a dead body
            mass = b.ReadSingle();
            worldToBone = new WORLDTOBONE();
            worldToBone.GetWorldToBone(b);
            boneToWorld = new BONETOWORLD();
            boneToWorld.ReadBoneToWorld(b);
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
            boneToWorld = new BONETOWORLD();
            boneToWorld.ReadBoneToWorld(b);
            worldToBone = new WORLDTOBONE();
            worldToBone.worldToBone = new float[3, 4];
            worldToBone.worldToBone[0, 0] = boneToWorld.boneToWorld[0, 0];
            worldToBone.worldToBone[0, 1] = boneToWorld.boneToWorld[0, 1];
            worldToBone.worldToBone[0, 2] = boneToWorld.boneToWorld[0, 2];
            worldToBone.worldToBone[0, 3] = boneToWorld.boneToWorld[0, 3];
            worldToBone.worldToBone[1, 0] = boneToWorld.boneToWorld[1, 0];
            worldToBone.worldToBone[1, 1] = boneToWorld.boneToWorld[1, 1];
            worldToBone.worldToBone[1, 2] = boneToWorld.boneToWorld[1, 2];
            worldToBone.worldToBone[1, 3] = boneToWorld.boneToWorld[1, 3];
            worldToBone.worldToBone[2, 0] = boneToWorld.boneToWorld[2, 0];
            worldToBone.worldToBone[2, 1] = boneToWorld.boneToWorld[2, 1];
            worldToBone.worldToBone[2, 2] = boneToWorld.boneToWorld[2, 2];
            worldToBone.worldToBone[2, 3] = boneToWorld.boneToWorld[2, 3];

            childIDs = new List<uint>();                    // Calculated
        }

        public void ReadCompiledBone_900(BinaryReader b)
        {
            ControllerID = b.ReadUInt32();                 // unique id of bone (generated from bone name)
            limbID = b.ReadInt32();
            offsetParent = b.ReadInt32();
            Quaternion relativeQuat = new Quaternion
            {
                X = b.ReadSingle(),
                Y = b.ReadSingle(),
                Z = b.ReadSingle(),
                W = b.ReadSingle()
            };
            Vector3 relativeTransform = new Vector3
            {
                X = b.ReadSingle(),
                Y = b.ReadSingle(),
                Z = b.ReadSingle()
            };
            Quaternion worldQuat = new Quaternion
            {
                X = b.ReadSingle(),
                Y = b.ReadSingle(),
                Z = b.ReadSingle(),
                W = b.ReadSingle()
            };
            Vector3 worldTransform = new Vector3
            {
                X = b.ReadSingle(),
                Y = b.ReadSingle(),
                Z = b.ReadSingle()
            };
            BindPoseMatrix = Matrix4x4.Transform(Matrix4x4.Identity, worldQuat);
            //worldToBone = new WORLDTOBONE(worldQuat.ConvertToRotationalMatrix(), worldTransform);
            //boneToWorld = new BONETOWORLD(relativeQuat.ConvertToRotationalMatrix(), relativeTransform);
        }
    }
}
