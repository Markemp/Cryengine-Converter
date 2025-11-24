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
    private UsdSkelRoot CreateSkeleton()
    {
        var skelRoot = new UsdSkelRoot("Armature");
        var skeleton = new UsdSkeleton("Skeleton");

        // Build joint paths (hierarchical bone names)
        var jointPaths = new List<string>();
        var bonePathMap = new Dictionary<CompiledBone, string>();
        BuildJointPaths(_cryData.SkinningInfo.RootBone, "", jointPaths, bonePathMap);

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
    private void BuildJointPaths(CompiledBone bone, string parentPath, List<string> jointPaths, Dictionary<CompiledBone, string> bonePathMap)
    {
        if (bone == null)
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

    /// <summary>Gets bind transforms (inverse bind matrices) in joint order.</summary>
    private List<Matrix4x4> GetBindTransforms(List<string> jointPaths, Dictionary<CompiledBone, string> bonePathMap)
    {
        var bindTransforms = new List<Matrix4x4>();

        // Build reverse lookup from path to bone
        var pathToBone = bonePathMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        foreach (var jointPath in jointPaths)
        {
            if (pathToBone.TryGetValue(jointPath, out var bone))
            {
                // USD bindTransforms are inverse bind matrices (world-to-bone)
                // BindPoseMatrix has translation in M14/M24/M34 (column 4)
                // But USD needs translation in M41/M42/M43 (row 4)
                // Convert the matrix format
                bindTransforms.Add(MoveTranslationToRow4(bone.BindPoseMatrix));
            }
            else
            {
                // Fallback to identity matrix if bone not found
                bindTransforms.Add(Matrix4x4.Identity);
            }
        }

        return bindTransforms;
    }

    /// <summary>
    /// Converts a Matrix4x4 with translation in column 4 (M14/M24/M34)
    /// to USD format: move translation to row 4 (M41/M42/M43), keep rotation as-is.
    /// </summary>
    private static Matrix4x4 MoveTranslationToRow4(Matrix4x4 source)
    {
        return new Matrix4x4
        {
            M11 = source.M11,
            M12 = source.M12,
            M13 = source.M13,
            M14 = 0,
            M21 = source.M21,
            M22 = source.M22,
            M23 = source.M23,
            M24 = 0,
            M31 = source.M31,
            M32 = source.M32,
            M33 = source.M33,
            M34 = 0,
            M41 = source.M14,  // Move translation from column 4 to row 4
            M42 = source.M24,
            M43 = source.M34,
            M44 = source.M44
        };
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
                // Following the glTF approach for computing local transforms:
                // 1. Start with BindPoseMatrix (which is LocalTransformMatrix converted - worldToBone)
                // 2. If has parent: multiply by inverse of parent's BindPoseMatrix to make relative
                // 3. Invert the result to get the local boneToParent transform
                // 4. Convert to USD coordinate system

                Matrix4x4 matrix = bone.BindPoseMatrix;

                // If has parent, make transform relative to parent
                if (bone.ParentBone != null)
                {
                    if (Matrix4x4.Invert(bone.ParentBone.BindPoseMatrix, out var parentInv))
                    {
                        matrix *= parentInv;
                    }
                    else
                    {
                        Log.W($"Failed to invert parent BindPoseMatrix for bone {bone.BoneName}");
                    }
                }

                // Invert to get local transform (boneToParent instead of worldToBone)
                if (Matrix4x4.Invert(matrix, out var localTransform))
                {
                    // Convert to USD matrix format (transpose to get row 4 as translation)
                    localTransform = Matrix4x4.Transpose(localTransform);
                    restTransforms.Add(localTransform);
                }
                else
                {
                    Log.W($"Failed to invert matrix for bone {bone.BoneName}, using identity");
                    restTransforms.Add(Matrix4x4.Identity);
                }
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
        // Add SkelBindingAPI schema
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

        // Get skinning data from IntVertices
        var skinningInfo = _cryData.SkinningInfo;
        if (skinningInfo.IntVertices == null || skinningInfo.IntVertices.Count == 0)
            return;

        // Build joint indices and weights arrays
        var jointIndices = new List<int>();
        var jointWeights = new List<float>();
        int maxInfluences = 0;

        foreach (var intVertex in skinningInfo.IntVertices)
        {
            // Count non-zero weights to determine influences per vertex
            int influences = 0;
            for (int i = 0; i < 4; i++)
            {
                if (intVertex.BoneMapping.Weight[i] > 0)
                    influences++;
            }
            maxInfluences = Math.Max(maxInfluences, influences);

            // Add bone indices and weights (up to 4 influences per vertex)
            for (int i = 0; i < 4; i++)
            {
                jointIndices.Add(intVertex.BoneMapping.BoneIndex[i]);
                jointWeights.Add(intVertex.BoneMapping.Weight[i]); // Weights are already 0-1 range in MeshBoneMapping
            }
        }

        // Add skinning arrays with elementSize (influences per vertex)
        int elementSize = 4; // CryEngine uses up to 4 bone influences per vertex
        meshPrim.Attributes.Add(new UsdIntArray("primvars:skel:jointIndices", jointIndices, elementSize, "vertex"));
        meshPrim.Attributes.Add(new UsdFloatArray("primvars:skel:jointWeights", jointWeights, elementSize, "vertex"));

        // Add relationship to skeleton
        meshPrim.Attributes.Add(new UsdRelationship("skel:skeleton", "</root/Armature/Skeleton>"));
    }

    #endregion
}
