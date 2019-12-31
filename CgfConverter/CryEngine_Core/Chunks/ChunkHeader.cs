using System;
using System.Text;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkHeader : Chunk
    {
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("*** CHUNK HEADER ***");
            sb.AppendFormat("    ChunkType: {0}", this.ChunkType);
            sb.AppendFormat("    ChunkVersion: {0:X}", this.Version);
            sb.AppendFormat("    Offset: {0:X}", this.Offset);
            sb.AppendFormat("    ID: {0:X}", this.ID);
            sb.AppendFormat("    Size: {0:X}", this.Size);
            sb.AppendFormat("*** END CHUNK HEADER ***");

            return sb.ToString();
        }
    }
}
