using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Gltf.Models;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    private bool WriteSkinOrLogError(
        out GltfSkin newSkin,
        out int weights,
        out int joints,
        GltfNode rootNode,
        SkinningInfo skinningInfo,
        IDictionary<uint, int> controllerIdToNodeIndex)
    {
        if (!skinningInfo.HasSkinningInfo)
            throw new ArgumentException("HasSkinningInfo must be true", nameof(skinningInfo));

        newSkin = null!;
        weights = joints = 0;

        var baseName = $"{rootNode.Name}/bone/weight";
        weights =
            GetAccessorOrDefault(baseName, 0,
                skinningInfo.IntVertices == null ? skinningInfo.BoneMapping.Count : skinningInfo.Ext2IntMap.Count)
            ?? AddAccessor(baseName, -1, null,
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
                if (!Matrix4x4.Invert(boneIdToBindPoseMatrices[bone.parentID], out var parentMat))
                    return Log.E<bool>("CompiledBone[{0}/{1}]: Failed to invert BindPoseMatrix.",
                        rootNode.Name, bone.ParentBone?.boneName);

                mat *= parentMat;
            }

            if (!Matrix4x4.Invert(mat, out mat))
                return Log.E<bool>("CompiledBone[{0}/{1}]: Failed to invert BindPoseMatrix.",
                    rootNode.Name, bone.boneName);

            mat = SwapAxes(Matrix4x4.Transpose(mat));
            if (!Matrix4x4.Decompose(mat, out var scale, out var rotation, out var translation))
                return Log.E<bool>("CompiledBone[{0}/{1}]: BindPoseMatrix is not decomposable.",
                    rootNode.Name, bone.boneName);

            controllerIdToNodeIndex[bone.ControllerID] = AddNode(new GltfNode
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
                _root.Nodes[controllerIdToNodeIndex[bone.parentID]].Children
                    .Add(controllerIdToNodeIndex[bone.ControllerID]);
        }

        baseName = $"{rootNode.Name}/bone/joint";
        joints =
            GetAccessorOrDefault(baseName, 0,
                skinningInfo.IntVertices == null ? skinningInfo.BoneMapping.Count : skinningInfo.Ext2IntMap.Count)
            ?? AddAccessor(baseName, -1, null,
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

        baseName = $"{rootNode.Name}/inverseBindMatrix";
        var inverseBindMatricesAccessor =
            GetAccessorOrDefault(baseName, 0, skinningInfo.CompiledBones.Count)
            ?? AddAccessor(baseName, -1, null,
                skinningInfo.CompiledBones.Select(x => SwapAxes(Matrix4x4.Transpose(x.BindPoseMatrix))).ToArray());

        newSkin = new GltfSkin
        {
            InverseBindMatrices = inverseBindMatricesAccessor,
            Joints = skinningInfo.CompiledBones.Select(x => controllerIdToNodeIndex[x.ControllerID]).ToList(),
            Name = $"{rootNode.Name}/skin",
        };
        return true;
    }

    private bool WriteMeshOrLogError(out GltfMesh newMesh, GltfNode rootNode, ChunkNode node, ChunkMesh mesh,
        GltfMeshPrimitiveAttributes accessors)
    {
        newMesh = null!;

        var vertices = node._model.ChunkMap.GetValueOrDefault(mesh.VerticesData) as ChunkDataStream;
        var vertsUvs = node._model.ChunkMap.GetValueOrDefault(mesh.VertsUVsData) as ChunkDataStream;
        var normals = node._model.ChunkMap.GetValueOrDefault(mesh.NormalsData) as ChunkDataStream;
        var uvs = node._model.ChunkMap.GetValueOrDefault(mesh.UVsData) as ChunkDataStream;
        var indices = node._model.ChunkMap.GetValueOrDefault(mesh.IndicesData) as ChunkDataStream;
        // var colors = node._model.ChunkMap.GetValueOrDefault(mesh.ColorsData) as ChunkDataStream;
        var tangents = node._model.ChunkMap.GetValueOrDefault(mesh.TangentsData) as ChunkDataStream;
        var subsets = node._model.ChunkMap.GetValueOrDefault(mesh.MeshSubsetsData) as ChunkMeshSubsets;

        if (indices is null)
            return Log.D<bool>("Mesh[{0}]: IndicesDat is empty.", rootNode.Name);
        if (subsets is null)
            return Log.D<bool>("Mesh[{0}]: MeshSubsetsData is empty.", rootNode.Name);
        if (vertices is null && vertsUvs is null)
            return Log.D<bool>("Mesh[{0}]: both VerticesData and VertsUVsData are empty.", rootNode.Name);

        var materialMap = WriteMaterial(node);
        if (subsets.MeshSubsets
                .Select(x => materialMap.GetValueOrDefault(x.MatID))
                .All(x => x?.IsSkippedFromArgs ?? false))
            return false;

        var usesTangent = subsets.MeshSubsets.Any(v =>
            materialMap.GetValueOrDefault(v.MatID) is { } m && m.Target?.HasNormalTexture() is true);

        var usesUv = usesTangent || subsets.MeshSubsets.Any(v =>
            materialMap.GetValueOrDefault(v.MatID) is { } m && m.Target?.HasAnyTexture() is true);

        string baseName;
        if (vertices is not null)
        {
            baseName = $"{rootNode.Name}/vertex";
            accessors.Position =
                GetAccessorOrDefault(baseName, 0, vertices.Vertices.Length)
                ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                    vertices.Vertices.Select(SwapAxesForPosition).ToArray());

            // TODO: Is this correct? This breaks some of RoL model colors, while having it set does not make anything better.
            // baseName = $"{rootNode.Name}/colors";
            // primitiveAccessors.Color0 = colors is null
            //     ? null
            //     : (_gltf.GetAccessorOrDefault(baseName, 0, colors.Colors.Length)
            //        ?? _AddAccessor(baseName, -1,
            //            colors.Colors.Select(x => new TypedVec4<float>(x.r / 255f, x.g / 255f, x.b / 255f, x.a / 255f))
            //                .ToArray()));

            var normalsArray = normals?.Normals ?? tangents?.Normals;
            baseName = $"{rootNode.Name}/normal";
            accessors.Normal = normalsArray is null
                ? null
                : GetAccessorOrDefault(baseName, 0, normalsArray.Length)
                  ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                      normalsArray.Select(SwapAxesForPosition).ToArray());

            // TODO: Do Tangents also need swapping axes?
            baseName = $"${rootNode.Name}/tangent";
            accessors.Tangent = tangents is null || !usesTangent
                ? null
                : GetAccessorOrDefault(baseName, 0, tangents.Tangents.Length / 2)
                  ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                      tangents.Tangents.Cast<Tangent>()
                          .Where((_, i) => i % 2 == 0)
                          .Select(x => new TypedVec4<float>(x.x / 32767f, x.y / 32767f, x.z / 32767f, x.w / 32767f))
                          .ToArray());

            baseName = $"${rootNode.Name}/uv";
            accessors.TexCoord0 =
                uvs is null || !usesUv
                    ? null
                    : GetAccessorOrDefault(baseName, 0, uvs.UVs.Length)
                      ?? AddAccessor($"{node.Name}/uv", -1, GltfBufferViewTarget.ArrayBuffer, uvs.UVs);
        }

        if (vertsUvs is not null && vertices is null)
            return Log.E<bool>("Mesh[{0}]: vertsUvs is currently not supported.", rootNode.Name);  // TODO: Support VertsUvs.

        baseName = $"${rootNode.Name}/index";
        var indexBufferView = GetBufferViewOrDefault(baseName) ??
                              AddBufferView(baseName, indices.Indices, GltfBufferViewTarget.ElementArrayBuffer);
        newMesh = new GltfMesh
        {
            Name = $"{rootNode.Name}/mesh",
            Primitives = subsets.MeshSubsets
                .Select(x => Tuple.Create(x, materialMap.GetValueOrDefault(x.MatID)))
                .Where(x => !(x.Item2?.IsSkippedFromArgs ?? false))
                .Select(x =>
                {
                    var (v, mat) = x;

                    return new GltfMeshPrimitive
                    {
                        Attributes = new GltfMeshPrimitiveAttributes
                        {
                            Position = accessors.Position,
                            Normal = accessors.Normal,
                            Tangent = mat?.Target?.HasNormalTexture() is true ? accessors.Tangent : null,
                            TexCoord0 = mat?.Target?.HasAnyTexture() is true ? accessors.TexCoord0 : null,
                            Color0 = accessors.Color0,
                        },
                        Indices = GetAccessorOrDefault(baseName, v.FirstIndex, v.FirstIndex + v.NumIndices)
                                  ?? AddAccessor(
                                      $"{node.Name}/index",
                                      indexBufferView, GltfBufferViewTarget.ElementArrayBuffer,
                                      indices.Indices, v.FirstIndex, v.FirstIndex + v.NumIndices),
                        Material = mat?.Index,
                    };
                })
                .ToList()
        };
        return newMesh.Primitives.Any();
    }

    protected bool CreateModelNode(out GltfNode node, CryEngine cryObject, bool omitSkins = false)
    {
        var model = cryObject.Models[^1];
        var controllerIdToNodeIndex = new Dictionary<uint, int>();

        var rootNodeName = Path.GetFileNameWithoutExtension(model.FileName!);
        var childNodes = new List<GltfNode>();
        foreach (var nodeChunk in model.ChunkMap.Values.OfType<ChunkNode>())
        {
            if (Args.IsNodeNameExcluded(nodeChunk.Name))
            {
                Log.D("NodeChunk[{0}]: Excluded.", nodeChunk.Name);
                continue;
            }

            if (nodeChunk.ObjectChunk is not ChunkMesh meshChunk)
            {
                Log.D("NodeChunk[{0}]: Skipped; no valid ChunkMesh is referenced to.", nodeChunk.Name);
                continue;
            }

            var rootNode = new GltfNode
            {
                Name = rootNodeName + "/" + nodeChunk.Name,
            };

            var accessors = new GltfMeshPrimitiveAttributes();

            if (!WriteMeshOrLogError(out var newMesh, rootNode, nodeChunk, meshChunk, accessors))
                continue;

            if (omitSkins)
                Log.D("NodeChunk[0]: Skipping skins.", nodeChunk.Name);
            else if (nodeChunk.GetSkinningInfo() is {HasSkinningInfo: true} skinningInfo)
            {
                if (WriteSkinOrLogError(out var newSkin, out var weights, out var joints, rootNode, skinningInfo,
                        controllerIdToNodeIndex))
                {
                    rootNode.Skin = AddSkin(newSkin);
                    foreach (var prim in newMesh.Primitives)
                    {
                        prim.Attributes.Joints0 = joints;
                        prim.Attributes.Weights0 = weights;
                    }
                }
            }
            else
                Log.D("NodeChunk[{0}]: No skinning info is available.", nodeChunk.Name);

            rootNode.Mesh = AddMesh(newMesh);
            childNodes.Add(rootNode);
        }

        if (omitSkins)
            Log.D("Model[{0}]: Skipping animations.", rootNodeName);
        else
        {
            var numAnimations = WriteAnimations(cryObject.Animations, controllerIdToNodeIndex);
            if (numAnimations == 0)
                Log.D("Model[{0}]: No associated animations found.");
            else
                Log.I("Model[{0}]: Written {1} animations.", rootNodeName, numAnimations);
        }

        switch (childNodes.Count)
        {
            case 0:
                Log.D("Model[{0}]: Empty.", rootNodeName);
                node = null!;
                return false;
            case 1:
                Log.D("Model[{0}]: Wrote 1 node.", rootNodeName);
                node = childNodes.First();
                return true;
            default:
                Log.D("Model[{0}]: Wrote {1} nodes.", rootNodeName, childNodes.Count);
                node = new GltfNode
                {
                    Name = rootNodeName,
                    Children = childNodes.Select(AddNode).ToList(),
                };
                return true;
        }
    }
}