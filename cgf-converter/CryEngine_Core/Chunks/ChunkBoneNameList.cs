using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    /// <summary>
    /// Legacy class.  Not used
    /// </summary>
    public abstract class ChunkBoneNameList : Chunk
    {
        public int NumEntities;
        public List<string> BoneNames;

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Debug, "*** START MorphTargets Chunk ***");
            Utils.Log(LogLevelEnum.Debug, "    ChunkType:           {0}", ChunkType);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", ID);
            Utils.Log(LogLevelEnum.Debug, "    Number of Targets:   {0:X}", NumEntities);
            foreach (string name in BoneNames)
            {
                Utils.Log(LogLevelEnum.Debug, "    Bone Name:       {0}", name);
            }
        }
    }
}
