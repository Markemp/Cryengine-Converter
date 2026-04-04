using CgfConverter.Models.Structs;
using System.IO;

namespace CgfConverter.CryEngineCore.Chunks;

internal sealed class ChunkTimingFormat_919 : ChunkTimingFormat
{
    public override void Read(BinaryReader reader)
    {
        base.Read(reader);

        // 919 is a condensed 12-byte format: flags (int32), ticks/sec (float), num sub-ranges (int32).
        // GlobalRange and per-frame tick count are not stored; defaults are applied.
        _ = reader.ReadInt32();  // flags/unknown, purpose TBD
        var ticksPerSecond = reader.ReadSingle();
        NumSubRanges = reader.ReadInt32();

        SecsPerTick = 1.0f / ticksPerSecond;
        TicksPerFrame = 1;
        GlobalRange = new RangeEntity { Name = string.Empty, Start = 0, End = 0 };
    }
}
