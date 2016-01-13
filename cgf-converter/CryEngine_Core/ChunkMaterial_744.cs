using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    // cccc0014:  provides material name as used in the .mtl file
    public class ChunkMtlName_744 : CryEngine_Core.ChunkMtlName
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.Name = b.ReadFString(128);
            this.NumChildren = b.ReadUInt32();
            this.PhysicsTypeArray = new MtlNamePhysicsType[NumChildren];

            for (Int32 i = 0; i < NumChildren; i++)
            {
                this.PhysicsTypeArray[i] = (MtlNamePhysicsType)Enum.ToObject(typeof(MtlNamePhysicsType), b.ReadUInt32());
            }
        }
    }
}
