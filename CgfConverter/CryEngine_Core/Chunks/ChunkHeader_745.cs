using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkHeader_745 : CryEngine_Core.ChunkHeader_744
    {
        public override void Read(BinaryReader reader)
        {
            UInt32 headerType = reader.ReadUInt32();
            this.ChunkType = (ChunkTypeEnum)headerType;
            this.Version = (UInt32)reader.ReadUInt32();
            this.Offset = reader.ReadUInt32();
            this.ID = reader.ReadInt32();
            this.Size = reader.ReadUInt32();

            // if (this.ChunkType == ChunkTypeEnum.Timing)
            // {
            //     this.ID = this.ID + 0xFFFF0000;
            // }
        }

        public override void Write(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
