using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkHelper_744 : CryEngineCore.ChunkHelper
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.HelperType = (HelperTypeEnum)Enum.ToObject(typeof(HelperTypeEnum), b.ReadUInt32());
            if (this.Version == 0x744)  // only has the Position.
            {
                this.Pos.x = b.ReadSingle();
                this.Pos.y = b.ReadSingle();
                this.Pos.z = b.ReadSingle();
            }
            else if (this.Version == 0x362)   // will probably never see these.
            {
                Char[] tmpName = new Char[64];
                tmpName = b.ReadChars(64);
                Int32 stringLength = 0;
                for (Int32 i = 0, j = tmpName.Length; i < j; i++)
                {
                    if (tmpName[i] == 0)
                    {
                        stringLength = i;
                        break;
                    }
                }
                this.Name = new string(tmpName, 0, stringLength);
                this.HelperType = (HelperTypeEnum)Enum.ToObject(typeof(HelperTypeEnum), b.ReadUInt32());
                this.Pos.x = b.ReadSingle();
                this.Pos.y = b.ReadSingle();
                this.Pos.z = b.ReadSingle();
            }
        }
    }
}
