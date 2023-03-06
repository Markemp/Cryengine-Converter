using System;
using System.Diagnostics.CodeAnalysis;

namespace CgfConverter.Terrain;

[Flags]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum StatInstGroupChunkFlags
{
    AlignToTerrain = 1 << 0,
    UseTerrainColor = 1 << 1,
    AffectedByVoxels = 1 << 2,
    Hideability = 1 << 3,
    HideabilitySecondary = 1 << 4,
    Pickable = 1 << 5,
    ComplexBending = 1 << 6,
    CastShadow = 1 << 7,
    RecvShadow = 1 << 8,
    PrecShadow = 1 << 9,
    UseAlphaBlending = 1 << 10,
    UseSprites = 1 << 11,
    RandomRotation = 1 << 12,
    AllowIndoor = 1 << 13,
    PlayerHideableMask = 1 << 13 | 1 << 14,
    RecvDecals = 1 << 15,
}