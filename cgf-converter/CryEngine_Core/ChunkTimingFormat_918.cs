using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkTimingFormat_918 : ChunkTimingFormat
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
}
