using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace CgfConverter.Terrain;

public class TerrainFile
{
    public readonly STerrainChunkHeader Header;
    public readonly ImmutableList<StatInstGroupChunk> StaticInstanceGroups;
    public readonly ImmutableList<string> BrushObjects;
    public readonly ImmutableList<string> BrushMaterials;
    public readonly STerrainNodeChunk TerrainNode;
    public readonly SOcTreeNodeChunk OcTreeNode;

    public TerrainFile(BinaryReader reader, bool closeAfter = false)
    {
        try
        {
            Header = new STerrainChunkHeader(reader);
            StaticInstanceGroups = Enumerable.Range(0, reader.ReadInt32())
                .Select(_ => new StatInstGroupChunk(reader, Header.Version))
                .ToImmutableList();
            BrushObjects = Enumerable.Range(0, reader.ReadInt32())
                .Select(_ => reader.ReadFString(0x100))
                .ToImmutableList();
            BrushMaterials = Enumerable.Range(0, reader.ReadInt32())
                .Select(_ => reader.ReadFString(0x100))
                .ToImmutableList();
            TerrainNode = new STerrainNodeChunk(reader, Header.TerrainInfo.UnitsToSectorBitshift);
            OcTreeNode = new SOcTreeNodeChunk(reader);
        }
        finally
        {
            if (closeAfter)
                reader.Close();
        }
    }
}