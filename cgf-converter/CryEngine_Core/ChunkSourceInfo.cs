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

        public override void Read(BinaryReader b)
        {
            Console.WriteLine("{0:X}+{1:X}", this._header.Offset, this._header.Size);
            base.Read(b);
            Console.WriteLine("{0:X}+{1:X}", this.Offset, this.Size);

            this.ChunkType = ChunkTypeEnum.SourceInfo; // this chunk doesn't actually have the chunktype header.
            this.SourceFile = b.ReadCString();
            this.Date = b.ReadCString().TrimEnd(); // Strip off last 2 Characters, because it contains a return
            this.Author = b.ReadCString();

            this.WriteChunk();
        }

        public override void WriteChunk()
        {
            Console.WriteLine("*** SOURCE INFO CHUNK ***");
            Console.WriteLine("    ID: {0:X}", ID);
            Console.WriteLine("    Sourcefile: {0}.  Length {1}", SourceFile, SourceFile.Length);
            Console.WriteLine("    Date:       {0}.  Length {1}", Date, Date.Length);
            Console.WriteLine("    Author:     {0}.  Length {1}", Author, Author.Length);
            Console.WriteLine("*** END SOURCE INFO CHUNK ***");
        }
    }

}
