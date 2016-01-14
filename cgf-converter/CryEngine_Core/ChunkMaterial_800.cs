using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkMtlName_800 : CryEngine_Core.ChunkMtlName_744
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.MatType = b.ReadUInt32();  // if 0x1, then material lib.  If 0x12, mat name.  This is actually a bitstruct.
            this.SkipBytes(b, 4);
            this.Name = b.ReadFString(128);
            this.PhysicsType = (MtlNamePhysicsType)Enum.ToObject(typeof(MtlNamePhysicsType), b.ReadUInt32());
            this.NumChildren = b.ReadUInt32();
            // Now we need to read the Children references.  2 parts; the number of children, and then 66 - numchildren padding
            this.Children = new uint[NumChildren];
            for (Int32 i = 0; i < this.NumChildren; i++)
            {
                this.Children[i] = b.ReadUInt32();
            }
            this.SkipBytes(b);
        }
    }
}
