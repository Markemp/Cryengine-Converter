using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkCompiledBones_900 : ChunkCompiledBones
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            NumBones = b.ReadInt32();

            for (int i = 0; i < NumBones; i++)
            {
                CompiledBone tempBone = new CompiledBone();
                tempBone.ReadCompiledBone_900(b);
            }
        }
    }
}
