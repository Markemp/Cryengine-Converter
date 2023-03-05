using System;
using System.IO;
using Extensions;

namespace CgfConverter.Terrain;

public class StatInstGroupChunk
{
    public readonly string FileName;
    public readonly float Bending;
    public readonly float SpriteDistRatio;
    public readonly float ShadowDistRatio;
    public readonly float MaxViewDistRatio;
    public readonly float Brightness;
    public readonly int MaterialLayers;
    public readonly float Density;
    public readonly float ElevationMax;
    public readonly float ElevationMin;
    public readonly float Size;
    public readonly float SizeVar;
    public readonly float SlopeMax;
    public readonly float SlopeMin;
    public readonly float StatObjRadiusNotUsed;
    public readonly float StatObjRadiusVertNotUsed;
    public readonly float LodDistRatio;
    public readonly int Reserved;
    public readonly int Flags;
    public readonly int MaterialId;
    public readonly int RenderFlags;

    public StatInstGroupChunk(BinaryReader reader, int version)
    {
        switch (version)
        {
            case 24:
                FileName = reader.ReadFString(0x100);
                reader.ReadInto(out Bending);
                reader.ReadInto(out SpriteDistRatio);
                reader.ReadInto(out ShadowDistRatio);
                reader.ReadInto(out MaxViewDistRatio);
                reader.ReadInto(out Brightness);
                reader.ReadInto(out MaterialLayers);
                reader.ReadInto(out Density);
                reader.ReadInto(out ElevationMax);
                reader.ReadInto(out ElevationMin);
                reader.ReadInto(out Size);
                reader.ReadInto(out SizeVar);
                reader.ReadInto(out SlopeMax);
                reader.ReadInto(out SlopeMin);
                reader.ReadInto(out StatObjRadiusNotUsed);
                reader.ReadInto(out StatObjRadiusVertNotUsed);
                reader.ReadInto(out LodDistRatio);
                reader.ReadInto(out Reserved);
                reader.ReadInto(out Flags);
                reader.ReadInto(out MaterialId);
                reader.ReadInto(out RenderFlags);
                return;
        }

        throw new NotSupportedException();
    }
}