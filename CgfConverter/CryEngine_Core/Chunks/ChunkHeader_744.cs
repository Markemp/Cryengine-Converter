using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkHeader_744 : CryEngineCore.ChunkHeader
    {
        public override void Read(BinaryReader reader)
        {
            UInt32 headerType = reader.ReadUInt32();
            this.ChunkType = (ChunkTypeEnum)headerType;
            this.Version = (UInt32)reader.ReadUInt32();
            this.Offset = reader.ReadUInt32();
            this.ID = reader.ReadInt32();
            this.Size = 0; // TODO: Figure out how to return a size - postprocess header table maybe?

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
