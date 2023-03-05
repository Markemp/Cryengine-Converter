using System;
using System.IO;
using Extensions;

namespace CgfConverter.Terrain;

public class SVegetationChunkEx : SVegetationChunk
{
    public readonly byte NormalX;
    public readonly byte NormalY;

    public SVegetationChunkEx(BinaryReader reader, int version) : base(reader, version)
    {
        switch (version)
        {
            case 4:
                reader.ReadInto(out NormalX);
                reader.ReadInto(out NormalY);
                return;
        }

        throw new NotSupportedException();
    }
}