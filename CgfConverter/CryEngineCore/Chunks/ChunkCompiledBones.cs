using System.Collections.Generic;
using System.Linq;

namespace CgfConverter.CryEngineCore;

public abstract class ChunkCompiledBones : Chunk     //  Bones info
{
    public string? RootBoneName { get; set; }         // Controller ID?  Name?  Not sure yet.
    // public CompiledBone RootBone;       // First bone in the data structure.
    public int NumBones;                // Number of bones in the chunk
    public int? Flags1;
    public int? Flags2;

    public List<CompiledBone> BoneList = [];

    public CompiledBone RootBone
    {
        get { return BoneList.First(); }
    }

    public override string ToString() => $@"Chunk Type: {ChunkType}, ID: {ID:X}";
}
