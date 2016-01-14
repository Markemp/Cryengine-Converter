using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public interface IBinaryChunk
    {
        void Read(BinaryReader reader);
        void Write(BinaryWriter writer);
    }
}
