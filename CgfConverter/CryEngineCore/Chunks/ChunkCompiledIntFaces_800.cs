using CgfConverter.Models.Structs;
using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkCompiledIntFaces_800 : ChunkCompiledIntFaces
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        NumIntFaces = DataSize / 6;        // This is an array of TFaces, which are 3 uint16.
        Faces = new TFace[NumIntFaces];
        for (int i = 0; i < NumIntFaces; i++)
        {
            Faces[i].I0 = b.ReadUInt16();
            Faces[i].I1 = b.ReadUInt16();
            Faces[i].I2 = b.ReadUInt16();
        }
    }
}
