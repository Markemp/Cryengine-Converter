using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
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
    #region Skeleton Methods

    /// <summary>Creates a USD skeleton hierarchy for skinned meshes.</summary>
    /// <param name="controllerIdToJointPath">Output mapping from bone controller IDs to USD joint paths for animation binding.</param>
    /// <param name="jointPaths">Output list of joint paths in order.</param>
    /// <param name="bonePathMap">Output mapping from CompiledBone to joint path.</param>
    /// <returns>The SkelRoot prim containing the skeleton.</returns>
    private UsdSkelRoot CreateSkeleton(
        out Dictionary<uint, string> controllerIdToJointPath,
        out List<string> jointPaths,
        out Dictionary<CompiledBone, string> bonePathMap)
    {
        var skelRoot = new UsdSkelRoot("Armature");
        var skeleton = new UsdSkeleton("Skeleton");

        // Build joint paths (hierarchical bone names)
        jointPaths = new List<string>();
        bonePathMap = new Dictionary<CompiledBone, string>();
        BuildJointPaths(_cryData.SkinningInfo.RootBone, "", jointPaths, bonePathMap);

        // Build controller ID to joint path mapping for animation binding
        controllerIdToJointPath = new Dictionary<uint, string>();
        foreach (var kvp in bonePathMap)
        {
            controllerIdToJointPath[kvp.Key.ControllerID] = kvp.Value;
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
    private void BuildJointPaths(CompiledBone? bone, string parentPath, List<string> jointPaths, Dictionary<CompiledBone, string> bonePathMap)
    {
        if (bone == null)
            return;

        // Cycle detection: skip bones we've already processed
        if (bonePathMap.ContainsKey(bone))
            return;

        // Clean bone name for USD compliance
        string cleanName = CleanPathString(bone.BoneName ?? "bone");

        // Build the full path for this bone
        string bonePath = string.IsNullOrEmpty(parentPath)
            ? cleanName
            : $"{parentPath}/{cleanName}";

        jointPaths.Add(bonePath);
        bonePathMap[bone] = bonePath;

        // Recursively process children
        var childBones = _cryData.SkinningInfo.GetChildBones(bone);
        foreach (var childBone in childBones)
        {
            BuildJointPaths(childBone, bonePath, jointPaths, bonePathMap);
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
                // Then transpose to convert from column-major to row-major for USD
                if (Matrix4x4.Invert(bone.BindPoseMatrix, out var boneToWorld))
                {
                    bindTransforms.Add(Matrix4x4.Transpose(boneToWorld));
                }
                else
                {
                    Log.W($"Failed to invert BindPoseMatrix for bone {bone.BoneName}, using identity");
                    bindTransforms.Add(Matrix4x4.Identity);
                }
            }
            else
            {
                // Fallback to identity matrix if bone not found
                bindTransforms.Add(Matrix4x4.Identity);
            }
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
                    {
                        restMatrix = boneToWorld;
                    }
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
                    {
                        restMatrix = bone.ParentBone.BindPoseMatrix * childBoneToWorld;
                    }
                    else
                    {
                        Log.W($"Failed to invert BindPoseMatrix for bone {bone.BoneName}");
                        restMatrix = Matrix4x4.Identity;
                    }
                }

                // Transpose to convert from column-major to row-major for USD
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
    private void AddSkinningAttributes(UsdMesh meshPrim, ChunkNode nodeChunk)
    {
        // Get skinning data
        var skinningInfo = _cryData.SkinningInfo;

        // Build joint indices and weights arrays FIRST to check if we have actual skinning data
        // Use Ext2IntMap if available to map external (mesh) vertices to internal (skinning) vertices
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
                    jointIndices.Add(intVertex.BoneMapping.BoneIndex[i]);
                    jointWeights.Add(intVertex.BoneMapping.Weight[i]);
                }
            }
        }
        else if (skinningInfo.BoneMappings != null)
        {
            // Fall back to BoneMappings for simpler skinning without IntVertices
            foreach (var boneMapping in skinningInfo.BoneMappings)
            {
                for (int i = 0; i < 4; i++)
                {
                    jointIndices.Add(boneMapping.BoneIndex[i]);
                    jointWeights.Add(boneMapping.Weight[i]);
                }
            }
        }
        else
        {
            // No skinning data available - don't add SkelBindingAPI
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
