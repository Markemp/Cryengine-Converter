using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkSceneProp : Chunk     // cccc0008 
    {
        // This chunk isn't really used, but contains some data probably necessary for the game.
        // Size for 0x744 type is always 0xBB4 (test this)
        public UInt32 NumProps;             // number of elements in the props array  (31 for type 0x744)
        public String[] PropKey;
        public String[] PropValue;

        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.NumProps = b.ReadUInt32();          // Should be 31 for 0x744
            this.PropKey = new String[this.NumProps];
            this.PropValue = new String[this.NumProps];

            // Read the array of scene props and their associated values
            for (Int32 i = 0; i < this.NumProps; i++)
            {
                this.PropKey[i] = b.ReadFString(32);
                this.PropValue[i] = b.ReadFString(64);
            }
        }
        public override void WriteChunk()
        {
            Console.WriteLine("*** START SceneProp Chunk ***");
            Console.WriteLine("    ChunkType:   {0}", ChunkType);
            Console.WriteLine("    Version:     {0:X}", Version);
            Console.WriteLine("    ID:          {0:X}", ID);
            for (Int32 i = 0; i < NumProps; i++)
            {
                Console.WriteLine("{0,30}{1,20}", PropKey[i], PropValue[i]);
            }
            Console.WriteLine("*** END SceneProp Chunk ***");
        }
    }
}
