using CgfConverter.Structs;
using Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace CgfConverter;

public sealed class CompiledBone
{
    public uint ControllerID { get; set; }
    public PhysicsGeometry[]? PhysicsGeometry { get; set; } // 2 of these.  One for live objects, other for dead (ragdoll?)
    public double Mass { get; set; }           // 0xD8 ?
    public Matrix3x4 LocalTransformMatrix { get; set; }     // WORLDTOBONE is also the Bind Pose Matrix (BPM) in Collada
    public Matrix3x4 WorldTransformMatrix { get; set; }
    public string? BoneName { get; set; }                   // String256 in old terms; convert to a real null terminated string.
    public uint LimbId { get; set; }                        // ID of this limb... usually just 0xFFFFFFFF
    public int OffsetParent { get; set; }                   // offset to the parent in number of CompiledBone structs (584 bytes)
    public int OffsetChild { get; set; }                    // Offset to the first child to this bone in number of CompiledBone structs. Don't use this. Not in Ivo files.
    public int NumberOfChildren { get; set; }                    // Number of children to this bone
    public int ObjectNodeIndex { get; set; }                // Points to index of NodeMeshCombo chunk (Ivo file)
    public int ParentIndex { get; set; }       // For 0x900, we don't have parent offset.
    // Calculated values
    public Matrix4x4 BindPoseMatrix { get; set; }     // This is the WorldToBone matrix for library_controllers
    public int ParentControllerIndex { get; set; }    // Calculated controllerID of the parent bone put into the Bone Dictionary (the key)
    public List<int> ChildIDs { get; set; } = [];     // Calculated controllerIDs of the children to this bone.

    public CompiledBone? ParentBone;

    public Matrix4x4 LocalTransform {
        get {
            if (ParentBone is null) // No parent
                return Matrix4x4Extensions.CreateFromMatrix3x4(WorldTransformMatrix);
            else
                return Matrix4x4Extensions.CreateFromMatrix3x4(ParentBone.WorldTransformMatrix) * Matrix4x4Extensions.CreateFromMatrix3x4(WorldTransformMatrix);
        }
    }

    public void ReadCompiledBone_800(BinaryReader b)
    {
        // Reads just a single 584 byte entry of a bone.
        ControllerID = b.ReadUInt32();                  // Bone controller.  Can be 0xFFFFFFFF
        PhysicsGeometry = new PhysicsGeometry[2];
        PhysicsGeometry[0].ReadPhysicsGeometry(b);     // LOD 0 is the physics of alive body, 
        PhysicsGeometry[1].ReadPhysicsGeometry(b);     // LOD 1 is the physics of a dead body
        Mass = b.ReadSingle();
        LocalTransformMatrix = b.ReadMatrix3x4();
        BindPoseMatrix = LocalTransformMatrix.ConvertToTransformMatrix();
        WorldTransformMatrix = b.ReadMatrix3x4();
        BoneName = b.ReadFString(256);
        LimbId = b.ReadUInt32();
        OffsetParent = b.ReadInt32();
        NumberOfChildren = b.ReadInt32();
        OffsetChild = b.ReadInt32();
    }

    public void ReadCompiledBone_801(BinaryReader b)
    {
        // Reads just a single 324 byte entry of a bone.
        ControllerID = b.ReadUInt32();        // Bone controller.  Can be 0xFFFFFFFF
        LimbId = b.ReadUInt32();
        PhysicsGeometry = new PhysicsGeometry[2];
        PhysicsGeometry[0].ReadPhysicsGeometry(b);     // LOD 0 is the physics of alive body, 
        PhysicsGeometry[1].ReadPhysicsGeometry(b);     // LOD 1 is the physics of a dead body
        BoneName = b.ReadFString(48);
        OffsetParent = b.ReadInt32();
        NumberOfChildren = b.ReadInt32();
        OffsetChild = b.ReadInt32();
        LocalTransformMatrix = b.ReadMatrix3x4();
        BindPoseMatrix = LocalTransformMatrix.ConvertToTransformMatrix();
        WorldTransformMatrix = new(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0);
    }

    public void ReadCompiledBone_900(BinaryReader b)
    {
        ControllerID = b.ReadUInt32();                 // unique id of bone (generated from bone name)
        LimbId = b.ReadUInt32();
        ParentIndex = b.ReadInt32();
        // have to compute offsetParent
        Quaternion relativeQuat = b.ReadQuaternion();
        Vector3 relativeTranslation = b.ReadVector3();
        Quaternion worldQuat = b.ReadQuaternion();
        Vector3 worldTranslation = b.ReadVector3();
        LocalTransformMatrix = Matrix3x4.CreateFromParts(relativeQuat, relativeTranslation);
        WorldTransformMatrix = Matrix3x4.CreateFromParts(worldQuat, worldTranslation);

        var m = Matrix4x4.CreateFromQuaternion(worldQuat);
        m.M14 = worldTranslation.X;
        m.M24 = worldTranslation.Y;
        m.M34 = worldTranslation.Z;
        Matrix4x4.Invert(m, out Matrix4x4 bpm);
        BindPoseMatrix = bpm;
    }

    public void ReadCompiledBone_901(BinaryReader b)
    {
        ControllerID = b.ReadUInt32();
        LimbId = b.ReadUInt16();
        NumberOfChildren = b.ReadUInt16();
        ParentControllerIndex = b.ReadInt16();
        var unknown = b.ReadInt16();  // only seen 0xffff
        var unknown2 = b.ReadInt16(); // only seen 0xffff
        ObjectNodeIndex = b.ReadInt16();
    }
}
