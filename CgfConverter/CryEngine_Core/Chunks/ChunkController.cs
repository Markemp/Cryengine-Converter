using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkController : Chunk    // cccc000d:  Controller chunk
    {
        public CtrlType ControllerType { get; internal set; }
        public uint NumKeys { get; internal set; }
        public uint ControllerFlags { get; internal set; }        // technically a bitstruct to identify a cycle or a loop.
        public uint ControllerID { get; internal set; }           // Unique id based on CRC32 of bone name.  Ver 827 only?
        public Key[] Keys { get; internal set; }                    // array length NumKeys.  Ver 827?

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Number of Keys: {NumKeys}, Controller ID: {ControllerID:X}, Controller Type: {ControllerType}, Controller Flags: {ControllerFlags}";
        }
    }
}
