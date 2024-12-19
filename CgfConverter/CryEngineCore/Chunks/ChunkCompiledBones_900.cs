using CgfConverter.Models;
using System.Collections.Generic;
using System.IO;
using static CgfConverter.Utilities.HelperMethods;

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
            if (BoneList[i].offsetParent != -1)
            {
                BoneList[i].ParentBone = BoneList[BoneList[i].offsetParent];
                BoneList[i].ParentControllerIndex = BoneList[i].offsetParent;
                BoneList[i].ParentBone.ChildIDs.Add(i);
                BoneList[i].ParentBone.numChildren++;
            }
        }

        SkinningInfo skin = GetSkinningInfo();
        skin.CompiledBones = BoneList;
    }
}
