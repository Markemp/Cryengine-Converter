using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkCompiledBones_900 : ChunkCompiledBones
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        NumBones = b.ReadInt32();

        for (int i = 0; i < NumBones; i++)
        {
            CompiledBone tempBone = new();
            tempBone.ReadCompiledBone_900(b);

            if (RootBone is null)  // First bone read is root bone
                RootBone = tempBone;

            BoneList.Add(tempBone);
        }

        

        List<string> boneNames = GetNullSeparatedStrings(NumBones, b);

        // Post bone read setup.  Parents, children, etc.
        // Add the ChildID to the parent bone.  This will help with navigation.
        for (int i = 0; i < NumBones; i++)
        {
            BoneList[i].boneName = boneNames[i];
            SetParentBone(BoneList[i]);
            AddChildIDToParent(BoneList[i]);
        }

        CalculateBindPoseMatrix();

        SkinningInfo skin = GetSkinningInfo();
        skin.CompiledBones = new List<CompiledBone>();
        skin.HasSkinningInfo = true;
        skin.CompiledBones = BoneList;
    }

    void SetParentBone(CompiledBone bone)
    {
        // offsetParent is really parent index.
        if (bone.offsetParent != -1)
        {
            bone.parentID = BoneList[bone.offsetParent].ControllerID;
            bone.ParentBone = BoneList[bone.offsetParent];
        }
    }

    private void CalculateBindPoseMatrix()
    {
        // For each bone, the BPM is the parent's world pose matrix * this world pose matrix (?)
        for (int i = 0; i < NumBones; i++)
        {
            if (i == 0) // root bone.  Identity * world pose matrix.
                BoneList[i].BindPoseMatrix = Matrix4x4.Identity * BoneList[i].WorldToBone.ConvertToTransformMatrix();
            else
            {
                BoneList[i].BindPoseMatrix = BoneList[i].ParentBone.BindPoseMatrix * BoneList[i].WorldToBone.ConvertToTransformMatrix();
                BoneList[i].BindPoseMatrix.M14 = BoneList[i].ParentBone.BindPoseMatrix.M14 - BoneList[i].WorldToBone.M14;
                BoneList[i].BindPoseMatrix.M24 = BoneList[i].ParentBone.BindPoseMatrix.M24 - BoneList[i].WorldToBone.M24;
                BoneList[i].BindPoseMatrix.M34 = BoneList[i].ParentBone.BindPoseMatrix.M34 - BoneList[i].WorldToBone.M34;
            }
        }
    }

    internal static List<string> GetNullSeparatedStrings(int numberOfNames, BinaryReader b)
    {
        List<string> names = new();
        StringBuilder builder = new();

        for (int i = 0; i < numberOfNames; i++)
        {    
            char c = b.ReadChar();
            while (c != 0)
            {
                builder.Append(c);
                c = b.ReadChar();
            }
            names.Add(builder.ToString());
            builder.Clear();
        }

        return names;
    }
}
