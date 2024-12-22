using System.Collections.Generic;

namespace CgfConverter.CryEngineCore;

public abstract class ChunkCompiledBones : Chunk     //  Bones info
{
    public string RootBoneName;         // Controller ID?  Name?  Not sure yet.
    public CompiledBone RootBone;       // First bone in the data structure.
    public int NumBones;                // Number of bones in the chunk
    public int? Flags1;
    public int? Flags2;

    public List<CompiledBone> BoneList = new();

    public List<CompiledBone> GetChildBones(CompiledBone bone)
    {
        List<CompiledBone> childBones = new();
        foreach (var bone1 in BoneList)
        {
            if (bone1.ParentBone == bone)
                childBones.Add(bone1);
        }
        return childBones;
    }

    public override string ToString() => $@"Chunk Type: {ChunkType}, ID: {ID:X}";
}
