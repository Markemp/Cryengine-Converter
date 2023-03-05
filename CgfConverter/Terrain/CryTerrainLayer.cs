using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CgfConverter.Terrain.Xml;

namespace CgfConverter.Terrain;

public class CryTerrainLayer
{
    public readonly string Name;
    public readonly int? Id;
    public readonly ImmutableList<CryTerrainLayer> Sublayers;
    public readonly ImmutableList<SRenderNodeChunk> RenderNodes;

    public CryTerrainLayer(
        string? name,
        int? id,
        IEnumerable<Layer> remainingLayers,
        IList<SRenderNodeChunk> renderNodeChunks)
    {
        Name = name ?? "";
        Id = id;

        // It's inefficient.
        var myDirectChildren = new List<Layer>();
        var notMyDirectChildren = new List<Layer>();
        foreach (var layer in remainingLayers)
        {
            if (layer.Parent == Name)
                myDirectChildren.Add(layer);
            else
                notMyDirectChildren.Add(layer);
        }

        Sublayers = myDirectChildren
            .Select(layer => new CryTerrainLayer(
                layer.Name,
                layer.IdValue,
                notMyDirectChildren,
                renderNodeChunks))
            .ToImmutableList();

        RenderNodes = id is { } existingId
            ? renderNodeChunks.Where(x => x.LayerId == existingId).ToImmutableList()
            : ImmutableList<SRenderNodeChunk>.Empty;
    }

    public ImmutableList<CryTerrainLayer> RecursiveSublayers
        => Sublayers.Any()
            ? Sublayers.Concat(Sublayers.SelectMany(x => x.RecursiveSublayers)).ToImmutableList()
            : ImmutableList<CryTerrainLayer>.Empty;
}