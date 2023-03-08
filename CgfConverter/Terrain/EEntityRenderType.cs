using System.Diagnostics.CodeAnalysis;

namespace CgfConverter.Terrain;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum EErType
{
    NotRenderNode = 0,
    Brush = 1,
    Vegetation = 2,
    Light = 3,
    Cloud = 4,
    VoxelObject = 5,
    FogVolume = 6,
    Decal = 7,
    ParticleEmitter = 8,
    WaterVolume = 9,
    WaterWave = 10,
    Road = 11,
    DistanceCloud = 12,
    VolumeObject = 13,
    AutoCubeMap = 14,
    Rope = 15,
    PrismObject = 16,
    IsoMesh = 17,
    LightPropagationVolume = 18,
    RenderProxy = 19,
    GameEffect = 20,
    Unused = 21,
    LightShape = 22,
    Decal2 = 23,
    TypesNum = 24,
}