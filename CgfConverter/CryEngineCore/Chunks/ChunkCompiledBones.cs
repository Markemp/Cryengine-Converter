using CgfConverter.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkCompiledBones : Chunk     //  0xACDC0000:  Bones info
    {
        public string RootBoneName;         // Controller ID?  Name?  Not sure yet.
        public CompiledBone RootBone;       // First bone in the data structure.  Usually Bip01
        public int NumBones;                // Number of bones in the chunk

        // Bones are a bit different than Node Chunks, since there is only one CompiledBones Chunk, and it contains all the bones in the model.
        public List<CompiledBone> BoneList = new List<CompiledBone>();

        public List<CompiledBone> GetAllChildBones(CompiledBone bone)
        {
            return BoneList.Where(a => bone.childIDs.Contains(a.ControllerID)).ToList();
        }

        public List<string> GetBoneNames()
        {
            return BoneList.Select(a => a.boneName).ToList();
        }

        protected void AddChildIDToParent(CompiledBone bone)
        {
            if (bone.parentID != 0)
            {
                CompiledBone parent = BoneList.Where(a => a.ControllerID == bone.parentID).FirstOrDefault();  // Should only be one parent.
                parent.childIDs.Add(bone.ControllerID);
            }
        }

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}";
        }
    }
}
