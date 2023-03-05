using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;

namespace CgfConverter.Terrain;

public class SOcTreeNodeChunk
{
    public readonly short ChunkVersion;
    public readonly short ChildsMask;
    public readonly AaBb NodeBox;
    public readonly int ObjectsBlockSize;

    public readonly SRenderNodeChunk[] Objects;
    public readonly SOcTreeNodeChunk?[] Children;

    public SOcTreeNodeChunk(BinaryReader reader)
    {
        reader.ReadInto(out ChunkVersion);
        switch (ChunkVersion)
        {
            case 4:
                reader.ReadInto(out ChildsMask);
                reader.ReadInto(out NodeBox);
                reader.ReadInto(out ObjectsBlockSize);
                break;

            default:
                throw new NotSupportedException();
        }

        var objects = new List<SRenderNodeChunk>();
        var targetPosition = reader.BaseStream.Position + ObjectsBlockSize;
        while (reader.BaseStream.Position < targetPosition)
        {
            var type = (EerType) reader.ReadInt32();
            switch (type)
            {
                case EerType.Brush:
                    objects.Add(new SBrushChunk(reader, ChunkVersion));
                    break;

                case EerType.Vegetation:
                    objects.Add(new SVegetationChunkEx(reader, ChunkVersion));
                    break;

                default:
                    throw new NotSupportedException($"Render node chunk type of {type} is currently not supported.");
            }
        }

        if (reader.BaseStream.Position != targetPosition)
            throw new InvalidDataException();

        Objects = objects.ToArray();

        Children = Enumerable.Range(0, 8)
            .Select(i => (ChildsMask & (1 << i)) == 0 ? null : new SOcTreeNodeChunk(reader))
            .ToArray();
    }
}