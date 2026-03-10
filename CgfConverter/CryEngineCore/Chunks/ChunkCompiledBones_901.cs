using CgfConverter.Models.Structs;
using Extensions;
using System.IO;
using System.Numerics;
using static CgfConverter.Utilities.HelperMethods;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// WARNING: This chunk version has NOT been validated for animation export.
/// Only ChunkCompiledBones_800 has been vetted for animations (with MWO DBA files).
/// The BindPoseMatrix calculation here may not be correct - do not trust animation
/// data from this chunk until it has been properly validated.
/// </summary>
internal class ChunkCompiledBones_901 : ChunkCompiledBones
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        NumBones = b.ReadInt32();
        var stringTableSize = b.ReadInt32();
        Flags1 = b.ReadInt32();
        Flags2 = b.ReadInt32();

        // SC 4.5+ Ivo format has 32 bytes of padding after the header fields
        if (ChunkType == ChunkType.CompiledBones_Ivo || ChunkType == ChunkType.CompiledBones_Ivo2)
            SkipBytes(b, 32);

        for (int i = 0; i < NumBones; i++)
        {
            CompiledBone tempBone = new();
            tempBone.ReadCompiledBone_901(b);
            BoneList.Add(tempBone);
        }

        var boneNames = GetNullSeparatedStrings(NumBones, stringTableSize, b);

        // Post bone read setup.  Parents, children, etc.
        // Add the ChildID to the parent bone.  This will help with navigation.
        for (int i = 0; i < NumBones; i++)
        {
            var relativeQuat = b.ReadQuaternion();
            var relativeTranslation = b.ReadVector3();
            var worldQuat = b.ReadQuaternion();
            var worldTranslation = b.ReadVector3();

            var m = Matrix4x4.CreateFromQuaternion(worldQuat);
            m.M14 = worldTranslation.X;
            m.M24 = worldTranslation.Y;
            m.M34 = worldTranslation.Z;
            Matrix4x4.Invert(m, out Matrix4x4 bpm);
            BoneList[i].BindPoseMatrix = bpm;

            BoneList[i].BoneName = boneNames[i];
            // ParentControllerIndex is read in ReadCompiledBone_901 as the parent bone index
            // A value of -1 (or 0xFFFF as signed short) means no parent (root bone)
            if (BoneList[i].ParentControllerIndex >= 0 && BoneList[i].ParentControllerIndex < BoneList.Count)
            {
                BoneList[i].ParentBone = BoneList[BoneList[i].ParentControllerIndex];
                BoneList[i].ParentBone.ChildIDs.Add(i);
            }
            BoneList[i].LocalTransformMatrix = Matrix3x4.CreateFromParts(relativeQuat, relativeTranslation);
            BoneList[i].WorldTransformMatrix = Matrix3x4.CreateFromParts(worldQuat, worldTranslation);
        }
    }
}
