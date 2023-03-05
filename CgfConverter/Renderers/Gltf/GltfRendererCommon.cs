using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using CgfConverter.Materials;
using CgfConverter.PackFileSystem;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Terrain;

namespace CgfConverter.Renderers.Gltf;

public partial class GltfRendererCommon
{
    private readonly IPackFileSystem _packFileSystem;
    private readonly Dictionary<Material, int> _materialMap = new();
    private readonly GltfWriter _gltf;
    private readonly List<Regex> _excludedNodeNames;

    public GltfRendererCommon(IPackFileSystem packFileSystem, List<Regex> excludedNodeNames)
    {
        _packFileSystem = packFileSystem;
        _gltf = new GltfWriter();
        _excludedNodeNames = excludedNodeNames;
    }

    public void RenderSingleModel(CryEngine cryData, string outputName, bool writeBinary, bool writeText)
    {
        _gltf.Clear();
        _materialMap.Clear();
        _gltf.Add(new GltfScene
        {
            Name = "Scene",
        });

        if (!CreateModelNode(out var node, cryData))
            throw new NotSupportedException();

        _gltf.Scenes[_gltf.Scene].Nodes.Add(_gltf.Add(node));

        if (writeBinary)
        {
            using var glb = new FileStream($"{outputName}.glb", FileMode.Create, FileAccess.Write);
            _gltf.CompileToBinary(glb);
        }

        if (writeText)
        {
            using var gltf = new FileStream($"{outputName}.gltf", FileMode.Create, FileAccess.Write);
            using var bin = new FileStream($"{outputName}.bin", FileMode.Create, FileAccess.Write);
            _gltf.CompileToPair($"{outputName}.bin", gltf, bin);
        }
    }

    public bool CreateNodeForEntityRecursive(out GltfNode newNode, CryTerrain terrain, CryTerrainEntity entity,
        Func<string, bool>? layerFilter = null)
    {
        if (entity.Underlying.Name == "S07_Spike_Hazard")
            System.Diagnostics.Debugger.Break();

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

            newNode.Children.Add(_gltf.Add(node));
        }

        if (layerFilter != null && (entity.Underlying.Layer is null || !layerFilter(entity.Underlying.Layer)))
            return newNode.Children.Any();

        foreach (var name in entity.Underlying.AllAttachedModelPaths)
        {
            if (_excludedNodeNames.Any(y => y.IsMatch(name)))
                continue;

            if (!terrain.Objects.TryGetValue(name, out var cryObject))
                continue;

            if (!CreateModelNode(out var node, cryObject, true))
                continue;

            newNode.Children.Add(_gltf.Add(node));
        }

        return newNode.Children.Any();
    }

    private bool CreateNodeForLayerRecursive(out GltfNode newNode, CryTerrain terrain, CryTerrainLayer layer)
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
                    name = terrain.TerrainFile.Files[chunk.ObjectTypeId].FileName;

                    translation = chunk.Pos;
                    scale = new Vector3(chunk.Scale);

                    var x = (chunk.NormalX - 127) / 127f;
                    var y = (chunk.NormalY - 127) / 127f;
                    var terrainNormal = new Vector3
                    {
                        X = x,
                        Y = y
                    };
                    terrainNormal.Z = (float) Math.Sqrt(
                        1
                        - terrainNormal.X * terrainNormal.X
                        + terrainNormal.Y * terrainNormal.Y);

                    var vDir = Vector3.Cross(-Vector3.UnitX, terrainNormal);
                    var up = -terrainNormal;
                    var yAxis = Vector3.Normalize(vDir);
                    if (yAxis is {X: 0, Y: 0} && up == Vector3.UnitZ)
                        up = Vector3.UnitX * -yAxis.Z;

                    var xAxis = Vector3.Normalize(Vector3.Cross(up, yAxis));
                    var zAxis = Vector3.Normalize(Vector3.Cross(xAxis, yAxis));

                    rotation = Quaternion.CreateFromRotationMatrix(new Matrix4x4(
                        xAxis.X, xAxis.Y, xAxis.Z, 0,
                        yAxis.X, yAxis.Y, yAxis.Z, 0,
                        zAxis.X, zAxis.Y, zAxis.Z, 0,
                        0, 0, 0, 1));
                    break;

                case SDecalChunk:
                    // TODO
                    continue;

                default:
                    // Unsupported
                    continue;
            }

            name = name
                .Replace("%level%", terrain.BasePath)
                .ToLowerInvariant()
                .Replace('\\', '/');
            if (_excludedNodeNames.Any(y => y.IsMatch(name)))
                continue;

            if (!terrain.Objects.TryGetValue(name, out var cryObject))
                continue;

            if (!CreateModelNode(out var node, cryObject, true))
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

            newNode.Children.Add(_gltf.Add(node));
        }

        foreach (var sublayer in layer.Sublayers)
        {
            if (!CreateNodeForLayerRecursive(out var node, terrain, sublayer))
                continue;

            newNode.Children.Add(_gltf.Add(node));
        }

        return newNode.Children.Any();
    }

    public int RenderSingleTerrain(CryTerrain terrain, string outputName, bool writeBinary, bool writeText,
        bool separateLayers)
    {
        var numOutputSet = 0;

        var baseTranslationList = new List<float>
        {
            (terrain.TerrainFile.OcTreeNode.NodeBox.Max.X - terrain.TerrainFile.OcTreeNode.NodeBox.Min.X) / 2,
            0,
            (terrain.TerrainFile.OcTreeNode.NodeBox.Max.Z - terrain.TerrainFile.OcTreeNode.NodeBox.Min.Z) / -2,
        };

        if (terrain.RootLayer.Sublayers.Count == 1 || !separateLayers)
        {
            _gltf.Clear();
            _materialMap.Clear();
            _gltf.Add(new GltfScene {Name = Path.GetFileName(terrain.BasePath)});

            if (CreateNodeForLayerRecursive(out var layerNode, terrain, terrain.RootLayer))
            {
                layerNode.Name = "Layers";
                layerNode.Translation = baseTranslationList;
                _gltf.CurrentScene.Nodes.Add(_gltf.Add(layerNode));
            }

            if (CreateNodeForEntityRecursive(out var entityNode, terrain, terrain.RootEntity))
            {
                entityNode.Name = "Entities";
                entityNode.Translation = baseTranslationList;
                _gltf.CurrentScene.Nodes.Add(_gltf.Add(entityNode));
            }

            if (writeBinary)
            {
                using var glb = new FileStream($"{outputName}.glb", FileMode.Create, FileAccess.Write);
                _gltf.CompileToBinary(glb);
                Utilities.Log(LogLevelEnum.Info, "Created \"{0}.glb\" file.", outputName);
            }

            if (writeText)
            {
                using var gltf = new FileStream($"{outputName}.gltf", FileMode.Create, FileAccess.Write);
                using var bin = new FileStream($"{outputName}.bin", FileMode.Create, FileAccess.Write);
                _gltf.CompileToPair($"{outputName}.bin", gltf, bin);
                Utilities.Log(LogLevelEnum.Info, "Created \"{0}.gltf\" and .bin files.", outputName);
            }

            return 1;
        }

        foreach (var layer in terrain.RootLayer.Sublayers)
        {
            _gltf.Clear();
            _materialMap.Clear();

            _gltf.Add(new GltfScene {Name = Path.GetFileName(terrain.BasePath) + ":" + layer.Name});

            if (CreateNodeForLayerRecursive(out var layerNode, terrain, layer))
            {
                layerNode.Name = "Layers";
                layerNode.Translation = baseTranslationList;
                _gltf.CurrentScene.Nodes.Add(_gltf.Add(layerNode));
            }

            var layers = layer.RecursiveSublayers.Select(x => x.Name).ToHashSet();
            if (CreateNodeForEntityRecursive(out var entityNode, terrain, terrain.RootEntity, x => layers.Contains(x)))
            {
                entityNode.Name = "Entities";
                entityNode.Translation = baseTranslationList;
                _gltf.CurrentScene.Nodes.Add(_gltf.Add(entityNode));
            }

            var layerBaseName = outputName;
            if (!string.IsNullOrWhiteSpace(layer.Name))
                layerBaseName += "." + Regex.Replace(layer.Name, "[<>:\\\\\"/\\|\\?\\*]", "_");

            if (writeBinary)
            {
                using var glb = new FileStream($"{layerBaseName}.glb", FileMode.Create, FileAccess.Write);
                _gltf.CompileToBinary(glb);
                Utilities.Log(LogLevelEnum.Info, "Created \"{0}.glb\" file.", layerBaseName);
            }

            if (writeText)
            {
                using var gltf = new FileStream($"{layerBaseName}.gltf", FileMode.Create, FileAccess.Write);
                using var bin = new FileStream($"{layerBaseName}.bin", FileMode.Create, FileAccess.Write);
                _gltf.CompileToPair($"{layerBaseName}.bin", gltf, bin);
                Utilities.Log(LogLevelEnum.Info, "Created \"{0}.gltf\" and .bin files.", layerBaseName);
            }

            numOutputSet++;
        }

        return numOutputSet;
    }
}