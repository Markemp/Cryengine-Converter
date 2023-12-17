using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Terrain;

namespace CgfConverter.Renderers.Gltf;

public class GltfTerrainRenderer : BaseGltfRenderer, IRenderer
{
    private readonly CryTerrain _cryTerrain;
    private readonly List<float> _baseTranslation;

    public GltfTerrainRenderer(ArgsHandler argsHandler, CryTerrain cryTerrain, bool writeText, bool writeBinary)
        : base(argsHandler, cryTerrain.BaseName, writeText, writeBinary)
    {
        _cryTerrain = cryTerrain;

        _baseTranslation = new List<float>
        {
            (_cryTerrain.TerrainFile.OcTreeNode.NodeBox.Max.X - _cryTerrain.TerrainFile.OcTreeNode.NodeBox.Min.X) / 2,
            0,
            (_cryTerrain.TerrainFile.OcTreeNode.NodeBox.Max.Z - _cryTerrain.TerrainFile.OcTreeNode.NodeBox.Min.Z) / -2,
        };
    }

    private bool CreateNodeForEntityRecursive(
        out GltfNode newNode,
        CryTerrain terrain,
        CryTerrainEntity entity,
        Func<string, bool>? layerFilter = null)
    {
        newNode = new GltfNode {Name = entity.Underlying.Name};
        if (entity.Underlying.ScaleValue is { } scaleValue && scaleValue != Vector3.One)
        {
            scaleValue = SwapAxesForScale(scaleValue);
            newNode.Scale = new List<float> {scaleValue.X, scaleValue.Y, scaleValue.Z};
        }

        if (entity.Underlying.RotateValue is {IsIdentity: false} rotateValue)
        {
            rotateValue = SwapAxesForLayout(rotateValue);
            newNode.Rotation = new List<float> {rotateValue.X, rotateValue.Y, rotateValue.Z, rotateValue.W};
        }

        if (entity.Underlying.PosValue is { } translateValue && translateValue != Vector3.Zero)
        {
            translateValue = SwapAxesForPosition(translateValue);
            newNode.Translation = new List<float> {translateValue.X, translateValue.Y, translateValue.Z};
        }

        foreach (var child in entity.Children)
        {
            if (!CreateNodeForEntityRecursive(out var node, terrain, child, layerFilter))
                continue;

            newNode.Children.Add(AddNode(node));
        }

        if (layerFilter != null && (entity.Underlying.Layer is null || !layerFilter(entity.Underlying.Layer)))
            return newNode.Children.Any();

        foreach (var name in entity.Underlying.AllAttachedModelPaths)
        {
            if (Args.IsNodeNameExcluded(name))
                continue;

            if (!terrain.Objects.TryGetValue(name, out var cryObject))
                continue;

            if (!CreateGltfNode(out var node, cryObject, true))
                continue;

            newNode.Children.Add(AddNode(node));
        }

        return newNode.Children.Any();
    }

    private bool CreateNodeForLayerRecursive(
        out GltfNode newNode,
        CryTerrain terrain,
        CryTerrainLayer layer)
    {
        newNode = new GltfNode
        {
            Name = layer.Name
        };

        foreach (var renderNode in layer.RenderNodes)
        {
            string name;
            Vector3 scale, translation;
            Quaternion rotation;

            switch (renderNode)
            {
                case SBrushChunk chunk:
                    name = terrain.TerrainFile.BrushObjects[chunk.ObjectTypeId];

                    if (!Matrix4x4.Decompose(Matrix4x4.Transpose(chunk.Matrix.ConvertToTransformMatrix()),
                            out scale,
                            out rotation,
                            out translation))
                        throw new Exception();
                    break;

                case SVegetationChunkEx chunk:
                {
                    var sigc = terrain.TerrainFile.StaticInstanceGroups[chunk.ObjectTypeId];
                    name = sigc.FileName;

                    translation = chunk.Pos;
                    scale = new Vector3(chunk.Scale);
                    rotation = chunk.GetRotation(sigc);
                    break;
                }

                default:
                    // Unsupported
                    Log.D("Unsupported renderer node type: {0}", renderNode.GetType().Name);
                    continue;
            }

            name = name
                .Replace("%level%", terrain.BasePath)
                .ToLowerInvariant()
                .Replace('\\', '/');
            if (Args.IsNodeNameExcluded(name))
                continue;

            if (!terrain.Objects.TryGetValue(name, out var cryObject))
                continue;

            if (!CreateGltfNode(out var node, cryObject, true))
                continue;

            if (translation != Vector3.Zero)
            {
                translation = SwapAxesForPosition(translation);
                node.Translation = new List<float> {translation.X, translation.Y, translation.Z};
            }

            if (!rotation.IsIdentity)
            {
                rotation = SwapAxesForAnimations(rotation);
                node.Rotation = new List<float> {rotation.X, rotation.Y, rotation.Z, rotation.W};
            }

            if (scale != Vector3.One)
            {
                scale = SwapAxesForScale(scale);
                node.Scale = new List<float> {scale.X, scale.Y, scale.Z};
            }

            newNode.Children.Add(AddNode(node));
        }

        foreach (var sublayer in layer.Sublayers)
        {
            if (!CreateNodeForLayerRecursive(out var node, terrain, sublayer))
                continue;

            newNode.Children.Add(AddNode(node));
        }

        return newNode.Children.Any();
    }

    private bool RenderAsSingleFile()
    {
        Reset(_cryTerrain.BaseName);

        if (CreateNodeForLayerRecursive(out var layerNode, _cryTerrain, _cryTerrain.RootLayer))
        {
            layerNode.Name = "Layers";
            layerNode.Translation = _baseTranslation;
            CurrentScene.Nodes.Add(AddNode(layerNode));
        }

        if (CreateNodeForEntityRecursive(out var entityNode, _cryTerrain, _cryTerrain.RootEntity))
        {
            entityNode.Name = "Entities";
            entityNode.Translation = _baseTranslation;
            CurrentScene.Nodes.Add(AddNode(entityNode));
        }

        if (!CurrentScene.Nodes.Any())
            return Log.I<bool>("Layer[{0}]: Skipping; no object has been added.", CurrentScene.Name);

        Save(_cryTerrain.BasePath);
        return true;
    }

    private bool RenderLayer(CryTerrainLayer layer)
    {
        Reset($"{_cryTerrain.BaseName}:{layer.Name}");

        if (CreateNodeForLayerRecursive(out var layerNode, _cryTerrain, layer))
        {
            layerNode.Name = "Layers";
            layerNode.Translation = _baseTranslation;
            CurrentScene.Nodes.Add(AddNode(layerNode));
        }

        var layers = layer.RecursiveSublayers.Select(x => x.Name).ToHashSet();
        if (CreateNodeForEntityRecursive(out var entityNode, _cryTerrain, _cryTerrain.RootEntity,
                x => layers.Contains(x)))
        {
            entityNode.Name = "Entities";
            entityNode.Translation = _baseTranslation;
            CurrentScene.Nodes.Add(AddNode(entityNode));
        }

        if (!CurrentScene.Nodes.Any())
            return Log.I<bool>("Layer[{0}]: Skipping; no object has been added.", CurrentScene.Name);

        Save(_cryTerrain.BasePath, layer.Name);
        return true;
    }

    public int Render()
    {
        if (_cryTerrain.RootLayer.Sublayers.Count == 1 || !Args.SplitLayers)
            return RenderAsSingleFile() ? 1 : 0;

        return _cryTerrain.RootLayer.Sublayers.Where(RenderLayer).Count();
    }
}
