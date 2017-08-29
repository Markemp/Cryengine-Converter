using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkCompiledBones : Chunk     //  0xACDC0000:  Bones info
    {
        public String RootBoneID;          // Controller ID?  Name?  Not sure yet.
        public CompiledBone RootBone;       // First bone in the data structure.  Usually Bip01
        public UInt32 NumBones;               // Number of bones in the chunk
        // Bone info
        // Bones are a bit different than Node Chunks, since there is only one CompiledBones Chunk, and it contains all the bones in the model.
        public Dictionary<String, CompiledBone> BoneDictionary = new Dictionary<String, CompiledBone>();  // Dictionary of all the CompiledBone objects based on bone name.

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Debug, "*** START CompiledBone Chunk ***");
            Utils.Log(LogLevelEnum.Debug, "    ChunkType:           {0}", ChunkType);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", ID);
        }
    }

}
