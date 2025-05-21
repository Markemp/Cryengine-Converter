using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkCompiledBones_800 : ChunkCompiledBones
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        SkipBytes(b, 32);  // Padding between the chunk header and the first bone.

        //  Read the first bone with ReadCompiledBone, then recursively grab all the children for each bone you find.
        //  Each bone structure is 584 bytes, so will need to seek childOffset * 584 each time, and go back.
        
        NumBones = (int)((Size - 32) / 584);

        for (int i = 0; i < NumBones; i++)
        {
            CompiledBone tempBone = new();
            tempBone.ReadCompiledBone_800(b);

            if (tempBone.OffsetParent != 0)
                tempBone.ParentBone = BoneList[i + tempBone.OffsetParent];
            
            if (tempBone.ParentBone is not null)
                tempBone.ParentControllerIndex = BoneList.IndexOf(tempBone) + tempBone.OffsetParent;
            else
                tempBone.ParentControllerIndex = 0;

            BoneList.Add(tempBone);
        }
    }
}
