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

        public override void Read(BinaryReader b)
        {
            base.Read(b);

            //Console.WriteLine("ID is:  {0}", id);
            this.ControllerType = (CtrlType)Enum.ToObject(typeof(CtrlType), b.ReadUInt32());
            this.NumKeys = b.ReadUInt32();
            this.ControllerFlags = b.ReadUInt32();
            this.ControllerID = b.ReadUInt32();
            this.Keys = new Key[NumKeys];

            for (Int32 i = 0; i < this.NumKeys; i++)
            {
                // Will implement fully later.  Not sure I understand the structure, or if it's necessary.
                this.Keys[i].Time = b.ReadInt32();
                // Console.WriteLine("Time {0}", Keys[i].Time);
                this.Keys[i].AbsPos.x = b.ReadSingle();
                this.Keys[i].AbsPos.y = b.ReadSingle();
                this.Keys[i].AbsPos.z = b.ReadSingle();
                // Console.WriteLine("Abs Pos: {0:F7}  {1:F7}  {2:F7}", Keys[i].AbsPos.x, Keys[i].AbsPos.y, Keys[i].AbsPos.z);
                this.Keys[i].RelPos.x = b.ReadSingle();
                this.Keys[i].RelPos.y = b.ReadSingle();
                this.Keys[i].RelPos.z = b.ReadSingle();
                // Console.WriteLine("Rel Pos: {0:F7}  {1:F7}  {2:F7}", Keys[i].RelPos.x, Keys[i].RelPos.y, Keys[i].RelPos.z);
            }
        }

        public override void WriteChunk()
        {
            Console.WriteLine("*** Controller Chunk ***");
            Console.WriteLine("Version:                 {0:X}", Version);
            Console.WriteLine("ID:                      {0:X}", ID);
            Console.WriteLine("Number of Keys:          {0}", NumKeys);
            Console.WriteLine("Controller Type:         {0}", ControllerType);
            Console.WriteLine("Conttroller Flags:       {0}", ControllerFlags);
            Console.WriteLine("Controller ID:           {0}", ControllerID);
            for (Int32 i = 0; i < NumKeys; i++)
            {
                Console.WriteLine("        Key {0}:       Time: {1}", i, Keys[i].Time);
                Console.WriteLine("        AbsPos {0}:    {1:F7}, {2:F7}, {3:F7}", i, Keys[i].AbsPos.x, Keys[i].AbsPos.y, Keys[i].AbsPos.z);
                Console.WriteLine("        RelPos {0}:    {1:F7}, {2:F7}, {3:F7}", i, Keys[i].RelPos.x, Keys[i].RelPos.y, Keys[i].RelPos.z);
            }
        }
    }
}
