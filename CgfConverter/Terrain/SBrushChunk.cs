using System;
using System.IO;
using CgfConverter.Structs;
using Extensions;

namespace CgfConverter.Terrain;

public class SBrushChunk : SRenderNodeChunk
{
    public readonly int Unknown;
    public readonly Matrix3x4 Matrix;
    public readonly int MergeGroupId;
    public readonly int MaterialId;
    public readonly int MaterialLayers;

    public SBrushChunk(BinaryReader reader, int version) : base(reader, version)
    {
        switch (version)
        {
            case 4:
                reader.ReadInto(out Unknown);
                reader.ReadInto(out Matrix);
                reader.ReadInto(out MergeGroupId);
                reader.ReadInto(out MaterialId);
                reader.ReadInto(out MaterialLayers);
                return;
        }

        throw new NotSupportedException();
    }
}