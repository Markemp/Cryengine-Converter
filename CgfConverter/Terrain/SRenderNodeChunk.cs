using System;
using System.IO;
using Extensions;

namespace CgfConverter.Terrain;

public class SRenderNodeChunk
{
    public readonly int ChunkVersion;
    public readonly AaBb WsBBox;
    public readonly short LayerId;
    public readonly short Dummy;
    public readonly ERenderFlags RenderFlags;
    public readonly short ObjectTypeId;
    public readonly byte ViewDistRatio;
    public readonly byte LodRatio;

    public SRenderNodeChunk(BinaryReader reader, int version)
    {
        ChunkVersion = version;
        switch (version)
        {
            case 4:
                reader.ReadInto(out WsBBox);
                reader.ReadInto(out LayerId);
                reader.ReadInto(out Dummy);
                RenderFlags = (ERenderFlags) reader.ReadUInt32();
                reader.ReadInto(out ObjectTypeId);
                reader.ReadInto(out ViewDistRatio);
                reader.ReadInto(out LodRatio);
                return;
        }

        throw new NotSupportedException();
    }
}