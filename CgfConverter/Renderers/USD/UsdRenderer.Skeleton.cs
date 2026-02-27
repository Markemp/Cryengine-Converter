using CgfConverter.CryEngineCore;
using CgfConverter.Models.Structs;
using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
using CgfConverter.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CgfConverter.Renderers.USD;

/// <summary>
/// UsdRenderer partial class - Skeletal animation and skinning
/// </summary>
public partial class UsdRenderer
{
    #region Matrix Conversion

    /// <summary>
    /// Converts a CryEngine matrix to USD format by moving translation from column 4 to row 4.
    /// CryEngine stores translation in (M14, M24, M34), USD expects it in (M41, M42, M43).
    /// Unlike Transpose(), this preserves the rotation orientation.
    /// </summary>
    private static Matrix4x4 CryEngineToUsdMatrix(Matrix4x4 cryMatrix)
    {
        return new Matrix4x4(
            cryMatrix.M11, cryMatrix.M12, cryMatrix.M13, 0,
            cryMatrix.M21, cryMatrix.M22, cryMatrix.M23, 0,
            cryMatrix.M31, cryMatrix.M32, cryMatrix.M33, 0,
            cryMatrix.M14, cryMatrix.M24, cryMatrix.M34, 1);
    }

    #endregion

    #region Skeleton Methods

    /// <summary>Creates a USD skeleton hierarchy for skinned meshes.</summary>
    /// <param name="controllerIdToJointPath">Output mapping from bone controller IDs to USD joint paths for animation binding.</param>
    /// <param name="jointPaths">Output list of joint paths in order.</param>
    /// <param name="bonePathMap">Output mapping from CompiledBone to joint path.</param>
    /// <param name="compiledBoneIndexToJointIndex">Output mapping from CompiledBones array index to jointPaths array index.</param>
    /// <returns>The SkelRoot prim containing the skeleton.</returns>
    private UsdSkelRoot CreateSkeleton(
        out Dictionary<uint, string> controllerIdToJointPath,
        out List<string> jointPaths,
        out Dictionary<CompiledBone, string> bonePathMap,
        out int[] compiledBoneIndexToJointIndex)
    {
        var skelRoot = new UsdSkelRoot("Armature");
        var skeleton = new UsdSkeleton("Skeleton");

        // Build joint paths (hierarchical bone names)
        jointPaths = new List<string>();
        bonePathMap = new Dictionary<CompiledBone, string>();
        var usedPaths = new HashSet<string>();
        BuildJointPaths(_cryData.SkinningInfo.RootBone, "", jointPaths, bonePathMap, usedPaths);

        // Build controller ID to joint path mapping for animation binding
        // Add both stored controller IDs and computed CRC32 hashes of bone names
        // (CAF animations may use either for matching)
        controllerIdToJointPath = new Dictionary<uint, string>();
        foreach (var kvp in bonePathMap)
        {
            var bone = kvp.Key;
            var jointPath = kvp.Value;

            // Add stored controller ID if valid
            if (bone.ControllerID != 0xFFFFFFFF && bone.ControllerID != 0)
                controllerIdToJointPath[bone.ControllerID] = jointPath;

            // Also add CRC32 hash of bone name for CAF matching
            // Different games use different conventions - add both original case and lowercase CRC32
            if (!string.IsNullOrEmpty(bone.BoneName))
            {
                // Original case CRC32 (used by ArcheAge)
                var crc32Original = Crc32CryEngine.Compute(bone.BoneName);
                Log.D($"Bone '{bone.BoneName}' -> CRC32 = 0x{crc32Original:X08}");
                controllerIdToJointPath.TryAdd(crc32Original, jointPath);

                // Lowercase CRC32 (used by some other games)
                var crc32Lower = Crc32CryEngine.Compute(bone.BoneName.ToLowerInvariant());
                if (crc32Lower != crc32Original)
                    controllerIdToJointPath.TryAdd(crc32Lower, jointPath);
            }
        }

        // Build mapping from CompiledBones array index to jointPaths array index
        // This is critical because:
        // - IntSkinVertex.BoneMapping.BoneIndex contains indices into CompiledBones array order
        // - USD skel:jointIndices expects indices into jointPaths array order
        // - These orders may differ due to depth-first vs linear traversal
        var compiledBones = _cryData.SkinningInfo.CompiledBones;
        compiledBoneIndexToJointIndex = new int[compiledBones.Count];

        // Build reverse lookup from path to jointPaths index
        var pathToJointIndex = new Dictionary<string, int>();
        for (int i = 0; i < jointPaths.Count; i++)
        {
            pathToJointIndex[jointPaths[i]] = i;
        }

        // Map each CompiledBone index to its corresponding jointPaths index
        for (int compiledIndex = 0; compiledIndex < compiledBones.Count; compiledIndex++)
        {
            var bone = compiledBones[compiledIndex];
            if (bonePathMap.TryGetValue(bone, out var path) && pathToJointIndex.TryGetValue(path, out var jointIndex))
                compiledBoneIndexToJointIndex[compiledIndex] = jointIndex;
            else
            {
                // Fallback to same index if bone wasn't found (shouldn't happen with valid data)
                Log.W($"Bone at index {compiledIndex} ({bone?.BoneName}) not found in joint paths, using index as-is");
                compiledBoneIndexToJointIndex[compiledIndex] = compiledIndex;
            }
        }

        // Add joint names array
        skeleton.Attributes.Add(new UsdTokenArray("joints", jointPaths, isUniform: true));

        // Add bind transforms (inverse bind pose matrices)
        var bindTransforms = GetBindTransforms(jointPaths, bonePathMap);
        skeleton.Attributes.Add(new UsdMatrix4dArray("bindTransforms", bindTransforms, isUniform: true));

        // Add rest transforms (local transform matrices)
        var restTransforms = GetRestTransforms(jointPaths, bonePathMap);
        skeleton.Attributes.Add(new UsdMatrix4dArray("restTransforms", restTransforms, isUniform: true));

        skelRoot.Children.Add(skeleton);
        return skelRoot;
    }

    /// <summary>Recursively builds joint path strings in USD format (e.g., "Bip01/bip_01_Pelvis/bip_01_Spine").</summary>
    private void BuildJointPaths(CompiledBone? bone, string parentPath, List<string> jointPaths, Dictionary<CompiledBone, string> bonePathMap, HashSet<string> usedPaths)
    {
        if (bone == null) return;

        // Cycle detection: skip bones we've already processed
        if (bonePathMap.ContainsKey(bone)) return;

        // Clean bone name for USD compliance
        string cleanName = CleanPathString(bone.BoneName ?? "bone");

        // Build the full path for this bone
        string bonePath = string.IsNullOrEmpty(parentPath)
            ? cleanName
            : $"{parentPath}/{cleanName}";

        // Deduplicate: if path already exists, append numeric suffix
        if (!usedPaths.Add(bonePath))
        {
            int suffix = 1;
            while (!usedPaths.Add($"{bonePath}_{suffix}"))
                suffix++;
            bonePath = $"{bonePath}_{suffix}";
            Log.W($"Duplicate bone path detected for '{bone.BoneName}', renamed to '{bonePath}'");
        }

        jointPaths.Add(bonePath);
        bonePathMap[bone] = bonePath;

        // Recursively process children
        var childBones = _cryData.SkinningInfo.GetChildBones(bone);
        foreach (var childBone in childBones)
        {
            BuildJointPaths(childBone, bonePath, jointPaths, bonePathMap, usedPaths);
        }
    }

    /// <summary>Gets bind transforms (world-space bone transforms) in joint order.</summary>
    private List<Matrix4x4> GetBindTransforms(List<string> jointPaths, Dictionary<CompiledBone, string> bonePathMap)
    {
        var bindTransforms = new List<Matrix4x4>();

        // Build reverse lookup from path to bone
        var pathToBone = bonePathMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        foreach (var jointPath in jointPaths)
        {
            if (pathToBone.TryGetValue(jointPath, out var bone))
            {
                // USD bindTransforms are world-space transforms (bone-to-world)
                // CryEngine BindPoseMatrix is worldToBone, so we need to invert it
                // Transpose converts from CryEngine convention (translation in column 4)
                // to USD convention (translation in row 4)
                if (Matrix4x4.Invert(bone.BindPoseMatrix, out var boneToWorld))
                    bindTransforms.Add(Matrix4x4.Transpose(boneToWorld));
                else
                {
                    Log.W($"Failed to invert BindPoseMatrix for bone {bone.BoneName}, using identity");
                    bindTransforms.Add(Matrix4x4.Identity);
                }
            }
            else
                bindTransforms.Add(Matrix4x4.Identity); // Fallback to identity matrix if bone not found
        }

        return bindTransforms;
    }

    /// <summary>Gets rest transforms (local-space bone transforms) in joint order.</summary>
    private List<Matrix4x4> GetRestTransforms(List<string> jointPaths, Dictionary<CompiledBone, string> bonePathMap)
    {
        var restTransforms = new List<Matrix4x4>();

        // Build reverse lookup from path to bone
        var pathToBone = bonePathMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        foreach (var jointPath in jointPaths)
        {
            if (pathToBone.TryGetValue(jointPath, out var bone))
            {
                // restTransforms are local-space transforms (bone relative to parent)
                // For root bones: use world transform (boneToWorld)
                // For child bones: compute relative to parent

                Matrix4x4 restMatrix;

                if (bone.ParentBone == null)
                {
                    // Root bone: local = world, invert BindPoseMatrix to get boneToWorld
                    if (Matrix4x4.Invert(bone.BindPoseMatrix, out var boneToWorld))
                        restMatrix = boneToWorld;
                    else
                    {
                        Log.W($"Failed to invert BindPoseMatrix for root bone {bone.BoneName}");
                        restMatrix = Matrix4x4.Identity;
                    }
                }
                else
                {
                    // Child bone: compute local transform relative to parent
                    // localTransform = parentWorldToBone * childBoneToWorld
                    //                = parent.BindPoseMatrix * inverse(child.BindPoseMatrix)
                    if (Matrix4x4.Invert(bone.BindPoseMatrix, out var childBoneToWorld))
                        restMatrix = bone.ParentBone.BindPoseMatrix * childBoneToWorld;
                    else
                    {
                        Log.W($"Failed to invert BindPoseMatrix for bone {bone.BoneName}");
                        restMatrix = Matrix4x4.Identity;
                    }
                }

                // Debug: log turret_arm rest transform (CtrlID = 0x9384FC75)
                if (bone.ControllerID == 0x9384FC75)
                {
                    Log.I($"USD skeleton turret_arm restMatrix (CryEngine format):");
                    Log.I($"  [{restMatrix.M11:F6}, {restMatrix.M12:F6}, {restMatrix.M13:F6}, {restMatrix.M14:F6}]");
                    Log.I($"  [{restMatrix.M21:F6}, {restMatrix.M22:F6}, {restMatrix.M23:F6}, {restMatrix.M24:F6}]");
                    Log.I($"  [{restMatrix.M31:F6}, {restMatrix.M32:F6}, {restMatrix.M33:F6}, {restMatrix.M34:F6}]");
                    Log.I($"  [{restMatrix.M41:F6}, {restMatrix.M42:F6}, {restMatrix.M43:F6}, {restMatrix.M44:F6}]");
                    if (Matrix4x4.Decompose(restMatrix, out var s, out var r, out var t))
                    {
                        Log.I($"USD skeleton turret_arm rest rotation (from restMatrix): ({r.X:F6}, {r.Y:F6}, {r.Z:F6}, {r.W:F6})");
                        var angle = 2 * Math.Acos(Math.Clamp(r.W, -1, 1)) * 180 / Math.PI;
                        Log.I($"USD skeleton turret_arm rest rotation angle: {angle:F2}°");
                    }
                }

                // Transpose converts from CryEngine convention (translation in column 4)
                // to USD convention (translation in row 4)
                restTransforms.Add(Matrix4x4.Transpose(restMatrix));
            }
            else
            {
                Log.W($"Bone not found for joint path: {jointPath}");
                restTransforms.Add(Matrix4x4.Identity);
            }
        }

        return restTransforms;
    }

    /// <summary>Adds skinning attributes to a mesh prim for skeletal animation.</summary>
    /// <param name="meshPrim">The mesh prim to add skinning to.</param>
    /// <param name="nodeChunk">The node chunk containing mesh data.</param>
    /// <param name="subsets">Geometry subsets for per-subset vertex extraction (Ivo format).</param>
    private void AddSkinningAttributes(UsdMesh meshPrim, ChunkNode nodeChunk, IEnumerable<MeshSubset>? subsets = null)
    {
        // Get skinning data
        var skinningInfo = _cryData.SkinningInfo;

        // Ensure we have the bone index mapping (set up by CreateSkeleton)
        if (_compiledBoneIndexToJointIndex is null)
        {
            Log.W("Skinning attributes requested but bone index mapping not initialized");
            return;
        }

        // Build joint indices and weights arrays FIRST to check if we have actual skinning data
        // Use Ext2IntMap if available to map external (mesh) vertices to internal (skinning) vertices
        // IMPORTANT: BoneIndex values in skinning data are indices into CompiledBones array,
        // but USD expects indices into the joints array (jointPaths). We must remap using
        // _compiledBoneIndexToJointIndex to fix bone-to-vertex mapping.
        var jointIndices = new List<int>();
        var jointWeights = new List<float>();

        if (skinningInfo.HasIntToExtMapping && skinningInfo.IntVertices != null)
        {
            // Use Ext2IntMap to properly map skinning data to mesh vertices
            foreach (var extIndex in skinningInfo.Ext2IntMap)
            {
                var intVertex = skinningInfo.IntVertices[extIndex];

                // Add bone indices and weights (up to 4 influences per vertex)
                for (int i = 0; i < 4; i++)
                {
                    // Remap from CompiledBones index to jointPaths index
                    int compiledBoneIndex = intVertex.BoneMapping.BoneIndex[i];
                    int jointIndex = compiledBoneIndex < _compiledBoneIndexToJointIndex.Length
                        ? _compiledBoneIndexToJointIndex[compiledBoneIndex]
                        : compiledBoneIndex;
                    jointIndices.Add(jointIndex);
                    jointWeights.Add(intVertex.BoneMapping.Weight[i]);
                }
            }
        }
        else if (skinningInfo.BoneMappings != null)
        {
            // For Ivo format with per-subset vertex extraction, extract bone mappings per-subset
            // to match the per-subset vertex extraction in CreateMeshPrim
            if (subsets != null)
            {
                foreach (var subset in subsets)
                {
                    for (int vertexIndex = subset.FirstVertex; vertexIndex < subset.FirstVertex + subset.NumVertices; vertexIndex++)
                    {
                        if (vertexIndex >= skinningInfo.BoneMappings.Count)
                        {
                            Log.W($"Bone mapping index {vertexIndex} out of bounds (count: {skinningInfo.BoneMappings.Count})");
                            // Add default mapping (bone 0 with weight 1)
                            jointIndices.AddRange([0, 0, 0, 0]);
                            jointWeights.AddRange([1.0f, 0, 0, 0]);
                            continue;
                        }

                        var boneMapping = skinningInfo.BoneMappings[vertexIndex];
                        for (int i = 0; i < 4; i++)
                        {
                            // Remap from CompiledBones index to jointPaths index
                            int compiledBoneIndex = boneMapping.BoneIndex[i];
                            int jointIndex = compiledBoneIndex < _compiledBoneIndexToJointIndex.Length
                                ? _compiledBoneIndexToJointIndex[compiledBoneIndex]
                                : compiledBoneIndex;
                            jointIndices.Add(jointIndex);
                            jointWeights.Add(boneMapping.Weight[i]);
                        }
                    }
                }
            }
            else
            {
                // Traditional format: use all bone mappings in order
                foreach (var boneMapping in skinningInfo.BoneMappings)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        // Remap from CompiledBones index to jointPaths index
                        int compiledBoneIndex = boneMapping.BoneIndex[i];
                        int jointIndex = compiledBoneIndex < _compiledBoneIndexToJointIndex.Length
                            ? _compiledBoneIndexToJointIndex[compiledBoneIndex]
                            : compiledBoneIndex;
                        jointIndices.Add(jointIndex);
                        jointWeights.Add(boneMapping.Weight[i]);
                    }
                }
            }
        }
        else
        {
            // No skinning data available
            return;
        }

        // Only add SkelBindingAPI if we have actual skinning data
        var skelBindingApi = new Dictionary<string, object> { ["apiSchemas"] = "[\"SkelBindingAPI\"]" };

        // Merge with existing properties or create new
        if (meshPrim.Properties != null && meshPrim.Properties.Count > 0)
        {
            // Update existing properties to include SkelBindingAPI
            meshPrim.Properties[0].Properties["apiSchemas"] = "[\"MaterialBindingAPI\", \"SkelBindingAPI\"]";
        }
        else
            meshPrim.Properties = [new UsdProperty(skelBindingApi, true)];

        // Add geomBindTransform (usually identity matrix)
        meshPrim.Attributes.Add(new UsdMatrix4d("primvars:skel:geomBindTransform", Matrix4x4.Identity));

        // Add skinning arrays with elementSize (influences per vertex)
        int elementSize = 4; // CryEngine uses up to 4 bone influences per vertex
        meshPrim.Attributes.Add(new UsdIntArray("primvars:skel:jointIndices", jointIndices, elementSize, "vertex"));
        meshPrim.Attributes.Add(new UsdFloatArray("primvars:skel:jointWeights", jointWeights, elementSize, "vertex"));

        // Add relationship to skeleton
        meshPrim.Attributes.Add(new UsdRelationship("skel:skeleton", "</root/Armature/Skeleton>"));
    }

    #endregion
}
