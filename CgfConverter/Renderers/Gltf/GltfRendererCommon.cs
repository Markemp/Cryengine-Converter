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
using CgfConverter.Terrain.Xml;

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

        if (WriteModel(cryData) is { } node)
            _gltf.Scenes[_gltf.Scene].Nodes.Add(node);
        else
            throw new NotSupportedException();

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

    public void RenderSingleTerrain(CryTerrain terrain, string outputName, bool writeBinary, bool writeText, bool separateLayers)
    {
        var allLayers = new Dictionary<string, List<string>>();
        var layerIds = new Dictionary<string, int>();
        var rootLayers = new Dictionary<string, List<string>>();
        foreach (var layer in terrain.LevelData.Layers!)
        {
            if (layer.IdValue is { } id)
                layerIds[layer.Name!] = id;
            if (string.IsNullOrEmpty(layer.Parent))
            {
                rootLayers[layer.Name!] = allLayers[layer.Name!] = new List<string>();
            }
            else
            {
                allLayers[layer.Name!] = new List<string>();
                if (allLayers.TryGetValue(layer.Parent, out var dict))
                    dict.Add(layer.Name!);
                else
                    allLayers[layer.Parent] = new List<string> {layer.Name!};
            }
        }

        if (!separateLayers)
        {
            rootLayers = new Dictionary<string, List<string>>
            {
                [""] = allLayers[""] = rootLayers.Keys.ToList(),
            };
        }

        var rootEntities = new HashSet<ObjectOrEntity>();
        var allEntities = new Dictionary<int, ObjectOrEntity>();
        var childEntities = new Dictionary<int, List<int>>();
        foreach (var entity in terrain.Missions
                     .SelectMany(x => x.ObjectsAndEntities!))
        {
            if (entity.EntityIdValue is null)
                continue;
            allEntities[entity.EntityIdValue!.Value] = entity;
            if (!childEntities.ContainsKey(entity.EntityIdValue!.Value))
                childEntities[entity.EntityIdValue!.Value] = new List<int>();
            if (entity.ParentIdValue is { } parentId)
            {
                if (!childEntities.ContainsKey(parentId))
                    childEntities[parentId] = new List<int> {entity.EntityIdValue!.Value};
                else
                    childEntities[parentId].Add(entity.EntityIdValue!.Value);
            }
            else
            {
                rootEntities.Add(entity);
            }
        }

        var childEntityNodes = new List<int>();
        var numProcessedLayers = 0;
        foreach (var rootLayer in rootLayers)
        {
            _gltf.Clear();
            _materialMap.Clear();
            _gltf.Add(new GltfScene
            {
                Name = "Scene",
            });

            var rootNode = _gltf.Add(new GltfNode
            {
                Name = "Terrain",
                Translation = new List<float>
                {
                    (terrain.TerrainFile.OcTreeNode.NodeBox.Max.X - terrain.TerrainFile.OcTreeNode.NodeBox.Min.X) / 2,
                    0,
                    (terrain.TerrainFile.OcTreeNode.NodeBox.Max.Z - terrain.TerrainFile.OcTreeNode.NodeBox.Min.Z) / -2,
                },
            });

            _gltf.Scenes[0].Nodes.Add(rootNode);

            var layerStack = new List<Tuple<int, string>> {Tuple.Create(rootNode, rootLayer.Key)};
            for (; layerStack.Any(); numProcessedLayers++)
            {
                var (parentNode, layerName) = layerStack.Last();
                layerStack.RemoveAt(layerStack.Count - 1);

                Utilities.Log(LogLevelEnum.Info, "Current layer ({0}/{1}): {2}", numProcessedLayers + 1,
                    allLayers.Count, layerName);

                var layerNode = _gltf.Add(new GltfNode
                {
                    Name = layerName,
                });

                _gltf.Nodes[parentNode].Children.Add(layerNode);

                layerStack.AddRange(allLayers[layerName].Select(x => Tuple.Create(layerNode, x)));

                if (layerIds.TryGetValue(layerName, out var layerId))
                {
                    var staticNodeContainer = -1;
                    foreach (var renderNode in terrain.AllRenderNodes.Where(x => x.LayerId == layerId))
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
                            .ToLowerInvariant();
                        if (_excludedNodeNames.Any(y => y.IsMatch(name)))
                            continue;

                        if (!terrain.Objects.TryGetValue(name, out var cryObject))
                            continue;

                        translation = SwapAxesForPosition(translation);
                        rotation = SwapAxesForAnimations(rotation);
                        scale = SwapAxesForScale(scale);

                        if (WriteModel(cryObject, translation, rotation, scale, true) is not { } node)
                            continue;

                        _gltf.Nodes[node].Name = Path.GetFileNameWithoutExtension(_gltf.Nodes[node].Name);
                        if (staticNodeContainer == -1)
                        {
                            _gltf.Nodes[parentNode].Children.Add(staticNodeContainer = _gltf.Add(new GltfNode
                            {
                                Name = "Static",
                            }));
                        }

                        _gltf.Nodes[staticNodeContainer].Children.Add(node);
                    }
                }

                var entityNode = _gltf.Add(new GltfNode
                {
                    Name = "Entities"
                });
                _gltf.Nodes[layerNode].Children.Add(entityNode);

                var entityStack = rootEntities.ToList();
                while (entityStack.Any())
                {
                    var entity = entityStack.Last();
                    entityStack.RemoveAt(entityStack.Count - 1);
                    entityStack.AddRange(childEntities[entity.EntityIdValue!.Value]
                        .Select(x => allEntities[x]));

                    if (entity.Layer != layerName)
                        continue;

                    foreach (var name in entity.AllAttachedModelPaths)
                    {
                        if (_excludedNodeNames.Any(y => y.IsMatch(name)))
                            continue;

                        if (!terrain.Objects.TryGetValue(name, out var cryObject))
                            continue;

                        var translation = SwapAxesForPosition(entity.PosValue ?? Vector3.Zero);
                        var rotation = SwapAxesForLayout(entity.RotateValue ?? Quaternion.Identity);
                        var scale = SwapAxesForScale(entity.ScaleValue ?? Vector3.One);

                        if (WriteModel(cryObject, translation, rotation, scale, true) is { } node2)
                            childEntityNodes.Add(node2);
                    }

                    switch (childEntityNodes.Count)
                    {
                        case 0:
                            break;
                        case 1:
                            _gltf.Nodes[entityNode].Children.Add(childEntityNodes.First());
                            childEntityNodes.Clear();
                            break;
                        default:
                            _gltf.Nodes[entityNode].Children.Add(_gltf.Add(new GltfNode
                            {
                                Name = $"{entity.Name}:{entity.EntityClass}@{layerName}",
                                Children = childEntityNodes,
                            }));
                            
                            childEntityNodes = new List<int>();
                            break;
                    }
                }
            }

            var layerBaseName = outputName;
            if (!string.IsNullOrWhiteSpace(rootLayer.Key))
                layerBaseName += "." + Regex.Replace(rootLayer.Key, "[<>:\\\\\"/\\|\\?\\*]", "_");
            
            if (writeBinary)
            {
                using var glb = new FileStream($"{layerBaseName}.glb", FileMode.Create, FileAccess.Write);
                _gltf.CompileToBinary(glb);
                Utilities.Log(LogLevelEnum.Info, "Created \"{0}.glb\" file.", layerBaseName);
            }

            if (writeText)
            {
                using var gltf = new FileStream($"{layerBaseName}.gltf", FileMode.Create,
                    FileAccess.Write);
                using var bin = new FileStream($"{layerBaseName}.bin", FileMode.Create, FileAccess.Write);
                _gltf.CompileToPair($"{outputName}.{rootLayer.Key}.bin", gltf, bin);
                Utilities.Log(LogLevelEnum.Info, "Created \"{0}.gltf\" and .bin files.", layerBaseName);
            }
        }
    }
}