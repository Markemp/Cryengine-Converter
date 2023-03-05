using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using CgfConverter.Structs;
using Extensions;

namespace CgfConverter.Terrain;

public class SLightShape : SRenderNodeChunk
{
    public readonly int Unknown;
    public readonly Matrix3x4 WorldMatrix;
    public readonly int Flags;
    
    public SLightShape(BinaryReader reader, int version) : base(reader, version)
    {
        switch (version)
        {
            case 4:
                reader.ReadInto(out Unknown);
                reader.ReadInto(out WorldMatrix);
                reader.ReadInto(out Flags);
                break;
            
            default:
                throw new NotSupportedException();
        }
    }
}