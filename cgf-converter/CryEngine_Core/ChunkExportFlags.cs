using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkExportFlags : Chunk  // cccc0015:  Export Flags
    {
        public UInt32 ChunkOffset;  // for some reason the offset of Export Flag chunk is stored here.
        public UInt32 Flags;    // ExportFlags type technically, but it's just 1 value
        public UInt32[] RCVersion;  // 4 uints
        public String RCVersionString;  // Technically String16

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Verbose, "*** START EXPORT FLAGS ***");
            Utils.Log(LogLevelEnum.Verbose, "    Export Chunk ID: {0:X}", ID);
            Utils.Log(LogLevelEnum.Verbose, "    ChunkType: {0}", ChunkType);
            Utils.Log(LogLevelEnum.Verbose, "    Version: {0}", Version);
            Utils.Log(LogLevelEnum.Verbose, "    Flags: {0}", Flags);
            StringBuilder sb = new StringBuilder("    RC Version: ");
            for (Int32 i = 0; i < 4; i++)
            {
                sb.Append(RCVersion[i]);
            }
            Utils.Log(LogLevelEnum.Verbose, sb.ToString());
            Utils.Log(LogLevelEnum.Verbose);
            Utils.Log(LogLevelEnum.Verbose, "    RCVersion String: {0}", this.RCVersionString);
            Utils.Log(LogLevelEnum.Verbose, "*** END EXPORT FLAGS ***");
        }
    }
}
