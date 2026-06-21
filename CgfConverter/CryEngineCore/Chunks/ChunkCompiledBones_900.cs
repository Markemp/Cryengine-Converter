using System.Collections.Generic;
using System.IO;
using static CgfConverter.Utilities.HelperMethods;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// WARNING: This chunk version has NOT been validated for animation export.
/// Only ChunkCompiledBones_800 has been vetted for animations (with MWO DBA files).
/// The BindPoseMatrix calculation here may not be correct - do not trust animation
/// data from this chunk until it has been properly validated.
/// </summary>
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
            tempBone.OffsetParent = i == 0 ? -1 : tempBone.ParentIndex - i;
            BoneList.Add(tempBone);
        }

        List<string> boneNames = GetNullSeparatedStrings(NumBones, b);

        // Post bone read setup.  Parents, children, etc.
        // Add the ChildID to the parent bone.  This will help with navigation.
        for (int i = 0; i < NumBones; i++)
        {
            BoneList[i].BoneName = boneNames[i];
            if (BoneList[i].ParentIndex != -1)  // root bone has parent index = -1
            {
                BoneList[i].ParentBone = BoneList[BoneList[i].ParentIndex];
                BoneList[i].ParentControllerIndex = BoneList[i].ParentIndex;
                BoneList[i].ParentBone.ChildIDs.Add(i);
                BoneList[i].ParentBone.NumberOfChildren++;
            }
        }
    }
}
