using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using CgfConverter.Structs;
using Extensions;

namespace CgfConverter.Terrain;

public class SWaterVolumeChunk : SRenderNodeChunk
{
    public readonly int VolumeTypeAndMiscBits;
    public readonly ulong VolumeId;
    public readonly int MaterialId;	
    public readonly float FogDensity;
    public readonly Vector3 FogColor;
    public readonly Plane FogPlane;
    public readonly float Unknown;
    public readonly float TexCoordBegin;
    public readonly float TexCoordEnd;
    public readonly float SurfUScale;
    public readonly float SurfVScale;
    public readonly int NumVertices;
    public readonly float VolumeDepth;
    public readonly float StreamSpeed;
    public readonly int NumVerticesPhysAreaContour;

    public readonly IImmutableList<Vector3> Vertices;
    public readonly IImmutableList<Vector3> PhysicsAreaContour;

    public SWaterVolumeChunk(BinaryReader reader, int version) : base(reader, version)
    {
        switch (version)
        {
            case 4:
                reader.ReadInto(out VolumeTypeAndMiscBits);
                reader.ReadInto(out VolumeId);
                reader.ReadInto(out MaterialId);
                reader.ReadInto(out FogDensity);
                reader.ReadInto(out FogColor);
                reader.ReadInto(out FogPlane);
                reader.ReadInto(out Unknown);
                reader.ReadInto(out TexCoordBegin);
                reader.ReadInto(out TexCoordEnd);
                reader.ReadInto(out SurfUScale);
                reader.ReadInto(out SurfVScale);
                reader.ReadInto(out NumVertices);
                reader.ReadInto(out VolumeDepth);
                reader.ReadInto(out StreamSpeed);
                reader.ReadInto(out NumVerticesPhysAreaContour);
                break;
            
            default:
                throw new NotSupportedException();
        }

        Vertices = Enumerable.Range(0, NumVertices).Select(_ => reader.ReadVector3()).ToImmutableList();
        PhysicsAreaContour = Enumerable.Range(0, NumVerticesPhysAreaContour).Select(_ => reader.ReadVector3()).ToImmutableList();
    }
}