using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkSourceInfo : Chunk  // cccc0013:  Source Info chunk.  Pretty useless overall
    {
        public string SourceFile;
        public string Date;
        public string Author;

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Sourcefile: {SourceFile}";
        }
    }
}
