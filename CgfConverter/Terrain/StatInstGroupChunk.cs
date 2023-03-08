using System;
using System.IO;
using Extensions;

namespace CgfConverter.Terrain;

public class StatInstGroupChunk
{
    public readonly int OcTreeChunkVersion;
    public readonly string FileName;
    public readonly float Bending;
    public readonly float SpriteDistRatio;
    public readonly float ShadowDistRatio;
    public readonly float MaxViewDistRatio;
    public readonly float Brightness;
    public readonly int RotationRangeToTerrainNormal;
    public readonly float AlignToTerrainCoefficient;
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
    public readonly int IdPlusOne;
    public readonly float LodDistRatio;
    public readonly int Reserved;
    public readonly StatInstGroupChunkFlags Flags;
    public readonly int MaterialId;
    public readonly ERenderFlags RenderFlags;
    public readonly float Stiffness;
    public readonly float Damping;
    public readonly float Variance;
    public readonly float AirResistance;

    public StatInstGroupChunk(BinaryReader reader, int ocTreeChunkVersion)
    {
        OcTreeChunkVersion = ocTreeChunkVersion;
        switch (ocTreeChunkVersion)
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
            case 29:
                FileName = reader.ReadFString(0x100);
                reader.ReadInto(out Bending);
                reader.ReadInto(out SpriteDistRatio);
                reader.ReadInto(out ShadowDistRatio);
                reader.ReadInto(out MaxViewDistRatio);
                reader.ReadInto(out Brightness);
                reader.ReadInto(out RotationRangeToTerrainNormal);
                reader.ReadInto(out AlignToTerrainCoefficient);
                reader.ReadInto(out MaterialLayers);
                reader.ReadInto(out Density);
                reader.ReadInto(out ElevationMax);
                reader.ReadInto(out ElevationMin);
                reader.ReadInto(out Size);
                reader.ReadInto(out SizeVar);
                reader.ReadInto(out SlopeMax);
                reader.ReadInto(out SlopeMin);
                reader.ReadInto(out StatObjRadiusNotUsed);
                reader.ReadInto(out IdPlusOne);
                reader.ReadInto(out LodDistRatio);
                reader.ReadInto(out Reserved);
                reader.ReadInto(out Flags);
                reader.ReadInto(out MaterialId);
                reader.ReadInto(out RenderFlags);
                reader.ReadInto(out Stiffness);
                reader.ReadInto(out Damping);
                reader.ReadInto(out Variance);
                reader.ReadInto(out AirResistance);
                return;
        }

        throw new NotSupportedException();
    }

    public float AlignToTerrainAmount => OcTreeChunkVersion switch
    {
        4 => (Flags & StatInstGroupChunkFlags.AlignToTerrain) != 0 ? 1f : 0f,
        _ => throw new NotSupportedException(),
    };
}