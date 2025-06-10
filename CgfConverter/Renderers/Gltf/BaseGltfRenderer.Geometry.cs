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
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Scene;
using CgfConverter.Utilities;
using CgfConverter.Models.Structs;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    protected void CreateGltfNodeInto(List<int> nodes, CryEngine cryData, bool omitSkins = false)
    {
        if (cryData.MaterialFiles is not null)
            WriteMaterial(cryData.MaterialFiles.FirstOrDefault(), cryData.Materials.Values.FirstOrDefault());

        // THERE CAN BE MULTIPLE ROOT NODES IN EACH FILE!  Check to see if the parentnodeid ~0 and be sure to add a node for it.
        List<ColladaNode> positionNodes = [];
        List<ChunkNode> positionRoots = cryData.Nodes.Where(a => a.ParentNodeID == ~0).ToList();

        foreach (ChunkNode cryNode in positionRoots)
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
        return node.Children.Count != 0;
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

        var controllerIdToNodeIndex = new Dictionary<uint, int>();

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
        if (cryNode.ChunkHelper is null && cryNode.MeshData?.GeometryInfo is not null)
        {
            // Some nodes don't have matching geometry in geometry file, even though the object chunk for the node
            // points to a mesh chunk ($PHYSICS_Proxy_Tail in Buccaneer Blue).  Check if the node exists in the geometry
            // file, and if not, continue processing.

            AddMesh(cryData, cryNode, node, controllerIdToNodeIndex, omitSkins);
        }

        if (!omitSkins)
            _ = WriteAnimations(cryData.Animations, controllerIdToNodeIndex);

        // For each child, recursively call this method to add the child to GltfRoot.Nodes.
        foreach (ChunkNode cryChildNode in cryNode.Children)
        {
            if (!CreateGltfNode(out GltfNode? childNode, cryData, cryChildNode, omitSkins))
                continue;

            node.Children.Add(Root.Nodes.Count);
            Root.Nodes.Add(childNode);
        }

        return true;
    }

    private void AddMesh(CryEngine cryData, ChunkNode cryNode, GltfNode gltfNode, Dictionary<uint, int> controllerIdToNodeIndex, bool omitSkins)
    {
        var accessors = new GltfMeshPrimitiveAttributes();
        var meshChunk = cryNode.MeshData;
        WriteMeshOrLogError(out var gltfMesh, cryData, gltfNode, cryNode, meshChunk!, accessors);

        gltfNode.Mesh = AddMesh(gltfMesh);

        if (omitSkins)
            return;

        if (cryData.SkinningInfo is { HasSkinningInfo: true } skinningInfo)
        {
            if (WriteSkinOrLogError(out var newSkin, out var weights, out var joints, cryData, gltfNode, skinningInfo,
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
        CryEngine cryData,
        GltfNode rootNode,
        SkinningInfo skinningInfo,
        IDictionary<uint, int> controllerIdToNodeIndex)
    {
        if (!skinningInfo.HasSkinningInfo)
            throw new ArgumentException("HasSkinningInfo must be true", nameof(skinningInfo));

        newSkin = null!;
        weights = joints = 0;

        var baseName = $"{rootNode.Name}/bone/weight";

        var nodeChunk = cryData.RootNode;
        var boneMappingData = nodeChunk.MeshData?.GeometryInfo?.BoneMappings;

        weights =
            GetAccessorOrDefault(baseName, 0,
                skinningInfo.IntVertices is null ? skinningInfo.BoneMappings.Count : skinningInfo.Ext2IntMap.Count)
            ?? AddAccessor(baseName, -1, null,
                skinningInfo.IntVertices is null
                    ? skinningInfo.BoneMappings
                        .Select(x => new Vector4(
                            x.Weight[0], x.Weight[1], x.Weight[2], x.Weight[3]))
                        .ToArray()
                    : skinningInfo.Ext2IntMap
                        .Select(x => skinningInfo.IntVertices[x])
                        .Select(x => new Vector4(
                            x.BoneMapping.Weight[0], x.BoneMapping.Weight[1], x.BoneMapping.Weight[2], x.BoneMapping.Weight[3]))
                        .ToArray());

        var boneIdToBindPoseMatrices = new Dictionary<int, Matrix4x4>();
        foreach (var bone in skinningInfo.CompiledBones)
        {
            var boneId = skinningInfo.CompiledBones.IndexOf(bone); // parent bone id is always 0
            var parentBone = boneId == 0 ? bone : skinningInfo.CompiledBones[boneId + bone.OffsetParent];
            var parentBoneId = skinningInfo.CompiledBones.IndexOf(parentBone);
            var matrix = boneIdToBindPoseMatrices[boneId] = bone.BindPoseMatrix;

            if (bone.OffsetParent != 0 && bone.OffsetParent != 0xffffffff)
            {
                if (!Matrix4x4.Invert(boneIdToBindPoseMatrices[parentBoneId], out var parentMat))
                    return Log.E<bool>("CompiledBone[{0}/{1}]: Failed to invert BindPoseMatrix.",
                        rootNode.Name, bone.ParentBone?.BoneName);

                matrix *= parentMat;
            }

            if (!Matrix4x4.Invert(matrix, out matrix))
                return Log.E<bool>("CompiledBone[{0}/{1}]: Failed to invert BindPoseMatrix.",
                    rootNode.Name, bone.BoneName);

            matrix = SwapAxes(Matrix4x4.Transpose(matrix));
            if (!Matrix4x4.Decompose(matrix, out var scale, out var rotation, out var translation))
                return Log.E<bool>("CompiledBone[{0}/{1}]: BindPoseMatrix is not decomposable.",
                rootNode.Name, bone.BoneName);

            var boneNode = new GltfNode
            {
                Name = bone.BoneName,

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

            if (bone.ParentControllerIndex == 0)
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
                skinningInfo.IntVertices is null ? skinningInfo.BoneMappings.Count : skinningInfo.Ext2IntMap.Count)
            ?? AddAccessor(
                baseName,
                -1,
                null,
                skinningInfo is { HasIntToExtMapping: true, IntVertices: { } }
                    ? skinningInfo.Ext2IntMap
                        .Select(x => skinningInfo.IntVertices[x])
                        .Select(x => new TypedVec4<ushort>(
                            x.BoneMapping.BoneIndex[0], x.BoneMapping.BoneIndex[1], x.BoneMapping.BoneIndex[2], x.BoneMapping.BoneIndex[3]))
                        .ToArray()
                    : skinningInfo.BoneMappings
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

        if (nodeChunk.MeshData is not ChunkMesh meshChunk)
            return Log.D<bool>("Mesh[{0}]: MeshData is not a ChunkMesh.", gltfNode.Name);

        var subsets = meshChunk.GeometryInfo.GeometrySubsets;
        Datastream<uint>? indices = meshChunk.GeometryInfo.Indices;
        Datastream<UV>? uvs = meshChunk.GeometryInfo.UVs;
        Datastream<Vector3>? verts = meshChunk.GeometryInfo.Vertices;
        Datastream<VertUV>? vertsUvs = meshChunk.GeometryInfo.VertUVs;
        Datastream<Vector3>? normals = meshChunk.GeometryInfo.Normals;
        Datastream<IRGBA>? colors = meshChunk.GeometryInfo.Colors;

        if (indices is null)
            return Log.D<bool>("Mesh[{0}]: IndicesData is empty.", gltfNode.Name);
        if (subsets is null)
            return Log.D<bool>("Mesh[{0}]: MeshSubsetsData is empty.", gltfNode.Name);
        if (verts is null && vertsUvs is null)
            return Log.D<bool>("Mesh[{0}]: both VerticesData and VertsUVsData are empty.", gltfNode.Name);

        if (subsets.All(x => FindMaterial(x.MatID)?.IsSkippedFromArgs ?? false))
            return false;

        var usesTangent = subsets.Any(v =>
            FindMaterial(v.MatID) is { } m && m.GltfMaterial?.HasNormalTexture() is true);

        var usesUv = usesTangent || subsets.Any(v =>
            FindMaterial(v.MatID) is { } m && m.GltfMaterial?.HasAnyTexture() is true);

        string baseName;

        if (verts is not null || vertsUvs is not null)
        {
            if (verts is not null)
            {
                baseName = $"{gltfNode.Name}/vertex";
                accessors.Position =
                    GetAccessorOrDefault(baseName, 0, verts.Data.Length)
                    ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                        verts.Data.Select(SwapAxesForPosition).ToArray());

                if (usesUv)
                {
                    baseName = $"${gltfNode.Name}/uv";
                    accessors.TexCoord0 =
                        uvs is null
                            ? null
                            : GetAccessorOrDefault(baseName, 0, uvs.Data.Length)
                            ?? AddAccessor($"{nodeChunk.Name}/uv", -1, GltfBufferViewTarget.ArrayBuffer, uvs.Data);
                }
            }
            else  // VertsUVs.
            {
                baseName = $"{gltfNode.Name}/vertex";

                var multiplerVector = Vector3.Abs((mesh.MinBound - mesh.MaxBound) / 2f);
                if (multiplerVector.X < 1) multiplerVector.X = 1;
                if (multiplerVector.Y < 1) multiplerVector.Y = 1;
                if (multiplerVector.Z < 1) multiplerVector.Z = 1;
                var boundaryBoxCenter = (mesh.MinBound + mesh.MaxBound) / 2f;

                Vector3 scalingVector = Vector3.One;
                if (meshChunk.ScalingVectors is not null)
                {
                    scalingVector = Vector3.Abs((meshChunk.ScalingVectors.Max - meshChunk.ScalingVectors.Min) / 2f);
                    if (scalingVector.X < 1) scalingVector.X = 1;
                    if (scalingVector.Y < 1) scalingVector.Y = 1;
                    if (scalingVector.Z < 1) scalingVector.Z = 1;
                }
                var scaleToBBox = cryData.InputFile.EndsWith("cga") || cryData.InputFile.EndsWith("cgf");
                var scalingBoxCenter = meshChunk.ScalingVectors is not null ? (meshChunk.ScalingVectors.Max + meshChunk.ScalingVectors.Min) / 2f : Vector3.Zero;
                var useScalingBox = cryData.InputFile
                    .EndsWith("cga") || cryData.InputFile.EndsWith("cgf")
                    && meshChunk.ScalingVectors is not null;

                var numberOfElements = nodeChunk.MeshData.GeometryInfo.GeometrySubsets.Sum(x => x.NumVertices);

                var vertslocal = meshChunk.GeometryInfo.VertUVs;

                var subsetVerts = (subsets ?? [])
                    .SelectMany(subset => Enumerable
                        .Range(subset.FirstVertex, subset.NumVertices)
                        .Select(i => vertsUvs.Data[i].Vertex)
                        .Select(x => scaleToBBox ? (x * multiplerVector) + boundaryBoxCenter : x)
                        .Select(SwapAxesForPosition))
                    .ToArray();

                accessors.Position =
                    GetAccessorOrDefault(baseName, 0, numberOfElements)
                        ?? AddAccessor(
                            baseName,
                            -1,
                            GltfBufferViewTarget.ArrayBuffer,
                            (subsets ?? [])
                                .SelectMany(subset => Enumerable
                                    .Range(subset.FirstVertex, subset.NumVertices)
                                    .Select(i => vertsUvs.Data[i].Vertex)
                                    .Select(x => useScalingBox ? (x * scalingVector) + scalingBoxCenter : (x * multiplerVector) + boundaryBoxCenter)
                                    .Select(SwapAxesForPosition))
                                .ToArray());

                if (usesUv)
                {
                    baseName = $"${gltfNode.Name}/uv";
                    accessors.TexCoord0 =
                        GetAccessorOrDefault(baseName, 0, numberOfElements)
                        ?? AddAccessor(
                            $"{nodeChunk.Name}/uv",
                            -1,
                            GltfBufferViewTarget.ArrayBuffer,
                            (meshChunk.GeometryInfo.GeometrySubsets ?? [])
                                .SelectMany(subset => Enumerable
                                    .Range(subset.FirstVertex, subset.NumVertices)
                                    .Select(i => vertsUvs.Data[i].UV))
                                .ToArray());
                }
            }

            var normalsArray = normals?.Data;
            baseName = $"{gltfNode.Name}/normal";
            accessors.Normal = normalsArray is null
                ? null
                : GetAccessorOrDefault(baseName, 0, normalsArray.Length)
                  ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                      (meshChunk.GeometryInfo.GeometrySubsets ?? [])
                          .SelectMany(subset => Enumerable
                              .Range(subset.FirstVertex, subset.NumVertices)
                              .Select(i => SwapAxesForPosition(normalsArray[i])))
                          .ToArray());

            baseName = $"{gltfNode.Name}/colors";
            accessors.Color0 = colors is null
                ? null
                : (GetAccessorOrDefault(baseName, 0, colors.Data.Length)
                    ?? AddAccessor(
                        baseName,
                        -1,
                        GltfBufferViewTarget.ArrayBuffer,
                        (meshChunk.GeometryInfo.GeometrySubsets ?? [])
                            .SelectMany(subset => Enumerable
                                .Range(subset.FirstVertex, subset.NumVertices)
                                .Select(i => new Vector4(colors.Data[i].R, colors.Data[i].G, colors.Data[i].B, colors.Data[i].A) / 255f))
                            .ToArray()));

            baseName = $"${gltfNode.Name}/tangent";
        }

        baseName = $"${gltfNode.Name}/index";
        var remappedIndices = new uint[indices.Data.Length];
        // Create a map of global indices to local indices
        var localIndexMap = new Dictionary<uint, uint>();
        uint currentOffset = 0;

        foreach (var subset in meshChunk.GeometryInfo.GeometrySubsets ?? [])
        {
            var firstGlobalIndex = indices.Data[subset.FirstIndex];

            for (int i = 0; i < subset.NumIndices; i++)
            {
                uint globalIndex = indices.Data[subset.FirstIndex + i];
                uint localIndex = (uint)((globalIndex - firstGlobalIndex) + currentOffset);

                // Map the global index to its local counterpart
                localIndexMap[globalIndex] = localIndex;
            }

            currentOffset += (uint)subset.NumVertices;
        }

        for (int i = 0; i < indices.Data.Length; i++)
        {
            remappedIndices[i] = localIndexMap.TryGetValue(indices.Data[i], out uint localIndex)
                ? localIndex
                : indices.Data[i]; // Fallback to original index if not found in map
        }

        var indexBufferView = GetBufferViewOrDefault(baseName) ??
                      AddBufferView(baseName, remappedIndices, GltfBufferViewTarget.ElementArrayBuffer);

        newMesh = new GltfMesh
        {
            Name = $"{gltfNode.Name}/mesh",
            Primitives = subsets
                .Select(x => Tuple.Create(x, FindMaterial(x.MatID)))
                .Where(x => !(x.Item2?.IsSkippedFromArgs ?? false))
                .Where(x => x.Item1.NumVertices != 0)
                .Select(x => {
                    var (v, mat) = x;

                    return new GltfMeshPrimitive
                    {
                        Attributes = new GltfMeshPrimitiveAttributes
                        {
                            Position = accessors.Position,
                            Normal = accessors.Normal,
                            TexCoord0 = mat?.GltfMaterial?.HasAnyTexture() is true ? accessors.TexCoord0 : null,
                            Color0 = new ParsedGenMask(mat?.CryMaterial.GenMask).UseVertexColors ? accessors.Color0 : null,
                        },
                        Indices = GetAccessorOrDefault(baseName, v.FirstIndex, v.FirstIndex + v.NumIndices)
                                  ?? AddAccessor(
                                      $"{nodeChunk.Name}/index",
                                      indexBufferView, GltfBufferViewTarget.ElementArrayBuffer,
                                      indices.Data, v.FirstIndex, v.FirstIndex + v.NumIndices),
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
