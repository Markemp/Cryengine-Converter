using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkExportFlags_1 : ChunkExportFlags
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            UInt32 tmpExportFlag = b.ReadUInt32();
            this.ChunkType = (ChunkTypeEnum)Enum.ToObject(typeof(ChunkTypeEnum), tmpExportFlag);
            this.Version = b.ReadUInt32();
            this.ChunkOffset = b.ReadUInt32();
            this.ID = b.ReadInt32();

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
    }
}
