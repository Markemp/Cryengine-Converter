using CgfConverter.Models.Structs;
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
    public int ObjectNodeIndex { get; set; } = -1;          // Points to index of NodeMeshCombo chunk (Ivo file only, -1 = not set)
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
        // Unlike 0x800 which stores both W2B and B2W matrices, 0x801 only stores B2W (boneToWorld).
        // We must compute W2B by inverting B2W.
        ControllerID = b.ReadUInt32();        // Bone controller.  Can be 0xFFFFFFFF
        LimbId = b.ReadUInt32();
        PhysicsGeometry = new PhysicsGeometry[2];
        PhysicsGeometry[0].ReadPhysicsGeometry(b);     // LOD 0 is the physics of alive body,
        PhysicsGeometry[1].ReadPhysicsGeometry(b);     // LOD 1 is the physics of a dead body
        BoneName = b.ReadFString(48);
        OffsetParent = b.ReadInt32();
        NumberOfChildren = b.ReadInt32();
        OffsetChild = b.ReadInt32();

        // The matrix stored in 0x801 is B2W (boneToWorld), not W2B like in 0x800
        WorldTransformMatrix = b.ReadMatrix3x4();

        // Compute W2B (worldToBone) by inverting B2W - this is the bind pose matrix
        var boneToWorld = WorldTransformMatrix.ConvertToTransformMatrix();
        if (Matrix4x4.Invert(boneToWorld, out var worldToBone))
        {
            BindPoseMatrix = worldToBone;
        }
        else
        {
            // Fallback to identity if inversion fails
            BindPoseMatrix = Matrix4x4.Identity;
        }

        // LocalTransformMatrix will be computed after parent relationships are established
        // For now, use the B2W matrix (actual local will be computed in ChunkCompiledBones_801)
        LocalTransformMatrix = WorldTransformMatrix;
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

        // Build boneToWorld matrix in CryEngine convention (column-vector rotation + translation in col 4).
        // IMPORTANT: Must use ConvertToRotationMatrix() which produces CryEngine convention rotation,
        // NOT Matrix4x4.CreateFromQuaternion() which produces .NET row-vector convention (transposed).
        // Using the wrong convention causes parent.BPM * inv(child.BPM) to produce incorrect local
        // transforms, making skeleton bone positions diverge from their bind transforms.
        var rot = worldQuat.ConvertToRotationMatrix();
        var boneToWorld = new Matrix4x4(
            rot.M11, rot.M12, rot.M13, worldTranslation.X,
            rot.M21, rot.M22, rot.M23, worldTranslation.Y,
            rot.M31, rot.M32, rot.M33, worldTranslation.Z,
            0, 0, 0, 1);
        Matrix4x4.Invert(boneToWorld, out Matrix4x4 bpm);
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
