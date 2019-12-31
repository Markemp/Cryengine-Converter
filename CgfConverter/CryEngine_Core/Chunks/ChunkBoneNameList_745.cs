using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public class ChunkBoneNameList_745 : ChunkBoneNameList
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            BoneNames = b.ReadCString().Split(' ').ToList();
        }
    }
}
