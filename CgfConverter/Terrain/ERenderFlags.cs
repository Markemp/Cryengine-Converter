using System;
using System.Diagnostics.CodeAnalysis;

namespace CgfConverter.Terrain;

[Flags]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum ERenderFlags : uint
{
    GoodOccluder = 1u << 0,
    Procedural = 1u << 1,
    CloneSource = 1u << 2,
    CastShadowMaps = 1u << 3,
    RenderAlways = 1u << 4,
    DynamicDistanceShadows = 1u << 5,
    Hidable = 1u << 6,
    HidableSecondary = 1u << 7,
    Hidden = 1u << 8,
    Selected = 1u << 9,
    UseNearestEnvProbe = 1u << 10,
    OutdoorOnly = 1u << 11,
    NoDynWater = 1u << 12,
    ExcludeFromTriangulation = 1u << 13,
    RegisterByBBox = 1u << 14,
    StaticInstancing = 1u << 15,
    VoxelizeStatic = 1u << 16,
    NoPhysics = 1u << 17,
    NoDecalNodeDecals = 1u << 18,
    RegisterByPosition = 1u << 19,
    ComponentEntity = 1u << 20,
    Recvwind = 1u << 21,
    CollisionProxy = 1u << 22, 
    LodBBoxBased = 1u << 23,
    SpecBit0 = 1u << 24,
    SpecBit1 = 1u << 25,
    SpecBit2 = 1u << 26,
    RaycastProxy = 1u << 27,
    Hud = 1u << 28,
    RainOccluder = 1u << 29,
    HasCastShadowMaps = 1u << 30,
    ActiveLayer = 1u << 31,
}