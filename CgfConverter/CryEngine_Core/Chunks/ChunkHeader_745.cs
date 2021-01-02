using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkHeader_745 : ChunkHeader
    {
        public override void Read(BinaryReader reader)
        {
            uint headerType = reader.ReadUInt32();
            ChunkType = (ChunkTypeEnum)headerType;
            Version = reader.ReadUInt32();
            Offset = reader.ReadUInt32();
            ID = reader.ReadInt32();
            Size = reader.ReadUInt32();
        }
    }
}
