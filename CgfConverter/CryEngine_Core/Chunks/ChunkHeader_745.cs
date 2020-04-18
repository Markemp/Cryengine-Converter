using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkHeader_745 : CryEngineCore.ChunkHeader_744
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
