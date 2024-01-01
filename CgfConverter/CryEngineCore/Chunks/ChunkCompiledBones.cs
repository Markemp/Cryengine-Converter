using System.Collections.Generic;
using System.Linq;

namespace CgfConverter.CryEngineCore;

public abstract class ChunkCompiledBones : Chunk     //  0xACDC0000:  Bones info
{
    public string RootBoneName;         // Controller ID?  Name?  Not sure yet.
    public CompiledBone RootBone;       // First bone in the data structure.
    public int NumBones;                // Number of bones in the chunk

    // Bones are a bit different than Node Chunks, since there is only one CompiledBones Chunk, and it contains all the bones in the model.
    public List<CompiledBone> BoneList = new();

    // This is wrong.  All child bones are based off the index of the bone and the child bone offsets
    //public List<CompiledBone> GetAllChildBones(CompiledBone bone) => BoneList.Where(a => bone.childIDs.Contains(a.ControllerID)).ToList();
    public List<CompiledBone> GetAllChildBones(CompiledBone bone)
    {
        var boneIndex = BoneList.IndexOf(bone);
        var startIndex = boneIndex + bone.offsetChild;
        var endIndex = startIndex + bone.numChildren;

        return BoneList.Skip(startIndex).Take(bone.numChildren).ToList();
    }

    public List<string> GetBoneNames() => BoneList.Select(a => a.boneName).ToList();

    public override string ToString() => $@"Chunk Type: {ChunkType}, ID: {ID:X}";
}
