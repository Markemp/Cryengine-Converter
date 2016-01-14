using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkCompiledPhysicalProxies : Chunk        // 0xACDC0003:  Hit boxes?
    {
        // Properties.  VERY similar to datastream, since it's essential vertex info.
        public UInt32 Flags2;
        public UInt32 NumBones; // Number of data entries
        public UInt32 BytesPerElement; // Bytes per data entry
        //public UInt32 Reserved1;
        //public UInt32 Reserved2;
        public HitBox[] HitBoxes;

        public override void WriteChunk()
        {
            base.WriteChunk();
        }
    }
}
