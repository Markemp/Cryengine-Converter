using CgfConverter.Models.Structs;
using System;
using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkTimingFormat_918 : ChunkTimingFormat
{
    public override void Read(BinaryReader reader)
    {
        base.Read(reader);

        SecsPerTick = reader.ReadSingle();
        TicksPerFrame = reader.ReadInt32();
        GlobalRange = new RangeEntity
        {
            Name = reader.ReadFString(32),  // Name is technically a String32
            Start = reader.ReadInt32(),
            End = reader.ReadInt32()
        };
    }
}
