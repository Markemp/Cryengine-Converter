﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using CgfConverter.PackFileSystem;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Terrain;
using CgfConverter.Terrain.Xml;

namespace CgfConverter.Renderers.Gltf;

public partial class GltfRendererCommon
{
    private readonly IPackFileSystem _packFileSystem;
    private readonly GltfWriter _gltf;
    private readonly List<Regex> _excludedNodeNames;

    public GltfRendererCommon(IPackFileSystem packFileSystem, List<Regex> excludedNodeNames)
    {
        _packFileSystem = packFileSystem;
        _gltf = new GltfWriter();
        _excludedNodeNames = excludedNodeNames;
    }

    public void RenderSingleModel(CryEngine cryData, FileInfo? glbOutputFile, FileInfo? gltfOutputFile,
        FileInfo? gltfBinOutputFile)
    {
        _gltf.Add(new GltfScene
        {
            Name = "Scene",
        });

        if (WriteModel(cryData) is { } node)
            _gltf.Scenes[_gltf.Scene].Nodes.Add(node);
        else
            throw new NotSupportedException();

        if (glbOutputFile is not null)
        {
            using var glb = glbOutputFile.Open(FileMode.Create, FileAccess.Write);
            _gltf.CompileToBinary(glb);
        }

        if (gltfOutputFile is not null && gltfBinOutputFile is not null)
        {
            using var gltf = gltfOutputFile.Open(FileMode.Create, FileAccess.Write);
            using var bin = gltfBinOutputFile.Open(FileMode.Create, FileAccess.Write);
            _gltf.CompileToPair(gltfBinOutputFile.Name, gltf, bin);
        }
    }

    public void RenderSingleTerrain(CryTerrain terrain, FileInfo? glbOutputFile, FileInfo? gltfOutputFile,
        FileInfo? gltfBinOutputFile)
    {
        _gltf.Add(new GltfScene
        {
            Name = "Scene",
        });

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
                allLayers[layer.Parent].Add(layer.Name!);
            }
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

        var layerStack = rootLayers.Keys.Select(x => Tuple.Create(rootNode, x)).ToList();
        for (var i = 0; layerStack.Any(); i++)
        {
            var (parentNode, layerName) = layerStack.Last();
            layerStack.RemoveAt(layerStack.Count - 1);

            Utilities.Log(LogLevelEnum.Info, "Current layer ({0}/{1}): {2}", i + 1, allLayers.Count, layerName);

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

            var entityNode = -1;
            var entityStack = rootEntities.ToList();
            while (entityStack.Any())
            {
                var entity = entityStack.Last();
                entityStack.RemoveAt(entityStack.Count - 1);

                var node = -1;

                if (entity.Layer == layerName && entity.Geometry is not null && entity.EntityClass == "AnimObject")
                {
                    var name = entity.Geometry!
                        .Replace("%level%", terrain.BasePath)
                        .ToLowerInvariant();

                    do
                    {
                        if (_excludedNodeNames.Any(y => y.IsMatch(name)))
                            break;

                        if (!terrain.Objects.TryGetValue(name, out var cryObject))
                            break;

                        var translation = SwapAxesForPosition(entity.PosValue ?? Vector3.Zero);
                        var rotation = SwapAxesForLayout(entity.RotateValue ?? Quaternion.Identity);
                        var scale = SwapAxesForScale(entity.ScaleValue ?? Vector3.One) * 4;

                        if (WriteModel(cryObject, translation, rotation, scale, true) is { } node2)
                            node = node2;
                    } while (false);
                }

                if (node == -1)
                {
                    if (!childEntities[entity.EntityIdValue!.Value].Any())
                        continue;
                    node = _gltf.Add(new GltfNode
                    {
                        Name = entity.Name,
                    });
                }
                else
                {
                    _gltf.Nodes[node].Name = entity.Name;
                }

                if (entityNode == -1)
                    _gltf.Nodes[layerNode].Children.Add(entityNode = _gltf.Add(new GltfNode
                    {
                        Name = "Entities"
                    }));

                _gltf.Nodes[entityNode].Children.Add(node);
                entityStack.AddRange(childEntities[entity.EntityIdValue!.Value]
                    .Select(x => allEntities[x]));
            }
        }

        if (glbOutputFile is not null)
        {
            using var glb = glbOutputFile.Open(FileMode.Create, FileAccess.Write);
            _gltf.CompileToBinary(glb);
        }

        if (gltfOutputFile is not null && gltfBinOutputFile is not null)
        {
            using var gltf = gltfOutputFile.Open(FileMode.Create, FileAccess.Write);
            using var bin = gltfBinOutputFile.Open(FileMode.Create, FileAccess.Write);
            _gltf.CompileToPair(gltfBinOutputFile.Name, gltf, bin);
        }
    }
}