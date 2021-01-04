using System.Text;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkHeader : Chunk
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("*** CHUNK HEADER ***");
            sb.AppendFormat("    ChunkType: {0}", ChunkType);
            sb.AppendFormat("    ChunkVersion: {0:X}", Version);
            sb.AppendFormat("    Offset: {0:X}", Offset);
            sb.AppendFormat("    ID: {0:X}", ID);
            sb.AppendFormat("    Size: {0:X}", Size);
            sb.AppendFormat("*** END CHUNK HEADER ***");

            return sb.ToString();
        }
    }
}
