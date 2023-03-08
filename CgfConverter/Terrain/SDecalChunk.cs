using System;
using System.IO;
using System.Numerics;
using CgfConverter.Structs;
using Extensions;

namespace CgfConverter.Terrain;

public class SDecalChunk : SRenderNodeChunk
{
    public readonly short ProjectionType;
    public readonly byte Deferred;
    public readonly byte Depth;
    public readonly int Unknown;
    public readonly Vector3 Pos;
    public readonly Vector3 Normal;
    public readonly Matrix3x3 ExplicitRightUpFront;
    public readonly float Radius;
    public readonly int MaterialId;
    public readonly int SortPriority;

    public SDecalChunk(BinaryReader reader, int version) : base(reader, version)
    {
        switch (version)
        {
            case 4:
                reader.ReadInto(out ProjectionType);
                reader.ReadInto(out Deferred);
                reader.ReadInto(out Depth);
                reader.ReadInto(out Unknown);
                reader.ReadInto(out Pos);
                reader.ReadInto(out Normal);
                reader.ReadInto(out ExplicitRightUpFront);
                reader.ReadInto(out Radius);
                reader.ReadInto(out MaterialId);
                reader.ReadInto(out SortPriority);
                return;
        }

        throw new NotSupportedException();
    }
}