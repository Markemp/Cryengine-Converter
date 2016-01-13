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

        public override void Read(BinaryReader b)
        {
            base.Read(b);

            UInt32 tmpExportFlag = b.ReadUInt32();
            this.ChunkType = (ChunkTypeEnum)Enum.ToObject(typeof(ChunkTypeEnum), tmpExportFlag);
            this.Version = b.ReadUInt32();
            this.ChunkOffset = b.ReadUInt32();
            this.ID = b.ReadUInt32();

            this.SkipBytes(b, 4);

            this.RCVersion = new uint[4];
            Int32 count = 0;
            for (count = 0; count < 4; count++)
            {
                this.RCVersion[count] = b.ReadUInt32();
            }
            this.RCVersionString = b.ReadFString(16);

            this.SkipBytes(b);
        }

        public override void WriteChunk()
        {
            Console.WriteLine("*** START EXPORT FLAGS ***");
            Console.WriteLine("    Export Chunk ID: {0:X}", ID);
            Console.WriteLine("    ChunkType: {0}", ChunkType);
            Console.WriteLine("    Version: {0}", Version);
            Console.WriteLine("    Flags: {0}", Flags);
            Console.Write("    RC Version: ");
            for (Int32 i = 0; i < 4; i++)
            {
                Console.Write(RCVersion[i]);
            }
            Console.WriteLine();
            Console.WriteLine("    RCVersion String: {0}", this.RCVersionString);
            Console.WriteLine("*** END EXPORT FLAGS ***");
        }
    }
}
