using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    /// <summary>
    /// Helper chunk.  This is the top level, then nodes, then mesh, then mesh subsets. CCCC0001  
    /// </summary>
    public abstract class ChunkHelper : Chunk
    {
        public string Name;
        public HelperTypeEnum HelperType;
        public Vector3 Pos;
        public Matrix44 Transform;
        
        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";
        }

        public void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Verbose, "*** START Helper Chunk ***");
            Utils.Log(LogLevelEnum.Verbose, "    ChunkType:   {0}", ChunkType);
            Utils.Log(LogLevelEnum.Verbose, "    Version:     {0:X}", Version);
            Utils.Log(LogLevelEnum.Verbose, "    ID:          {0:X}", ID);
            Utils.Log(LogLevelEnum.Verbose, "    HelperType:  {0}", HelperType);
            Utils.Log(LogLevelEnum.Verbose, "    Position:    {0}, {1}, {2}", Pos.x, Pos.y, Pos.z);
            Utils.Log(LogLevelEnum.Verbose, "*** END Helper Chunk ***");
        }
    }
}
