using System;
using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkTimingFormat_918 : ChunkTimingFormat
{
    public override void Read(BinaryReader reader)
    {
        base.Read(reader);

        this.SecsPerTick = reader.ReadSingle();
        this.TicksPerFrame = reader.ReadInt32();
        this.GlobalRange.Name = reader.ReadFString(32);  // Name is technically a String32, but F those structs
        this.GlobalRange.Start = reader.ReadInt32();
        this.GlobalRange.End = reader.ReadInt32();
    }
}
