using System;
using System.IO;
using System.Numerics;
using Extensions;

namespace CgfConverter.Terrain;

public class SVegetationChunk : SRenderNodeChunk
{
    public readonly int Unknown;
    public readonly Vector3 Pos;
    public readonly float Scale;
    public readonly byte Bright;
    public readonly byte AngleZ;  // [0..255] <=> [0..pi]

    public SVegetationChunk(BinaryReader reader, int version) : base(reader, version)
    {
        switch (version)
        {
            case 4:
                reader.ReadInto(out Unknown);
                reader.ReadInto(out Pos);
                reader.ReadInto(out Scale);
                reader.ReadInto(out Bright);
                reader.ReadInto(out AngleZ);
                return;
        }

        throw new NotSupportedException();
    }
}