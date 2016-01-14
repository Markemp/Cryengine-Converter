using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkTimingFormat : Chunk  // cccc000e:  Timing format chunk
    {
        // This chunk doesn't have an ID, although one may be assigned in the chunk table.
        public Single SecsPerTick;
        public Int32 TicksPerFrame;
        public RangeEntity GlobalRange;
        public Int32 NumSubRanges;

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            this.SecsPerTick = reader.ReadSingle();
            this.TicksPerFrame = reader.ReadInt32();
            this.GlobalRange.Name = reader.ReadFString(32);  // Name is technically a String32, but F those structs
            this.GlobalRange.Start = reader.ReadInt32();
            this.GlobalRange.End = reader.ReadInt32();
        }

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Verbose, "*** TIMING CHUNK ***");
            Utils.Log(LogLevelEnum.Verbose, "    ID: {0:X}", this.ID);
            Utils.Log(LogLevelEnum.Verbose, "    Version: {0:X}", this.Version);
            Utils.Log(LogLevelEnum.Verbose, "    Secs Per Tick: {0}", this.SecsPerTick);
            Utils.Log(LogLevelEnum.Verbose, "    Ticks Per Frame: {0}", this.TicksPerFrame);
            Utils.Log(LogLevelEnum.Verbose, "    Global Range:  Name: {0}", this.GlobalRange.Name);
            Utils.Log(LogLevelEnum.Verbose, "    Global Range:  Start: {0}", this.GlobalRange.Start);
            Utils.Log(LogLevelEnum.Verbose, "    Global Range:  End:  {0}", this.GlobalRange.End);
            Utils.Log(LogLevelEnum.Verbose, "*** END TIMING CHUNK ***");
        }
    }
}
