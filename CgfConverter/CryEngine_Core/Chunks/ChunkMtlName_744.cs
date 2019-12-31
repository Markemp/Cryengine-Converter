using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    // cccc0014:  provides material name as used in the .mtl file
    public class ChunkMtlName_744 : CryEngineCore.ChunkMtlName
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.Name = b.ReadFString(128);
            this.NumChildren = b.ReadUInt32();
            this.PhysicsType = new MtlNamePhysicsType[NumChildren];
            this.MatType = this.NumChildren == 0 ? MtlNameTypeEnum.Single : MtlNameTypeEnum.Library;

            for (Int32 i = 0; i < NumChildren; i++)
            {
                this.PhysicsType[i] = (MtlNamePhysicsType)Enum.ToObject(typeof(MtlNamePhysicsType), b.ReadUInt32());
            }
        }
    }
}
