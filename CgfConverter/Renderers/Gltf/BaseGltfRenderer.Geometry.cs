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

        // For Ivo format with skinning, create skeleton first and attach meshes to skeleton nodes
        // This ensures geometry moves with the skeleton
        if (!omitSkins && cryData.SkinningInfo is { HasSkinningInfo: true } skinningInfo
            && HasObjectNodeIndexMappings(skinningInfo))
        {
            CreateIvoSkeletonWithMeshes(nodes, cryData, skinningInfo);
            return;
        }

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

    /// <summary>
    /// Check if skinning info has ObjectNodeIndex mappings (Ivo format).
    /// </summary>
    private static bool HasObjectNodeIndexMappings(SkinningInfo skinningInfo)
    {
        if (skinningInfo.CompiledBones is null || skinningInfo.CompiledBones.Count == 0)
            return false;

        // If any bone has a valid ObjectNodeIndex, we have Ivo format
        return skinningInfo.CompiledBones.Any(b => b.ObjectNodeIndex >= 0);
    }

    /// <summary>
    /// Creates skeleton with meshes attached directly to bone nodes (Ivo format).
    /// This ensures geometry moves with the skeleton.
    /// </summary>
    private void CreateIvoSkeletonWithMeshes(List<int> nodes, CryEngine cryData, SkinningInfo skinningInfo)
    {
        var controllerIdToNodeIndex = new Dictionary<uint, int>();
        var boneIndexToNodeIndex = new Dictionary<int, int>();

        // Build mapping from ObjectNodeIndex (ChunkNode index) to bone index
        var nodeIndexToBoneIndex = new Dictionary<int, int>();
        for (int boneIndex = 0; boneIndex < skinningInfo.CompiledBones.Count; boneIndex++)
        {
            var bone = skinningInfo.CompiledBones[boneIndex];
            if (bone.ObjectNodeIndex >= 0)
            {
                nodeIndexToBoneIndex[bone.ObjectNodeIndex] = boneIndex;
            }
        }

        // Get all ChunkNodes indexed by their position (for mesh lookup)
        var allNodes = cryData.Nodes;
        var nodeIndexToChunkNode = new Dictionary<int, ChunkNode>();
        for (int i = 0; i < allNodes.Count; i++)
        {
            nodeIndexToChunkNode[i] = allNodes[i];
        }

        // Create all bone nodes first
        for (int boneIndex = 0; boneIndex < skinningInfo.CompiledBones.Count; boneIndex++)
        {
            var bone = skinningInfo.CompiledBones[boneIndex];

            // Compute local transform using same approach as existing code
            Matrix4x4 localMatrix;

            if (bone.ParentBone == null)
            {
                if (!Matrix4x4.Invert(bone.BindPoseMatrix, out localMatrix))
                {
                    Log.W("CompiledBone[{0}]: Failed to invert BindPoseMatrix for root", bone.BoneName);
                    localMatrix = Matrix4x4.Identity;
                }
            }
            else
            {
                if (Matrix4x4.Invert(bone.BindPoseMatrix, out var childBoneToWorld))
                {
                    localMatrix = bone.ParentBone.BindPoseMatrix * childBoneToWorld;
                }
                else
                {
                    Log.W("CompiledBone[{0}]: Failed to invert BindPoseMatrix", bone.BoneName);
                    localMatrix = Matrix4x4.Identity;
                }
            }

            // Transpose and swap axes for glTF coordinate system (Y-up)
            var matrix = SwapAxes(Matrix4x4.Transpose(localMatrix));
            if (!Matrix4x4.Decompose(matrix, out var scale, out var rotation, out var translation))
            {
                Log.W("CompiledBone[{0}]: BindPoseMatrix is not decomposable", bone.BoneName);
                scale = Vector3.One;
                rotation = Quaternion.Identity;
                translation = Vector3.Zero;
            }

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

            var nodeIndex = AddNode(boneNode);
            boneIndexToNodeIndex[boneIndex] = nodeIndex;
            controllerIdToNodeIndex[bone.ControllerID] = nodeIndex;
        }

        // Set up bone parent-child relationships
        for (int boneIndex = 0; boneIndex < skinningInfo.CompiledBones.Count; boneIndex++)
        {
            var bone = skinningInfo.CompiledBones[boneIndex];
            var nodeIndex = boneIndexToNodeIndex[boneIndex];

            if (bone.ParentBone == null)
            {
                // Root bone - add to scene
                nodes.Add(nodeIndex);
            }
            else
            {
                // Find parent bone index and add as child
                var parentBoneIndex = skinningInfo.CompiledBones.IndexOf(bone.ParentBone);
                if (parentBoneIndex >= 0 && boneIndexToNodeIndex.TryGetValue(parentBoneIndex, out var parentNodeIndex))
                {
                    Root.Nodes[parentNodeIndex].Children.Add(nodeIndex);
                }
                else
                {
                    Log.W("Bone[{0}]: Parent bone not found, treating as root", bone.BoneName);
                    nodes.Add(nodeIndex);
                }
            }
        }

        // Attach meshes to corresponding bone nodes
        foreach (var kvp in nodeIndexToBoneIndex)
        {
            var chunkNodeIndex = kvp.Key;
            var boneIndex = kvp.Value;

            if (!nodeIndexToChunkNode.TryGetValue(chunkNodeIndex, out var cryNode))
                continue;

            if (cryNode.MeshData?.GeometryInfo is null)
                continue;

            var boneNodeIndex = boneIndexToNodeIndex[boneIndex];
            var boneNode = Root.Nodes[boneNodeIndex];

            // Add mesh to this bone node
            AddMeshToExistingNode(cryData, cryNode, boneNode, controllerIdToNodeIndex, skinningInfo);
        }

        // Write animations using the skeleton mapping
        _ = WriteAnimations(cryData.Animations, controllerIdToNodeIndex);
        _ = WriteCafAnimations(cryData.CafAnimations, controllerIdToNodeIndex);
    }

    /// <summary>
    /// Adds mesh to an existing node (used for Ivo format where meshes are attached to skeleton bones).
    /// </summary>
    private void AddMeshToExistingNode(
        CryEngine cryData,
        ChunkNode cryNode,
        GltfNode gltfNode,
        Dictionary<uint, int> controllerIdToNodeIndex,
        SkinningInfo skinningInfo)
    {
        var accessors = new GltfMeshPrimitiveAttributes();
        var meshChunk = cryNode.MeshData;

        if (!WriteMeshOrLogError(out var gltfMesh, cryData, gltfNode, cryNode, meshChunk!, accessors))
            return;

        gltfNode.Mesh = AddMesh(gltfMesh);

        var geometrySubsets = meshChunk?.GeometryInfo?.GeometrySubsets;
        bool usePerSubsetExtraction = meshChunk?.GeometryInfo?.VertUVs is not null;

        if (WriteSkinningDataOnly(out var newSkin, out var weights, out var joints, gltfNode, skinningInfo,
            controllerIdToNodeIndex, geometrySubsets, usePerSubsetExtraction))
        {
            gltfNode.Skin = AddSkin(newSkin);
            foreach (var prim in gltfMesh.Primitives)
            {
                prim.Attributes.Joints0 = joints;
                prim.Attributes.Weights0 = weights;
            }
        }
    }

    /// <summary>
    /// Writes skinning data (weights, joints, inverse bind matrices) without creating skeleton nodes.
    /// Used when skeleton nodes are already created separately.
    /// </summary>
    private bool WriteSkinningDataOnly(
        out GltfSkin newSkin,
        out int weights,
        out int joints,
        GltfNode rootNode,
        SkinningInfo skinningInfo,
        IDictionary<uint, int> controllerIdToNodeIndex,
        List<MeshSubset>? geometrySubsets,
        bool usePerSubsetExtraction)
    {
        newSkin = null!;
        weights = joints = 0;

        var baseName = $"{rootNode.Name}/bone/weight";

        if (usePerSubsetExtraction)
        {
            var subsets = geometrySubsets ?? [];
            var numberOfElements = subsets.Sum(x => x.NumVertices);

            weights =
                GetAccessorOrDefault(baseName, 0, numberOfElements)
                ?? AddAccessor(baseName, -1, null,
                    skinningInfo.IntVertices is null
                        ? subsets
                            .SelectMany(subset => Enumerable
                                .Range(subset.FirstVertex, subset.NumVertices)
                                .Select(i => skinningInfo.BoneMappings[i])
                                .Select(x => new Vector4(
                                    x.Weight[0], x.Weight[1], x.Weight[2], x.Weight[3])))
                            .ToArray()
                        : subsets
                            .SelectMany(subset => Enumerable
                                .Range(subset.FirstVertex, subset.NumVertices)
                                .Select(i => skinningInfo.IntVertices[skinningInfo.Ext2IntMap[i]])
                                .Select(x => new Vector4(
                                    x.BoneMapping.Weight[0], x.BoneMapping.Weight[1], x.BoneMapping.Weight[2], x.BoneMapping.Weight[3])))
                            .ToArray());
        }
        else
        {
            var skinCount = skinningInfo.IntVertices is null ? skinningInfo.BoneMappings.Count : skinningInfo.Ext2IntMap.Count;

            weights =
                GetAccessorOrDefault(baseName, 0, skinCount)
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
        }

        baseName = $"{rootNode.Name}/bone/joint";
        if (usePerSubsetExtraction)
        {
            var subsets = geometrySubsets ?? [];
            var numberOfElements = subsets.Sum(x => x.NumVertices);

            joints =
                GetAccessorOrDefault(baseName, 0, numberOfElements)
                ?? AddAccessor(
                    baseName,
                    -1,
                    null,
                    skinningInfo is { HasIntToExtMapping: true, IntVertices: { } }
                        ? subsets
                            .SelectMany(subset => Enumerable
                                .Range(subset.FirstVertex, subset.NumVertices)
                                .Select(i => skinningInfo.IntVertices[skinningInfo.Ext2IntMap[i]])
                                .Select(x => new TypedVec4<ushort>(
                                    x.BoneMapping.BoneIndex[0], x.BoneMapping.BoneIndex[1], x.BoneMapping.BoneIndex[2], x.BoneMapping.BoneIndex[3])))
                            .ToArray()
                        : subsets
                            .SelectMany(subset => Enumerable
                                .Range(subset.FirstVertex, subset.NumVertices)
                                .Select(i => skinningInfo.BoneMappings[i])
                                .Select(x => new TypedVec4<ushort>(
                                    (ushort)x.BoneIndex[0], (ushort)x.BoneIndex[1], (ushort)x.BoneIndex[2],
                                    (ushort)x.BoneIndex[3])))
                            .ToArray());
        }
        else
        {
            var skinCount = skinningInfo.IntVertices is null ? skinningInfo.BoneMappings.Count : skinningInfo.Ext2IntMap.Count;

            joints =
                GetAccessorOrDefault(baseName, 0, skinCount)
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
        }

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
            Skeleton = controllerIdToNodeIndex.Values.Min()  // Root bone node
        };
        return true;
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
        // Create shared dictionary for skeleton - will be populated by first skinned mesh
        // and reused by subsequent meshes to avoid duplicate skeletons
        var controllerIdToNodeIndex = new Dictionary<uint, int>();
        return CreateGltfNode(out node, cryData, cryNode, omitSkins, controllerIdToNodeIndex);
    }

    /// <summary>
    /// Recursive method to add a gltf node to the nodes array with shared skeleton dictionary.
    /// </summary>
    private bool CreateGltfNode(
        [MaybeNullWhen(false)] out GltfNode node,
        CryEngine cryData,
        ChunkNode cryNode,
        bool omitSkins,
        Dictionary<uint, int> controllerIdToNodeIndex)
    {
        if (Args.IsNodeNameExcluded(cryNode.Name))
        {
            node = null;
            Log.D("NodeChunk[{0}]: Excluded.", cryNode.Name);
            return false;
        }

        // Create this node and add to GltfRoot.Nodes
        // Use the transformed matrix directly to preserve correct coordinate transformation
        // This avoids issues with TRS decomposition when axes are swapped and negated
        // Note: CryEngine uses row-major with translation in last row (row-vector convention)
        // glTF uses column-major with translation in last column (column-vector convention)
        // MatrixToGltfList outputs in column-major format which naturally transposes
        var transformedMatrix = SwapAxes(cryNode.LocalTransform);

        node = new GltfNode
        {
            Name = cryNode.Name,
            Matrix = MatrixToGltfList(transformedMatrix)
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
        {
            _ = WriteAnimations(cryData.Animations, controllerIdToNodeIndex);
            _ = WriteCafAnimations(cryData.CafAnimations, controllerIdToNodeIndex);
        }

        // For each child, recursively call this method to add the child to GltfRoot.Nodes.
        // Pass the shared controllerIdToNodeIndex so all meshes use the same skeleton.
        foreach (ChunkNode cryChildNode in cryNode.Children)
        {
            if (!CreateGltfNode(out GltfNode? childNode, cryData, cryChildNode, omitSkins, controllerIdToNodeIndex))
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
            var geometrySubsets = meshChunk?.GeometryInfo?.GeometrySubsets;
            // Ivo format (VertUVs) requires per-subset extraction; traditional format (Vertices) uses raw arrays
            bool usePerSubsetExtraction = meshChunk?.GeometryInfo?.VertUVs is not null;

            if (WriteSkinOrLogError(out var newSkin, out var weights, out var joints, cryData, gltfNode, skinningInfo,
                controllerIdToNodeIndex, geometrySubsets, usePerSubsetExtraction))
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
        IDictionary<uint, int> controllerIdToNodeIndex,
        List<MeshSubset>? geometrySubsets,
        bool usePerSubsetExtraction)
    {
        if (!skinningInfo.HasSkinningInfo)
            throw new ArgumentException("HasSkinningInfo must be true", nameof(skinningInfo));

        newSkin = null!;
        weights = joints = 0;

        var baseName = $"{rootNode.Name}/bone/weight";

        var nodeChunk = cryData.RootNode;
        var boneMappingData = nodeChunk.MeshData?.GeometryInfo?.BoneMappings;

        if (usePerSubsetExtraction)
        {
            // Ivo format: extract weights per-subset to match per-subset vertex extraction
            var subsets = geometrySubsets ?? [];
            var numberOfElements = subsets.Sum(x => x.NumVertices);

            weights =
                GetAccessorOrDefault(baseName, 0, numberOfElements)
                ?? AddAccessor(baseName, -1, null,
                    skinningInfo.IntVertices is null
                        ? subsets
                            .SelectMany(subset => Enumerable
                                .Range(subset.FirstVertex, subset.NumVertices)
                                .Select(i => skinningInfo.BoneMappings[i])
                                .Select(x => new Vector4(
                                    x.Weight[0], x.Weight[1], x.Weight[2], x.Weight[3])))
                            .ToArray()
                        : subsets
                            .SelectMany(subset => Enumerable
                                .Range(subset.FirstVertex, subset.NumVertices)
                                .Select(i => skinningInfo.IntVertices[skinningInfo.Ext2IntMap[i]])
                                .Select(x => new Vector4(
                                    x.BoneMapping.Weight[0], x.BoneMapping.Weight[1], x.BoneMapping.Weight[2], x.BoneMapping.Weight[3])))
                            .ToArray());
        }
        else
        {
            // Traditional format: use raw skinning data (like USD renderer)
            var skinCount = skinningInfo.IntVertices is null ? skinningInfo.BoneMappings.Count : skinningInfo.Ext2IntMap.Count;

            weights =
                GetAccessorOrDefault(baseName, 0, skinCount)
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
        }

        // Build bone index to node index mapping
        // If skeleton already exists (controllerIdToNodeIndex is populated), reuse those nodes
        var boneIndexToNodeIndex = new Dictionary<int, int>();
        bool skeletonAlreadyExists = controllerIdToNodeIndex.Count > 0;

        for (int boneIndex = 0; boneIndex < skinningInfo.CompiledBones.Count; boneIndex++)
        {
            var bone = skinningInfo.CompiledBones[boneIndex];

            // Check if this bone's node already exists (from a previous mesh)
            if (controllerIdToNodeIndex.TryGetValue(bone.ControllerID, out int existingNodeIndex))
            {
                boneIndexToNodeIndex[boneIndex] = existingNodeIndex;
                continue;
            }

            // Compute local transform using same approach as USD renderer
            Matrix4x4 localMatrix;

            if (bone.ParentBone == null)
            {
                // Root bone: local = world, invert BindPoseMatrix to get boneToWorld
                if (!Matrix4x4.Invert(bone.BindPoseMatrix, out localMatrix))
                {
                    Log.W("CompiledBone[{0}/{1}]: Failed to invert BindPoseMatrix for root",
                        rootNode.Name, bone.BoneName);
                    localMatrix = Matrix4x4.Identity;
                }
            }
            else
            {
                // Child bone: compute local transform relative to parent
                // localTransform = parentWorldToBone * childBoneToWorld
                //                = parent.BindPoseMatrix * inverse(child.BindPoseMatrix)
                if (Matrix4x4.Invert(bone.BindPoseMatrix, out var childBoneToWorld))
                {
                    localMatrix = bone.ParentBone.BindPoseMatrix * childBoneToWorld;
                }
                else
                {
                    Log.W("CompiledBone[{0}/{1}]: Failed to invert BindPoseMatrix",
                        rootNode.Name, bone.BoneName);
                    localMatrix = Matrix4x4.Identity;
                }
            }

            // Transpose and swap axes for glTF coordinate system (Y-up)
            var matrix = SwapAxes(Matrix4x4.Transpose(localMatrix));
            if (!Matrix4x4.Decompose(matrix, out var scale, out var rotation, out var translation))
            {
                Log.W("CompiledBone[{0}/{1}]: BindPoseMatrix is not decomposable", rootNode.Name, bone.BoneName);
                scale = Vector3.One;
                rotation = Quaternion.Identity;
                translation = Vector3.Zero;
            }

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
            var nodeIndex = AddNode(boneNode);
            boneIndexToNodeIndex[boneIndex] = nodeIndex;
            controllerIdToNodeIndex[bone.ControllerID] = nodeIndex;
        }

        // Only set up parent-child relationships if we created new skeleton nodes
        if (!skeletonAlreadyExists)
        {
            for (int boneIndex = 0; boneIndex < skinningInfo.CompiledBones.Count; boneIndex++)
            {
                var bone = skinningInfo.CompiledBones[boneIndex];
                var nodeIndex = boneIndexToNodeIndex[boneIndex];

                if (bone.ParentBone == null)
                {
                    // Root bone
                    CurrentScene.Nodes.Add(nodeIndex);
                }
                else
                {
                    // Find parent bone index
                    var parentBoneIndex = skinningInfo.CompiledBones.IndexOf(bone.ParentBone);
                    if (parentBoneIndex >= 0 && boneIndexToNodeIndex.TryGetValue(parentBoneIndex, out var parentNodeIndex))
                    {
                        if (parentNodeIndex != nodeIndex)
                        {
                            Root.Nodes[parentNodeIndex].Children.Add(nodeIndex);
                        }
                        else
                        {
                            Log.W("Bone[{0}]: Self-reference detected, treating as root", bone.BoneName);
                            CurrentScene.Nodes.Add(nodeIndex);
                        }
                    }
                    else
                    {
                        Log.W("Bone[{0}]: Parent bone '{1}' not found in bone list, treating as root",
                            bone.BoneName, bone.ParentBone.BoneName);
                        CurrentScene.Nodes.Add(nodeIndex);
                    }
                }
            }
        }

        baseName = $"{rootNode.Name}/bone/joint";
        if (usePerSubsetExtraction)
        {
            // Ivo format: extract joints per-subset to match per-subset vertex extraction
            var subsets = geometrySubsets ?? [];
            var numberOfElements = subsets.Sum(x => x.NumVertices);

            joints =
                GetAccessorOrDefault(baseName, 0, numberOfElements)
                ?? AddAccessor(
                    baseName,
                    -1,
                    null,
                    skinningInfo is { HasIntToExtMapping: true, IntVertices: { } }
                        ? subsets
                            .SelectMany(subset => Enumerable
                                .Range(subset.FirstVertex, subset.NumVertices)
                                .Select(i => skinningInfo.IntVertices[skinningInfo.Ext2IntMap[i]])
                                .Select(x => new TypedVec4<ushort>(
                                    x.BoneMapping.BoneIndex[0], x.BoneMapping.BoneIndex[1], x.BoneMapping.BoneIndex[2], x.BoneMapping.BoneIndex[3])))
                            .ToArray()
                        : subsets
                            .SelectMany(subset => Enumerable
                                .Range(subset.FirstVertex, subset.NumVertices)
                                .Select(i => skinningInfo.BoneMappings[i])
                                .Select(x => new TypedVec4<ushort>(
                                    (ushort)x.BoneIndex[0], (ushort)x.BoneIndex[1], (ushort)x.BoneIndex[2],
                                    (ushort)x.BoneIndex[3])))
                            .ToArray());
        }
        else
        {
            // Traditional format: use raw skinning data (like USD renderer)
            var skinCount = skinningInfo.IntVertices is null ? skinningInfo.BoneMappings.Count : skinningInfo.Ext2IntMap.Count;

            joints =
                GetAccessorOrDefault(baseName, 0, skinCount)
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
        }

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

        // Track whether we're using per-subset extraction (Ivo format) or raw arrays (traditional)
        bool usePerSubsetExtraction = vertsUvs is not null;

        if (verts is not null || vertsUvs is not null)
        {
            if (verts is not null)
            {
                // Traditional format: use raw vertex array directly (like USD renderer)
                // No per-subset extraction needed - indices reference vertices directly
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
            else  // VertsUVs (Ivo format) - requires per-subset extraction
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
            if (usePerSubsetExtraction)
            {
                // Ivo format: extract normals per-subset
                accessors.Normal = normalsArray is null
                    ? null
                    : GetAccessorOrDefault(baseName, 0, normalsArray.Length)
                      ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                          (meshChunk.GeometryInfo.GeometrySubsets ?? [])
                              .SelectMany(subset => Enumerable
                                  .Range(subset.FirstVertex, subset.NumVertices)
                                  .Select(i => SwapAxesForPosition(normalsArray[i])))
                              .ToArray());
            }
            else
            {
                // Traditional format: use raw normals array
                accessors.Normal = normalsArray is null
                    ? null
                    : GetAccessorOrDefault(baseName, 0, normalsArray.Length)
                      ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                          normalsArray.Select(SwapAxesForPosition).ToArray());
            }

            baseName = $"{gltfNode.Name}/colors";
            if (usePerSubsetExtraction)
            {
                // Ivo format: extract colors per-subset
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
            }
            else
            {
                // Traditional format: use raw colors array
                accessors.Color0 = colors is null
                    ? null
                    : (GetAccessorOrDefault(baseName, 0, colors.Data.Length)
                        ?? AddAccessor(
                            baseName,
                            -1,
                            GltfBufferViewTarget.ArrayBuffer,
                            colors.Data.Select(c => new Vector4(c.R, c.G, c.B, c.A) / 255f).ToArray()));
            }

            baseName = $"${gltfNode.Name}/tangent";
        }

        baseName = $"${gltfNode.Name}/index";
        uint[] finalIndices;

        if (usePerSubsetExtraction)
        {
            // Ivo format: remap indices per-subset to reference extracted vertex positions.
            // Each subset's indices reference original vertices [FirstVertex, FirstVertex+NumVertices)
            // which get extracted to positions [currentOffset, currentOffset+NumVertices)
            var remappedIndices = new uint[indices.Data.Length];
            uint currentOffset = 0;

            foreach (var subset in meshChunk.GeometryInfo.GeometrySubsets ?? [])
            {
                var firstGlobalIndex = indices.Data[subset.FirstIndex];

                for (int i = 0; i < subset.NumIndices; i++)
                {
                    uint globalIndex = indices.Data[subset.FirstIndex + i];
                    uint localIndex = (uint)((globalIndex - firstGlobalIndex) + currentOffset);
                    remappedIndices[subset.FirstIndex + i] = localIndex;
                }

                currentOffset += (uint)subset.NumVertices;
            }
            finalIndices = remappedIndices;
        }
        else
        {
            // Traditional format: use raw indices directly (like USD renderer)
            finalIndices = indices.Data;
        }

        var indexBufferView = GetBufferViewOrDefault(baseName) ??
                      AddBufferView(baseName, finalIndices, GltfBufferViewTarget.ElementArrayBuffer);

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
