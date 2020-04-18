using System.IO;

namespace CgfConverter.CryEngineCore
{
    /// <summary>
    /// Remapped to CryEngine_744 format for now
    /// </summary>
    public class ChunkMtlName_802 : CryEngineCore.ChunkMtlName_744
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            // Appears to have 4 more Bytes than ChunkMtlName_744
        }
    }
}
