using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Gltf.Models;

namespace CgfConverter.Renderers.Gltf;

public partial class GltfRenderer
{
    private int? WriteSkin(GltfNode rootNode, ChunkNode nodeChunk,
        GltfMeshPrimitiveAttributes primitiveAccessors)
    {
        var skinningInfo = nodeChunk.GetSkinningInfo();
        if (!skinningInfo.HasSkinningInfo)
            return null;
        primitiveAccessors.Weights0 = _gltf.AddAccessor($"{nodeChunk.Name}/bone/weight", -1,
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

            _controllerIdToNodeIndex[bone.ControllerID] = _gltf.Add(new GltfNode
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
                rootNode.Children.Add(_controllerIdToNodeIndex[bone.ControllerID]);
            else
                _gltf.Nodes[_controllerIdToNodeIndex[bone.parentID]].Children
                    .Add(_controllerIdToNodeIndex[bone.ControllerID]);
        }

        primitiveAccessors.Joints0 = _gltf.AddAccessor($"{nodeChunk.Name}/bone/joint", -1,
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

        var inverseBindMatricesAccessor = _gltf.AddAccessor($"{nodeChunk.Name}/inverseBindMatrix", -1,
            skinningInfo.CompiledBones.Select(x => SwapAxes(Matrix4x4.Transpose(x.BindPoseMatrix))).ToArray());

        return _gltf.Add(new GltfSkin
        {
            InverseBindMatrices = inverseBindMatricesAccessor,
            Joints = skinningInfo.CompiledBones.Select(x => _controllerIdToNodeIndex[x.ControllerID]).ToList(),
            Name = $"{nodeChunk.Name}/skin",
        });
    }

    private int? WriteMesh(ChunkNode nodeChunk, ChunkMesh meshChunk,
        GltfMeshPrimitiveAttributes primitiveAccessors)
    {
        var materialMap = WriteMaterial(nodeChunk);
        
        var vertices = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.VerticesData) as ChunkDataStream;
        var vertsUvs = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.VertsUVsData) as ChunkDataStream;
        var normals = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.NormalsData) as ChunkDataStream;
        var uvs = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.UVsData) as ChunkDataStream;
        var indices = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.IndicesData) as ChunkDataStream;
        // var colors = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.ColorsData) as ChunkDataStream;
        var tangents = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.TangentsData) as ChunkDataStream;
        var subsets = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.MeshSubsetsData) as ChunkMeshSubsets;

        if (indices is null || subsets is null || (vertices is null && vertsUvs is null))
            return null;

        if (vertices is not null)
        {
            primitiveAccessors.Position =
                _gltf.AddAccessor($"{nodeChunk.Name}/vertex", -1,
                    vertices.Vertices.Select(SwapAxes).ToArray());

            // TODO: Is this correct? This breaks some of RoL model colors, while having it set does not make anything better.
            // primitiveAccessors.Color0 = colors is null
            //     ? null
            //     : _gltf.AddAccessor($"{nodeChunk.Name}/colors", -1,
            //         colors.Colors.Select(x => new TypedVec4<float>(x.r / 255f, x.g / 255f, x.b / 255f, x.a / 255f))
            //             .ToArray());

            var normalsArray = normals?.Normals ?? tangents?.Normals;
            primitiveAccessors.Normal = normalsArray is null
                ? null
                : _gltf.AddAccessor($"{nodeChunk.Name}/normal", -1,
                    normalsArray.Select(SwapAxes).ToArray());

            // TODO: Do Tangents also need swapping axes?
            primitiveAccessors.Tangent = tangents is null
                ? null
                : _gltf.AddAccessor($"{nodeChunk.Name}/tangent", -1,
                    tangents.Tangents.Cast<Tangent>()
                        .Where((_, i) => i % 2 == 0)
                        .Select(x => new TypedVec4<float>(x.x / 32767f, x.y / 32767f, x.z / 32767f, x.w / 32767f))
                        .ToArray());

            primitiveAccessors.TexCoord0 =
                uvs is null ? null : _gltf.AddAccessor($"{nodeChunk.Name}/uv", -1, uvs.UVs);
        }

        if (vertsUvs is not null && vertices is null)
            throw new NotSupportedException();

        var indexBufferView = _gltf.AddBufferView($"{nodeChunk.Name}/index", indices.Indices);
        return _gltf.Add(new GltfMesh
        {
            Primitives = subsets.MeshSubsets.Select(v => new GltfMeshPrimitive
            {
                Attributes = primitiveAccessors,
                Indices = _gltf.AddAccessor(
                    $"{nodeChunk.Name}/index",
                    indexBufferView,
                    indices.Indices, v.FirstIndex, v.FirstIndex + v.NumIndices),
                Material = materialMap.GetValueOrDefault(v.MatID),
            }).ToList()
        });
    }

    private void WriteGeometries(Model model)
    {
        var hasAnyMesh = false;
        foreach (var nodeChunk in model.ChunkMap.Values.OfType<ChunkNode>())
        {
            if (IsNodeNameExcluded(nodeChunk.Name))
            {
                Utilities.Log(LogLevelEnum.Debug, $"Excluding node {nodeChunk.Name}");
                continue;
            }

            if (nodeChunk.ObjectChunk is null)
            {
                Utilities.Log(LogLevelEnum.Warning, "Skipped node with missing Object {0}", nodeChunk.Name);
                continue;
            }

            if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID] is not ChunkMesh meshChunk)
                continue;

            var rootNode = new GltfNode
            {
                Name = nodeChunk.Name,
            };

            var primitiveAccessors = new GltfMeshPrimitiveAttributes();
            rootNode.Mesh = WriteMesh(nodeChunk, meshChunk, primitiveAccessors);
            if (rootNode.Mesh is null)
                continue;
            
            rootNode.Skin = WriteSkin(rootNode, nodeChunk, primitiveAccessors);
            
            _gltf.Scenes[_gltf.Scene].Nodes.Add(_gltf.Add(rootNode));
            hasAnyMesh = true;
        }

        if (!hasAnyMesh)
            throw new NotSupportedException();
    }
}