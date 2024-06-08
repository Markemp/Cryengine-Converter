using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Gltf.Models;
using Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using CgfConverter.Renderers.MaterialTextures;
using CgfConverter.Models;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    protected void CreateGltfNodeInto(List<int> nodes, CryEngine cryData, bool omitSkins = false)
    {
        if (cryData.MaterialFiles is not null)
            WriteMaterial(cryData.MaterialFiles.FirstOrDefault(), cryData.Materials.Values.FirstOrDefault());

        foreach (ChunkNode cryNode in cryData.Models[0].RootNodes)
        {
            // CurrentScene.Nodes has the index for the nodes in GltfRoot.Nodes
            if (!CreateGltfNode(out GltfNode? childNode, cryData, cryNode, omitSkins))
                continue;

            nodes.Add(Root.Nodes.Count);
            Root.Nodes.Add(childNode);
        }
    }

    protected bool CreateGltfNode(
        [MaybeNullWhen(false)] out GltfNode node,
        CryEngine cryData,
        bool omitSkins)
    {
        node = new GltfNode
        {
            Name = cryData.Name,
        };

        CreateGltfNodeInto(node.Children, cryData, omitSkins);
        return node.Children.Any();
    }

    /// <summary>
    /// Recursive method to add a gltf node to the nodes array. Crynode should always be from model[0]
    /// </summary>
    /// <param name="node">The added node.</param>
    /// <param name="cryData">Whole data.</param>
    /// <param name="cryNode">A node chunk</param>
    /// <param name="omitSkins">Whether to omit skins.</param>
    /// <returns><c>true</c> if success.</returns>
    protected bool CreateGltfNode(
        [MaybeNullWhen(false)] out GltfNode node,
        CryEngine cryData,
        ChunkNode cryNode,
        bool omitSkins)
    {
        if (Args.IsNodeNameExcluded(cryNode.Name))
        {
            node = null;
            Log.D("NodeChunk[{0}]: Excluded.", cryNode.Name);
            return false;
        }

        var controllerIdToNodeIndex = new Dictionary<int, int>();

        // Create this node and add to GltfRoot.Nodes
        var rotationQuat = Quaternion.CreateFromRotationMatrix(cryNode.LocalTransform);
        var translation = cryNode.LocalTransform.GetTranslation();

        node = new GltfNode
        {
            Name = cryNode.Name,
            Translation = SwapAxesForPosition(translation).ToGltfList(),
            Rotation = SwapAxesForLayout(rotationQuat).ToGltfList(),
            Scale = Vector3.One.ToGltfList()
        };

        // Add mesh if needed
        if (cryData.Models[0].IsIvoFile ||
            cryData.Models[0].ChunkMap[cryNode.ObjectNodeID].ChunkType != ChunkType.Helper)
        {
            if (cryData.Models.Count == 1)
            {
                if (cryNode.ObjectChunk is ChunkMesh meshChunk
                    && meshChunk.MeshSubsetsData != 0)
                {
                    AddMesh(cryData, cryNode, node, controllerIdToNodeIndex, omitSkins);
                }
            }

            else  // Has geometry file
            {
                // Some nodes don't have matching geometry in geometry file, even though the object chunk for the node
                // points to a mesh chunk ($PHYSICS_Proxy_Tail in Buccaneer Blue).  Check if the node exists in the geometry
                // file, and if not, continue processing.
                ChunkNode? geometryNode = cryData.Models[1].NodeMap.Values.FirstOrDefault(a => a.Name == cryNode.Name);
                if (geometryNode is not null)
                {
                    if (cryData.Models[1].ChunkMap[geometryNode.ObjectNodeID] is ChunkMesh geometryMesh
                        && geometryMesh.NumIndices != 0)
                    {
                        AddMesh(cryData, geometryNode, node, controllerIdToNodeIndex, omitSkins);
                    }
                }
            }
        }

        if (!omitSkins)
            _ = WriteAnimations(cryData.Animations, controllerIdToNodeIndex);

        // For each child, recursively call this method to add the child to GltfRoot.Nodes.
        foreach (ChunkNode cryChildNode in cryNode.AllChildNodes)
        {
            if (!CreateGltfNode(out GltfNode? childNode, cryData, cryChildNode, omitSkins))
                continue;

            node.Children.Add(Root.Nodes.Count);
            Root.Nodes.Add(childNode);
        }

        return true;
    }

    private void AddMesh(CryEngine cryData, ChunkNode cryNode, GltfNode gltfNode, Dictionary<int, int> controllerIdToNodeIndex, bool omitSkins)
    {
        var accessors = new GltfMeshPrimitiveAttributes();
        var meshChunk = cryNode.ObjectChunk as ChunkMesh;
        WriteMeshOrLogError(out var gltfMesh, cryData, gltfNode, cryNode, meshChunk!, accessors);

        gltfNode.Mesh = AddMesh(gltfMesh);

        if (omitSkins)
            return;

        if (cryData.SkinningInfo is { HasSkinningInfo: true } skinningInfo)
        {
            if (WriteSkinOrLogError(out var newSkin, out var weights, out var joints, gltfNode, skinningInfo,
                controllerIdToNodeIndex))
            {
                gltfNode.Skin = AddSkin(newSkin);
                foreach (var prim in gltfMesh.Primitives)
                {
                    prim.Attributes.Joints0 = joints;
                    prim.Attributes.Weights0 = weights;
                }
            }
        }
        else
            Log.D("NodeChunk[{0}]: No skinning info is available.", cryNode.Name);
    }

    private bool WriteSkinOrLogError(
        out GltfSkin newSkin,
        out int weights,
        out int joints,
        GltfNode rootNode,
        SkinningInfo skinningInfo,
        IDictionary<int, int> controllerIdToNodeIndex)
    {
        if (!skinningInfo.HasSkinningInfo)
            throw new ArgumentException("HasSkinningInfo must be true", nameof(skinningInfo));

        newSkin = null!;
        weights = joints = 0;

        var baseName = $"{rootNode.Name}/bone/weight";

        weights =
            GetAccessorOrDefault(baseName, 0,
                skinningInfo.IntVertices is null ? skinningInfo.BoneMapping.Count : skinningInfo.Ext2IntMap.Count)
            ?? AddAccessor(baseName, -1, null,
                skinningInfo.IntVertices is null
                    ? skinningInfo.BoneMapping
                        .Select(x => new Vector4(
                            x.Weight[0] / 255f, x.Weight[1] / 255f, x.Weight[2] / 255f, x.Weight[3] / 255f))
                        .ToArray()
                    : skinningInfo.Ext2IntMap
                        .Select(x => skinningInfo.IntVertices[x])
                        .Select(x => new Vector4(
                            x.Weights[0], x.Weights[1], x.Weights[2], x.Weights[3]))
                        .ToArray());

        var boneIdToBindPoseMatrices = new Dictionary<int, Matrix4x4>();
        foreach (var bone in skinningInfo.CompiledBones)
        {
            var boneId = skinningInfo.CompiledBones.IndexOf(bone);
            var parentBone = skinningInfo.CompiledBones[boneId + bone.offsetParent];
            var parentBoneId = skinningInfo.CompiledBones.IndexOf(parentBone);
            var matrix = boneIdToBindPoseMatrices[skinningInfo.CompiledBones.IndexOf(bone)] = bone.BindPoseMatrix;

            if (bone.offsetParent != 0)
            {
                if (!Matrix4x4.Invert(boneIdToBindPoseMatrices[parentBoneId], out var parentMat))
                    return Log.E<bool>("CompiledBone[{0}/{1}]: Failed to invert BindPoseMatrix.",
                        rootNode.Name, bone.ParentBone?.boneName);

                matrix *= parentMat;
            }

            if (!Matrix4x4.Invert(matrix, out matrix))
                return Log.E<bool>("CompiledBone[{0}/{1}]: Failed to invert BindPoseMatrix.",
                    rootNode.Name, bone.boneName);

            matrix = SwapAxes(Matrix4x4.Transpose(matrix));
            if (!Matrix4x4.Decompose(matrix, out var scale, out var rotation, out var translation))
                return Log.E<bool>("CompiledBone[{0}/{1}]: BindPoseMatrix is not decomposable.",
                rootNode.Name, bone.boneName);

            var boneNode = new GltfNode
            {
                Name = bone.boneName,

                Scale = (scale - Vector3.One).LengthSquared() > 0.000001
                    ? new List<float> { scale.X, scale.Y, scale.Z }
                    : null,
                Translation = translation != Vector3.Zero
                    ? new List<float> { translation.X, translation.Y, translation.Z }
                    : null,
                Rotation = rotation != Quaternion.Identity
                    ? new List<float> { rotation.X, rotation.Y, rotation.Z, rotation.W }
                    : null
            };
            controllerIdToNodeIndex[bone.ControllerID] = AddNode(boneNode);

            if (bone.parentID == 0)
                CurrentScene.Nodes.Add(controllerIdToNodeIndex[bone.ControllerID]);
            else
                Root.Nodes[controllerIdToNodeIndex[parentBone.ControllerID]].Children
                    .Add(controllerIdToNodeIndex[bone.ControllerID]);
        }

        baseName = $"{rootNode.Name}/bone/joint";
        joints =
            GetAccessorOrDefault(
                baseName,
                0,
                skinningInfo.IntVertices is null ? skinningInfo.BoneMapping.Count : skinningInfo.Ext2IntMap.Count)
            ?? AddAccessor(
                baseName,
                -1,
                null,
                skinningInfo is { HasIntToExtMapping: true, IntVertices: { } }
                    ? skinningInfo.Ext2IntMap
                        .Select(x => skinningInfo.IntVertices[x])
                        .Select(x => new TypedVec4<ushort>(
                            x.BoneIDs[0], x.BoneIDs[1], x.BoneIDs[2], x.BoneIDs[3]))
                        .ToArray()
                    : skinningInfo.BoneMapping
                        .Select(x => new TypedVec4<ushort>(
                            (ushort)x.BoneIndex[0], (ushort)x.BoneIndex[1], (ushort)x.BoneIndex[2],
                            (ushort)x.BoneIndex[3]))
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
            Skeleton = 1
        };
        return true;
    }

    private bool WriteMeshOrLogError(
        out GltfMesh newMesh,
        CryEngine cryData,
        GltfNode gltfNode,
        ChunkNode nodeChunk,
        ChunkMesh mesh,
        GltfMeshPrimitiveAttributes accessors)
    {
        newMesh = null!;

        var vertices = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.VerticesData) as ChunkDataStream;
        var vertsUvs = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.VertsUVsData) as ChunkDataStream;
        var normals = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.NormalsData) as ChunkDataStream;
        var uvs = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.UVsData) as ChunkDataStream;
        var indices = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.IndicesData) as ChunkDataStream;
        var colors = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.ColorsData) as ChunkDataStream;
        var colors2 = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.Colors2Data) as ChunkDataStream;
        var tangents = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.TangentsData) as ChunkDataStream;
        var subsets = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.MeshSubsetsData) as ChunkMeshSubsets;

        if (indices is null)
            return Log.D<bool>("Mesh[{0}]: IndicesData is empty.", gltfNode.Name);
        if (subsets is null)
            return Log.D<bool>("Mesh[{0}]: MeshSubsetsData is empty.", gltfNode.Name);
        if (vertices is null && vertsUvs is null)
            return Log.D<bool>("Mesh[{0}]: both VerticesData and VertsUVsData are empty.", gltfNode.Name);

        if (subsets.MeshSubsets.All(x => FindMaterial(x.MatID)?.IsSkippedFromArgs ?? false))
            return false;

        var usesTangent = subsets.MeshSubsets.Any(v =>
            FindMaterial(v.MatID) is { } m && m.GltfMaterial?.HasNormalTexture() is true);

        var usesUv = usesTangent || subsets.MeshSubsets.Any(v =>
            FindMaterial(v.MatID) is { } m && m.GltfMaterial?.HasAnyTexture() is true);

        string baseName;

        if (vertices is not null || vertsUvs is not null)
        {
            if (vertices is not null)
            {
                baseName = $"{gltfNode.Name}/vertex";
                accessors.Position =
                    GetAccessorOrDefault(baseName, 0, vertices.Vertices.Length)
                    ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                        vertices.Vertices.Select(SwapAxesForPosition).ToArray());

                if (usesUv)
                {
                    baseName = $"${gltfNode.Name}/uv";
                    accessors.TexCoord0 =
                        uvs is null
                            ? null
                            : GetAccessorOrDefault(baseName, 0, uvs.UVs.Length)
                            ?? AddAccessor($"{nodeChunk.Name}/uv", -1, GltfBufferViewTarget.ArrayBuffer, uvs.UVs);
                }
            }
            else  // VertsUVs.
            {
                baseName = $"{gltfNode.Name}/vertex";
                var multiplerVector = Vector3.Abs((mesh.MinBound - mesh.MaxBound) / 2f);
                if (multiplerVector.X < 1) { multiplerVector.X = 1; }
                if (multiplerVector.Y < 1) { multiplerVector.Y = 1; }
                if (multiplerVector.Z < 1) { multiplerVector.Z = 1; }
                var boundaryBoxCenter = (mesh.MinBound + mesh.MaxBound) / 2f;
                var scaleToBBox = cryData.InputFile.EndsWith("cga") || cryData.InputFile.EndsWith("cgf");

                accessors.Position =
                    GetAccessorOrDefault(baseName, 0, vertsUvs.Vertices.Length)
                        ?? AddAccessor(
                            baseName,
                            -1,
                            GltfBufferViewTarget.ArrayBuffer,
                            vertsUvs.Vertices
                                .Select(x => scaleToBBox ? (x * multiplerVector) + boundaryBoxCenter : x)
                                .Select(SwapAxesForPosition)
                                .ToArray());

                if (usesUv)
                {
                    baseName = $"${gltfNode.Name}/uv";
                    accessors.TexCoord0 =
                        GetAccessorOrDefault(baseName, 0, vertsUvs.UVs.Length)
                        ?? AddAccessor(
                            $"{nodeChunk.Name}/uv",
                            -1,
                            GltfBufferViewTarget.ArrayBuffer,
                            vertsUvs.UVs);
                }
            }

            var normalsArray = normals?.Normals ?? tangents?.Normals;
            baseName = $"{gltfNode.Name}/normal";
            accessors.Normal = normalsArray is null
                ? null
                : GetAccessorOrDefault(baseName, 0, normalsArray.Length)
                  ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                      normalsArray.Select(SwapAxesForPosition).ToArray());

            baseName = $"{gltfNode.Name}/colors";
            accessors.Color0 = colors is null
                ? null
                : (GetAccessorOrDefault(baseName, 0, colors.Colors.Length)
                    ?? AddAccessor(
                        baseName,
                        -1,
                        GltfBufferViewTarget.ArrayBuffer,
                        colors.Colors.Select(x => new Vector4(x.r, x.g, x.b, x.a) / 255f)
                            .ToArray()));

            baseName = $"${gltfNode.Name}/tangent";
            accessors.Tangent = tangents is null || !usesTangent
                ? null
                : GetAccessorOrDefault(baseName, 0, tangents.Tangents.Length / 2)
                  ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                      tangents.Tangents.Cast<Tangent>()
                          .Where((_, i) => i % 2 == 1)
                          .Select(x => new Vector4(x.x, x.y, x.z, x.w) / 32767f)
                          .Select(SwapAxesForTangent)
                          .ToArray());

        }

        baseName = $"${gltfNode.Name}/index";
        var indexBufferView = GetBufferViewOrDefault(baseName) ??
                              AddBufferView(baseName, indices.Indices, GltfBufferViewTarget.ElementArrayBuffer);

        newMesh = new GltfMesh
        {
            Name = $"{gltfNode.Name}/mesh",
            Primitives = subsets.MeshSubsets
                .Select(x => Tuple.Create(x, FindMaterial(x.MatID)))
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
                            Tangent = mat?.GltfMaterial?.HasNormalTexture() is true ? accessors.Tangent : null,
                            TexCoord0 = mat?.GltfMaterial?.HasAnyTexture() is true ? accessors.TexCoord0 : null,
                            Color0 = new ParsedGenMask(mat?.CryMaterial.GenMask).UseVertexColors ? accessors.Color0 : null,
                        },
                        Indices = GetAccessorOrDefault(baseName, v.FirstIndex, v.FirstIndex + v.NumIndices)
                                  ?? AddAccessor(
                                      $"{nodeChunk.Name}/index",
                                      indexBufferView, GltfBufferViewTarget.ElementArrayBuffer,
                                      indices.Indices, v.FirstIndex, v.FirstIndex + v.NumIndices),
                        Material = mat?.Index,
                    };
                })
                .ToList()
        };

        return newMesh.Primitives.Any();

        WrittenMaterial? FindMaterial(int matId)
        {
            // TODO: This only works for models with a single material file. Should be almost all models, but some may fail here.
            var allSubMaterials = cryData.Materials.Values
                .Where(material => material?.SubMaterials != null)
                .SelectMany(material => material.SubMaterials);


            if (cryData.Materials?.First().Value.SubMaterials is not {} submats)
                return null;
            if (matId >= submats.Length || matId < 0)
                return null;
            string? subMatName = submats[matId].Name;
            return subMatName is null || cryData.MaterialFiles is null
                ? null
                : _materialMap.GetValueOrDefault((MaterialFile: cryData.MaterialFiles.FirstOrDefault(), subMatName));
        }
    }
}
