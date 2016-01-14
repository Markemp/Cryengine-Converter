using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkSourceInfo : Chunk  // cccc0013:  Source Info chunk.  Pretty useless overall
    {
        public String SourceFile;
        public String Date;
        public String Author;

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Verbose, "*** SOURCE INFO CHUNK ***");
            Utils.Log(LogLevelEnum.Verbose, "    ID: {0:X}", this.ID);
            Utils.Log(LogLevelEnum.Verbose, "    Sourcefile: {0}.", this.SourceFile);
            Utils.Log(LogLevelEnum.Verbose, "    Date:       {0}.", this.Date);
            Utils.Log(LogLevelEnum.Verbose, "    Author:     {0}.", this.Author);
            Utils.Log(LogLevelEnum.Verbose, "*** END SOURCE INFO CHUNK ***");
        }
    }

}
