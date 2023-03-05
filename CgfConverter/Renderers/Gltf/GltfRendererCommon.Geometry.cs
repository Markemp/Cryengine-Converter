using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Gltf.Models;

namespace CgfConverter.Renderers.Gltf;

public partial class GltfRendererCommon
{
    private int? WriteSkin(
        string fileName,
        GltfNode rootNode,
        ChunkNode nodeChunk,
        GltfMeshPrimitiveAttributes primitiveAccessors,
        IDictionary<uint, int> controllerIdToNodeIndex)
    {
        var skinningInfo = nodeChunk.GetSkinningInfo();
        if (!skinningInfo.HasSkinningInfo)
            return null;

        var baseName = $"{fileName}/{nodeChunk.Name}/bone/weight";
        primitiveAccessors.Weights0 =
            _gltf.GetAccessorOrDefault(baseName, 0,
                skinningInfo.IntVertices == null ? skinningInfo.BoneMapping.Count : skinningInfo.Ext2IntMap.Count)
            ?? _gltf.AddAccessor(baseName, -1, null,
                skinningInfo.IntVertices == null
                    ? skinningInfo.BoneMapping
                        .Select(x => new TypedVec4<float>(
                            x.Weight[0] / 255f, x.Weight[1] / 255f, x.Weight[2] / 255f, x.Weight[3] / 255f))
                        .ToArray()
                    : skinningInfo.Ext2IntMap
                        .Select(x => skinningInfo.IntVertices[x])
                        .Select(x => new TypedVec4<float>(
                            x.Weights[0], x.Weights[1], x.Weights[2], x.Weights[3]))
                        .ToArray());

        var boneIdToBindPoseMatrices = new Dictionary<uint, Matrix4x4>();
        foreach (var bone in skinningInfo.CompiledBones)
        {
            var mat = boneIdToBindPoseMatrices[bone.ControllerID] = bone.BindPoseMatrix;
            if (bone.parentID != 0)
            {
                if (!Matrix4x4.Invert(boneIdToBindPoseMatrices[bone.parentID], out var pm4x4))
                    throw new Exception();
                mat *= pm4x4;
            }

            if (!Matrix4x4.Invert(mat, out mat))
                throw new Exception();

            mat = Matrix4x4.Transpose(mat);
            mat = SwapAxes(mat);

            if (!Matrix4x4.Decompose(mat, out var scale, out var rotation, out var translation))
                throw new Exception();

            controllerIdToNodeIndex[bone.ControllerID] = _gltf.Add(new GltfNode
            {
                Name = bone.boneName,
                Scale = (scale - Vector3.One).LengthSquared() > 0.000001
                    ? new List<float> {scale.X, scale.Y, scale.Z}
                    : null,
                Translation = translation != Vector3.Zero
                    ? new List<float> {translation.X, translation.Y, translation.Z}
                    : null,
                Rotation = rotation != Quaternion.Identity
                    ? new List<float> {rotation.X, rotation.Y, rotation.Z, rotation.W}
                    : null,
            });

            if (bone.parentID == 0)
                rootNode.Children.Add(controllerIdToNodeIndex[bone.ControllerID]);
            else
                _gltf.Nodes[controllerIdToNodeIndex[bone.parentID]].Children
                    .Add(controllerIdToNodeIndex[bone.ControllerID]);
        }

        baseName = $"{fileName}/{nodeChunk.Name}/bone/joint";
        primitiveAccessors.Joints0 =
            _gltf.GetAccessorOrDefault(baseName, 0,
                skinningInfo.IntVertices == null ? skinningInfo.BoneMapping.Count : skinningInfo.Ext2IntMap.Count)
            ?? _gltf.AddAccessor(baseName, -1, null,
                skinningInfo is {HasIntToExtMapping: true, IntVertices: { }}
                    ? skinningInfo.Ext2IntMap
                        .Select(x => skinningInfo.IntVertices[x])
                        .Select(x => new TypedVec4<ushort>(
                            x.BoneIDs[0], x.BoneIDs[1], x.BoneIDs[2], x.BoneIDs[3]))
                        .ToArray()
                    : skinningInfo.BoneMapping
                        .Select(x => new TypedVec4<ushort>(
                            (ushort) x.BoneIndex[0], (ushort) x.BoneIndex[1], (ushort) x.BoneIndex[2],
                            (ushort) x.BoneIndex[3]))
                        .ToArray());

        baseName = $"{fileName}/{nodeChunk.Name}/inverseBindMatrix";
        var inverseBindMatricesAccessor =
            _gltf.GetAccessorOrDefault(baseName, 0, skinningInfo.CompiledBones.Count)
            ?? _gltf.AddAccessor(baseName, -1, null,
                skinningInfo.CompiledBones.Select(x => SwapAxes(Matrix4x4.Transpose(x.BindPoseMatrix))).ToArray());

        return _gltf.Add(new GltfSkin
        {
            InverseBindMatrices = inverseBindMatricesAccessor,
            Joints = skinningInfo.CompiledBones.Select(x => controllerIdToNodeIndex[x.ControllerID]).ToList(),
            Name = $"{nodeChunk.Name}/skin",
        });
    }

    private int? WriteMesh(string fileName, ChunkNode node, ChunkMesh mesh, GltfMeshPrimitiveAttributes accessors)
    {
        var vertices = node._model.ChunkMap.GetValueOrDefault(mesh.VerticesData) as ChunkDataStream;
        var vertsUvs = node._model.ChunkMap.GetValueOrDefault(mesh.VertsUVsData) as ChunkDataStream;
        var normals = node._model.ChunkMap.GetValueOrDefault(mesh.NormalsData) as ChunkDataStream;
        var uvs = node._model.ChunkMap.GetValueOrDefault(mesh.UVsData) as ChunkDataStream;
        var indices = node._model.ChunkMap.GetValueOrDefault(mesh.IndicesData) as ChunkDataStream;
        // var colors = node._model.ChunkMap.GetValueOrDefault(mesh.ColorsData) as ChunkDataStream;
        var tangents = node._model.ChunkMap.GetValueOrDefault(mesh.TangentsData) as ChunkDataStream;
        var subsets = node._model.ChunkMap.GetValueOrDefault(mesh.MeshSubsetsData) as ChunkMeshSubsets;

        if (indices is null || subsets is null || (vertices is null && vertsUvs is null))
            return null;

        var materialMap = WriteMaterial(node);

        var usesTangent = subsets.MeshSubsets.Any(v =>
            materialMap.GetValueOrDefault(v.MatID) is { } matIndex && _gltf.Materials[matIndex].HasNormalTexture());

        var usesUv = usesTangent || subsets.MeshSubsets.Any(v =>
            materialMap.GetValueOrDefault(v.MatID) is { } matIndex && _gltf.Materials[matIndex].HasAnyTexture());

        string baseName;
        if (vertices is not null)
        {
            baseName = $"{fileName}/{node.Name}/vertex";
            accessors.Position =
                _gltf.GetAccessorOrDefault(baseName, 0, vertices.Vertices.Length)
                ?? _gltf.AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                    vertices.Vertices.Select(SwapAxesForPosition).ToArray());

            // TODO: Is this correct? This breaks some of RoL model colors, while having it set does not make anything better.
            // baseName = $"{fileName}/{nodeChunk.Name}/colors";
            // primitiveAccessors.Color0 = colors is null
            //     ? null
            //     : (_gltf.GetAccessorOrDefault(baseName, 0, colors.Colors.Length)
            //        ?? _gltf.AddAccessor(baseName, -1,
            //            colors.Colors.Select(x => new TypedVec4<float>(x.r / 255f, x.g / 255f, x.b / 255f, x.a / 255f))
            //                .ToArray()));

            var normalsArray = normals?.Normals ?? tangents?.Normals;
            baseName = $"{fileName}/{node.Name}/normal";
            accessors.Normal = normalsArray is null
                ? null
                : _gltf.GetAccessorOrDefault(baseName, 0, normalsArray.Length)
                  ?? _gltf.AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                      normalsArray.Select(SwapAxesForPosition).ToArray());

            // TODO: Do Tangents also need swapping axes?
            baseName = $"${fileName}/{node.Name}/tangent";
            accessors.Tangent = tangents is null || !usesTangent
                ? null
                : _gltf.GetAccessorOrDefault(baseName, 0, tangents.Tangents.Length / 2)
                  ?? _gltf.AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                      tangents.Tangents.Cast<Tangent>()
                          .Where((_, i) => i % 2 == 0)
                          .Select(x => new TypedVec4<float>(x.x / 32767f, x.y / 32767f, x.z / 32767f, x.w / 32767f))
                          .ToArray());

            baseName = $"${fileName}/{node.Name}/uv";
            accessors.TexCoord0 =
                uvs is null || !usesUv
                    ? null
                    : _gltf.GetAccessorOrDefault(baseName, 0, uvs.UVs.Length)
                      ?? _gltf.AddAccessor($"{node.Name}/uv", -1, GltfBufferViewTarget.ArrayBuffer, uvs.UVs);
        }

        if (vertsUvs is not null && vertices is null)
            throw new NotSupportedException();

        baseName = $"${fileName}/{node.Name}/index";
        var indexBufferView = _gltf.GetBufferViewOrDefault(baseName) ??
                              _gltf.AddBufferView(baseName, indices.Indices, GltfBufferViewTarget.ElementArrayBuffer);
        return _gltf.Add(new GltfMesh
        {
            Primitives = subsets.MeshSubsets.Select(v =>
            {
                var matIndex = materialMap.GetValueOrDefault(v.MatID);

                return new GltfMeshPrimitive
                {
                    Attributes = new GltfMeshPrimitiveAttributes
                    {
                        Position = accessors.Position,
                        Normal = accessors.Normal,
                        Tangent = matIndex is null || !_gltf.Materials[matIndex.Value].HasNormalTexture()
                            ? null
                            : accessors.Tangent,
                        TexCoord0 = matIndex is null || !_gltf.Materials[matIndex.Value].HasAnyTexture()
                            ? null
                            : accessors.TexCoord0,
                        Color0 = accessors.Color0,
                        Joints0 = accessors.Joints0,
                        Weights0 = accessors.Weights0,
                    },
                    Indices = _gltf.GetAccessorOrDefault(baseName, v.FirstIndex, v.FirstIndex + v.NumIndices)
                              ?? _gltf.AddAccessor(
                                  $"{node.Name}/index",
                                  indexBufferView, GltfBufferViewTarget.ElementArrayBuffer,
                                  indices.Indices, v.FirstIndex, v.FirstIndex + v.NumIndices),
                    Material = matIndex,
                };
            }).ToList()
        });
    }

    private int? WriteModel(CryEngine cryObject) =>
        WriteModel(cryObject, Vector3.Zero, Quaternion.Identity, Vector3.One, false);

    private int? WriteModel(CryEngine cryObject, Vector3 translation, Quaternion rotation, Vector3 scale,
        bool omitSkins)
    {
        var model = cryObject.Models[^1];
        var controllerIdToNodeIndex = new Dictionary<uint, int>();

        var meshNodes = new List<int>();
        foreach (var nodeChunk in model.ChunkMap.Values.OfType<ChunkNode>())
        {
            // TODO
            // if (IsNodeNameExcluded(nodeChunk.Name))
            // {
            //     Utilities.Log(LogLevelEnum.Debug, $"Excluding node {nodeChunk.Name}");
            //     continue;
            // }

            if (nodeChunk.ObjectChunk is null)
            {
                Utilities.Log(LogLevelEnum.Warning, "Skipped node with missing Object {0}", nodeChunk.Name);
                continue;
            }

            if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID] is not ChunkMesh meshChunk)
                continue;

            var rootNode = new GltfNode
            {
                Name = Path.GetFileNameWithoutExtension(model.FileName!) + "/" + nodeChunk.Name,
            };

            var primitiveAccessors = new GltfMeshPrimitiveAttributes();

            rootNode.Mesh = WriteMesh(model.FileName!, nodeChunk, meshChunk, primitiveAccessors);
            if (rootNode.Mesh is not null)
                _gltf.Meshes[rootNode.Mesh.Value].Name = rootNode.Name + "/mesh";
            else
                continue;

            if (!omitSkins)
            {
                rootNode.Skin = WriteSkin(model.FileName!, rootNode, nodeChunk, primitiveAccessors,
                    controllerIdToNodeIndex);
                if (rootNode.Skin is not null)
                    _gltf.Skins[rootNode.Skin.Value].Name = rootNode.Name + "/skin";
            }

            meshNodes.Add(_gltf.Add(rootNode));
        }

        if (!meshNodes.Any())
            return null;

        if (!omitSkins)
            WriteAnimations(cryObject.Animations, controllerIdToNodeIndex);

        return _gltf.Add(new GltfNode
        {
            Name = model.FileName,
            Children = meshNodes,
            Translation = translation == Vector3.Zero
                ? null
                : new List<float> {translation.X, translation.Y, translation.Z},
            Rotation = rotation == Quaternion.Identity
                ? null
                : new List<float> {rotation.X, rotation.Y, rotation.Z, rotation.W},
            Scale = scale == Vector3.One
                ? null
                : new List<float> {scale.X, scale.Y, scale.Z},
        });
    }
}