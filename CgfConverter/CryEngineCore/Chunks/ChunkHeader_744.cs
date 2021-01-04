using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkHeader_744 : ChunkHeader
    {
        public override void Read(BinaryReader reader)
        {
            uint headerType = reader.ReadUInt32();
            ChunkType = (ChunkTypeEnum)headerType;
            Version = reader.ReadUInt32();
            Offset = reader.ReadUInt32();
            ID = reader.ReadInt32();
            Size = 0; // TODO: Figure out how to return a size - postprocess header table maybe?
        }
    }
}
