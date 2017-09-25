using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkCompiledPhysicalProxies : Chunk        // 0xACDC0003:  Hit boxes?
    {
        // Properties.  VERY similar to datastream, since it's essential vertex info.
        public UInt32 Flags2;
        public UInt32 NumPhysicalProxies; // Number of data entries
        public UInt32 BytesPerElement; // Bytes per data entry
        //public UInt32 Reserved1;
        //public UInt32 Reserved2;
        public PhysicalProxy[] PhysicalProxies;

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Debug, "*** START CompiledPhysicalProxies Chunk ***");
            Utils.Log(LogLevelEnum.Debug, "    ChunkType:           {0}", ChunkType);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", ID);
            Utils.Log(LogLevelEnum.Debug, "    Number of Targets:   {0:X}", NumPhysicalProxies);

        }
    }
}
