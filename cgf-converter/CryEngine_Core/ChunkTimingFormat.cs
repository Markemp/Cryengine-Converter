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
            Console.WriteLine("*** TIMING CHUNK ***");
            Console.WriteLine("    ID: {0:X}", this.ID);
            Console.WriteLine("    Version: {0:X}", this.Version);
            Console.WriteLine("    Secs Per Tick: {0}", this.SecsPerTick);
            Console.WriteLine("    Ticks Per Frame: {0}", this.TicksPerFrame);
            Console.WriteLine("    Global Range:  Name: {0}", this.GlobalRange.Name);
            Console.WriteLine("    Global Range:  Start: {0}", this.GlobalRange.Start);
            Console.WriteLine("    Global Range:  End:  {0}", this.GlobalRange.End);
            Console.WriteLine("*** END TIMING CHUNK ***");
        }
    }
}
