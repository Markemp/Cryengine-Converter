using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkTimingFormat : Chunk  // cccc000e:  Timing format chunk
    {
        // This chunk doesn't have an ID, although one may be assigned in the chunk table.
        public Single SecsPerTick;
        public int TicksPerFrame;
        public RangeEntity GlobalRange;
        public int NumSubRanges;

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Ticks per Frame: {TicksPerFrame}, Seconds per Tick: {SecsPerTick}";
        }
    }
}
