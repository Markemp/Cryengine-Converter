using CgfConverter.Structs;
using Extensions;
using System.IO;
using System.Numerics;
using static CgfConverter.Utilities.HelperMethods;

namespace CgfConverter.CryEngineCore.Chunks;
internal class ChunkCompiledBones_901 : ChunkCompiledBones
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        NumBones = b.ReadInt32();
        var _ = b.ReadInt32(); // String table size.  Won't use.
        Flags1 = b.ReadInt32();
        Flags2 = b.ReadInt32();

        for (int i = 0; i < NumBones; i++)
        {
            CompiledBone tempBone = new();
            tempBone.ReadCompiledBone_901(b);
            BoneList.Add(tempBone);
        }

        var boneNames = GetNullSeparatedStrings(NumBones, b);

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
            if (BoneList[i].OffsetParent != -1)
            {
                BoneList[i].ParentBone = BoneList[BoneList[i].OffsetParent];
                BoneList[i].ParentControllerIndex = BoneList[i].OffsetParent;
                BoneList[i].ParentBone.ChildIDs.Add(i);
                BoneList[i].ParentBone.NumberOfChildren++;
            }
            BoneList[i].LocalTransformMatrix = Matrix3x4.CreateFromParts(relativeQuat, relativeTranslation);
            BoneList[i].WorldTransformMatrix = Matrix3x4.CreateFromParts(worldQuat, worldTranslation);
        }
    }
}
