using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    /// <summary>
    /// Remapped to CryEngine_744 format for now
    /// </summary>
    public class ChunkMtlName_802 : CryEngine_Core.ChunkMtlName_744
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            // Appears to have 4 more Bytes than ChunkMtlName_744
        }
    }
}
