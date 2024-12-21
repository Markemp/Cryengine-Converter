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
    public PhysicsGeometry[]? physicsGeometry; // 2 of these.  One for live objects, other for dead (ragdoll?)
    public double mass;                        // 0xD8 ?
    public Matrix3x4 LocalTransformMatrix;     // WORLDTOBONE is also the Bind Pose Matrix (BPM) in Collada
    public Matrix3x4 WorldTransformMatrix;
    public string? boneName;                   // String256 in old terms; convert to a real null terminated string.
    public uint limbID;                        // ID of this limb... usually just 0xFFFFFFFF
    public int offsetParent;                   // offset to the parent in number of CompiledBone structs (584 bytes)
    public int offsetChild;                    // Offset to the first child to this bone in number of CompiledBone structs. Don't use this. Not in Ivo files.
    public int numChildren;                    // Number of children to this bone
    public int objectNodeIndex;                // Points to index of NodeMeshCombo chunk (Ivo file)

    // Calculated values
    public Matrix4x4 BindPoseMatrix { get; set; }     // This is the WorldToBone matrix for library_controllers
    public int ParentControllerIndex { get; set; }    // Calculated controllerID of the parent bone put into the Bone Dictionary (the key)
    public List<int> ChildIDs { get; set; } = new();  // Calculated controllerIDs of the children to this bone.

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
        physicsGeometry = new PhysicsGeometry[2];
        physicsGeometry[0].ReadPhysicsGeometry(b);     // LOD 0 is the physics of alive body, 
        physicsGeometry[1].ReadPhysicsGeometry(b);     // LOD 1 is the physics of a dead body
        mass = b.ReadSingle();
        LocalTransformMatrix = b.ReadMatrix3x4();
        BindPoseMatrix = LocalTransformMatrix.ConvertToTransformMatrix();
        WorldTransformMatrix = b.ReadMatrix3x4();
        boneName = b.ReadFString(256);
        limbID = b.ReadUInt32();
        offsetParent = b.ReadInt32();
        numChildren = b.ReadInt32();
        offsetChild = b.ReadInt32();
    }

    public void ReadCompiledBone_801(BinaryReader b)
    {
        // Reads just a single 324 byte entry of a bone.
        ControllerID = b.ReadUInt32();        // Bone controller.  Can be 0xFFFFFFFF
        limbID = b.ReadUInt32();
        physicsGeometry = new PhysicsGeometry[2];
        physicsGeometry[0].ReadPhysicsGeometry(b);     // LOD 0 is the physics of alive body, 
        physicsGeometry[1].ReadPhysicsGeometry(b);     // LOD 1 is the physics of a dead body
        boneName = b.ReadFString(48);
        offsetParent = b.ReadInt32();
        numChildren = b.ReadInt32();
        offsetChild = b.ReadInt32();
        LocalTransformMatrix = b.ReadMatrix3x4();
        BindPoseMatrix = LocalTransformMatrix.ConvertToTransformMatrix();
        WorldTransformMatrix = new(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0);
    }

    public void ReadCompiledBone_900(BinaryReader b)
    {
        ControllerID = b.ReadUInt32();                 // unique id of bone (generated from bone name)
        limbID = b.ReadUInt32();
        offsetParent = b.ReadInt32();
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
        limbID = b.ReadUInt16();
        numChildren = b.ReadUInt16();
        ParentControllerIndex = b.ReadInt16();
        var unknown = b.ReadInt16();  // only seen 0xffff
        var unknown2 = b.ReadInt16(); // only seen 0xffff
        objectNodeIndex = b.ReadInt16();
    }
}
