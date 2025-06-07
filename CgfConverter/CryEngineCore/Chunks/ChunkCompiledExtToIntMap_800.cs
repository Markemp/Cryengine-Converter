using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkCompiledExtToIntMap_800 : ChunkCompiledExtToIntMap
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        NumExtVertices = DataSize / sizeof(ushort);
        Source = new ushort[NumExtVertices];
        for (int i = 0; i < NumExtVertices; i++)
        {
            Source[i] = b.ReadUInt16();
        }
    }
}
