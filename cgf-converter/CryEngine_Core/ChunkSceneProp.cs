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
            Utils.Log(LogLevelEnum.Verbose, "*** START SceneProp Chunk ***");
            Utils.Log(LogLevelEnum.Verbose, "    ChunkType:   {0}", ChunkType);
            Utils.Log(LogLevelEnum.Verbose, "    Version:     {0:X}", Version);
            Utils.Log(LogLevelEnum.Verbose, "    ID:          {0:X}", ID);
            for (Int32 i = 0; i < NumProps; i++)
            {
                Utils.Log(LogLevelEnum.Verbose, "{0,30}{1,20}", PropKey[i], PropValue[i]);
            }
            Utils.Log(LogLevelEnum.Verbose, "*** END SceneProp Chunk ***");
        }
    }
}
