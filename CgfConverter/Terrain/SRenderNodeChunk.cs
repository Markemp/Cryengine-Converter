using System;
using System.IO;
using Extensions;

namespace CgfConverter.Terrain;

public class SRenderNodeChunk
{
    public readonly AaBb WSBBox;
    public readonly short LayerId;
    public readonly short Dummy;
    public readonly int RenderFlags;
    public readonly short ObjectTypeId;
    public readonly byte ViewDistRatio;
    public readonly byte LodRatio;

    public SRenderNodeChunk(BinaryReader reader, int version)
    {
        switch (version)
        {
            case 4:
                reader.ReadInto(out WSBBox);
                reader.ReadInto(out LayerId);
                reader.ReadInto(out Dummy);
                reader.ReadInto(out RenderFlags);
                reader.ReadInto(out ObjectTypeId);
                reader.ReadInto(out ViewDistRatio);
                reader.ReadInto(out LodRatio);
                return;
        }

        throw new NotSupportedException();
    }
}