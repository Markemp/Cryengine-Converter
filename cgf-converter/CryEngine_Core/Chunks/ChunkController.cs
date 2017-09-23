using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkController : Chunk    // cccc000d:  Controller chunk
    {
        public CtrlType ControllerType { get; internal set; }
        public UInt32 NumKeys { get; internal set; }
        public UInt32 ControllerFlags { get; internal set; }        // technically a bitstruct to identify a cycle or a loop.
        public UInt32 ControllerID { get; internal set; }           // Unique id based on CRC32 of bone name.  Ver 827 only?
        public Key[] Keys { get; internal set; }                  // array length NumKeys.  Ver 827?

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Verbose, "*** Controller Chunk ***");
            Utils.Log(LogLevelEnum.Verbose, "Version:                 {0:X}", Version);
            Utils.Log(LogLevelEnum.Verbose, "ID:                      {0:X}", ID);
            Utils.Log(LogLevelEnum.Verbose, "Number of Keys:          {0}", NumKeys);
            Utils.Log(LogLevelEnum.Verbose, "Controller Type:         {0}", ControllerType);
            Utils.Log(LogLevelEnum.Verbose, "Conttroller Flags:       {0}", ControllerFlags);
            Utils.Log(LogLevelEnum.Verbose, "Controller ID:           {0}", ControllerID);
            for (Int32 i = 0; i < NumKeys; i++)
            {
                Utils.Log(LogLevelEnum.Verbose, "        Key {0}:       Time: {1}", i, Keys[i].Time);
                Utils.Log(LogLevelEnum.Verbose, "        AbsPos {0}:    {1:F7}, {2:F7}, {3:F7}", i, Keys[i].AbsPos.x, Keys[i].AbsPos.y, Keys[i].AbsPos.z);
                Utils.Log(LogLevelEnum.Verbose, "        RelPos {0}:    {1:F7}, {2:F7}, {3:F7}", i, Keys[i].RelPos.x, Keys[i].RelPos.y, Keys[i].RelPos.z);
            }
        }
    }
}
