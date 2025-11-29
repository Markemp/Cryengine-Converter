using CgfConverter.Collada;
using CgfConverter.Models;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Controller;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Scene;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Transform;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Collada.Collada.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using static CgfConverter.Utilities.HelperMethods;

namespace CgfConverter.Renderers.Collada;

/// <summary>
/// ColladaModelRenderer partial class - Skeleton/controller and bone handling
/// </summary>
public partial class ColladaModelRenderer
{
    private void WriteLibrary_Controllers()
    {
        if (DaeObject.Library_Geometries.Geometry.Length != 0)
        {
            ColladaLibraryControllers libraryController = new();

            // There can be multiple controllers in the controller library.  But for Cryengine files, there is only one rig.
            // So if a rig exists, make that the controller.  This applies mostly to .chr files, which will have a rig and may have geometry.
            ColladaController controller = new() { ID = "Controller" };
            // Create the skin object and assign to the controller
            ColladaSkin skin = new()
            {
                source = "#" + DaeObject.Library_Geometries.Geometry[0].ID,
                Bind_Shape_Matrix = new ColladaFloatArrayString()
            };
            skin.Bind_Shape_Matrix.Value_As_String = CreateStringFromMatrix4x4(Matrix4x4.Identity);  // We will assume the BSM is the identity matrix for now

            // Create the 3 sources for this controller:  joints, bind poses, and weights
            skin.Source = new ColladaSource[3];

            // Populate the data.
            // Need to map the exterior vertices (geometry) to the int vertices.  Or use the Bone Map datastream if it exists (check HasBoneMapDatastream).
            #region Joints Source
            ColladaSource jointsSource = new()
            {
                ID = "Controller-joints",
                Name_Array = new ColladaNameArray()
                {
                    ID = "Controller-joints-array",
                    Count = _cryData.SkinningInfo.CompiledBones.Count,
                }
            };
            StringBuilder boneNames = new();
            for (int i = 0; i < _cryData.SkinningInfo.CompiledBones.Count; i++)
            {
                boneNames.Append(_cryData.SkinningInfo.CompiledBones[i].BoneName.Replace(' ', '_') + " ");
            }
            jointsSource.Name_Array.Value_Pre_Parse = boneNames.ToString().TrimEnd();
            jointsSource.Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = "#Controller-joints-array",
                    Count = (uint)_cryData.SkinningInfo.CompiledBones.Count,
                    Stride = 1
                }
            };
            skin.Source[0] = jointsSource;
            #endregion

            #region Bind Pose Array Source
            ColladaSource bindPoseArraySource = new()
            {
                ID = "Controller-bind_poses",
                Float_Array = new()
                {
                    ID = "Controller-bind_poses-array",
                    Count = _cryData.SkinningInfo.CompiledBones.Count * 16,
                    Value_As_String = GetBindPoseArray(_cryData.SkinningInfo.CompiledBones)
                },
                Technique_Common = new ColladaTechniqueCommonSource
                {
                    Accessor = new ColladaAccessor
                    {
                        Source = "#Controller-bind_poses-array",
                        Count = (uint)_cryData.SkinningInfo.CompiledBones.Count,
                        Stride = 16,
                    }
                }
            };
            bindPoseArraySource.Technique_Common.Accessor.Param = new ColladaParam[1];
            bindPoseArraySource.Technique_Common.Accessor.Param[0] = new ColladaParam
            {
                Name = "TRANSFORM",
                Type = "float4x4"
            };
            skin.Source[1] = bindPoseArraySource;
            #endregion

            #region Weights Source
            var skinningInfo = _cryData.SkinningInfo;
            var nodeChunk = _cryData.RootNode;

            ColladaSource weightArraySource = new()
            {
                ID = "Controller-weights",
                Technique_Common = new ColladaTechniqueCommonSource()
            };
            ColladaAccessor accessor = weightArraySource.Technique_Common.Accessor = new ColladaAccessor();

            weightArraySource.Float_Array = new ColladaFloatArray()
            {
                ID = "Controller-weights-array",
            };

            var numberOfWeights = skinningInfo.IntVertices is null ? skinningInfo.BoneMappings.Count : skinningInfo.Ext2IntMap.Count;
            var boneInfluenceCount =
                nodeChunk.MeshData?.GeometryInfo?.BoneMappings?.Data[0].BoneInfluenceCount == 8
                    ? 8
                    : 4;

            var boneMappingData = skinningInfo.IntVertices is null
                ? nodeChunk.MeshData?.GeometryInfo?.BoneMappings.Data.ToList()
                : skinningInfo.Ext2IntMap
                    .Select(x => skinningInfo.IntVertices[x])
                    .Select(x => x.BoneMapping)
                    .ToList();

            if (boneMappingData is null) return;

            StringBuilder weights = new();
            weightArraySource.Float_Array.Count = numberOfWeights;
            for (int i = 0; i < numberOfWeights; i++)
            {
                for (int j = 0; j < boneInfluenceCount; j++)
                {
                    weights.Append(boneMappingData[i].Weight[j].ToString() + " ");
                }
            }
            ;
            accessor.Count = (uint)(numberOfWeights * boneInfluenceCount);

            CleanNumbers(weights);
            weightArraySource.Float_Array.Value_As_String = weights.ToString().TrimEnd();
            // Add technique_common part.
            accessor.Source = "#Controller-weights-array";
            accessor.Stride = 1;
            accessor.Param = new ColladaParam[1];
            accessor.Param[0] = new ColladaParam
            {
                Name = "WEIGHT",
                Type = "float"
            };
            skin.Source[2] = weightArraySource;

            #endregion

            #region Joints
            skin.Joints = new ColladaJoints
            {
                Input = new ColladaInputUnshared[2]
            };
            skin.Joints.Input[0] = new ColladaInputUnshared
            {
                Semantic = new ColladaInputSemantic()
            };
            skin.Joints.Input[0].Semantic = ColladaInputSemantic.JOINT;
            skin.Joints.Input[0].source = "#Controller-joints";
            skin.Joints.Input[1] = new ColladaInputUnshared
            {
                Semantic = new ColladaInputSemantic()
            };
            skin.Joints.Input[1].Semantic = ColladaInputSemantic.INV_BIND_MATRIX;
            skin.Joints.Input[1].source = "#Controller-bind_poses";
            #endregion

            #region Vertex Weights
            ColladaVertexWeights vertexWeights = skin.Vertex_Weights = new();
            vertexWeights.Count = (int)numberOfWeights;
            skin.Vertex_Weights.Input = new ColladaInputShared[2];
            ColladaInputShared jointSemantic = skin.Vertex_Weights.Input[0] = new();
            jointSemantic.Semantic = ColladaInputSemantic.JOINT;
            jointSemantic.source = "#Controller-joints";
            jointSemantic.Offset = 0;
            ColladaInputShared weightSemantic = skin.Vertex_Weights.Input[1] = new();
            weightSemantic.Semantic = ColladaInputSemantic.WEIGHT;
            weightSemantic.source = "#Controller-weights";
            weightSemantic.Offset = 1;
            StringBuilder vCount = new();
            for (int i = 0; i < numberOfWeights; i++)
            {
                vCount.Append($"{boneInfluenceCount} ");
            }
            ;
            vertexWeights.VCount = new ColladaIntArrayString
            {
                Value_As_String = vCount.ToString().TrimEnd()
            };
            StringBuilder vertices = new();
            int index = 0;

            for (int i = 0; i < numberOfWeights; i++)
            {
                vertices.Append(boneMappingData[i].BoneIndex[0] + " " + index + " ");
                vertices.Append(boneMappingData[i].BoneIndex[1] + " " + (index + 1) + " ");
                vertices.Append(boneMappingData[i].BoneIndex[2] + " " + (index + 2) + " ");
                vertices.Append(boneMappingData[i].BoneIndex[3] + " " + (index + 3) + " ");
                if (boneInfluenceCount == 8)
                {
                    vertices.Append(boneMappingData[i].BoneIndex[4] + " " + (index + 4) + " ");
                    vertices.Append(boneMappingData[i].BoneIndex[5] + " " + (index + 5) + " ");
                    vertices.Append(boneMappingData[i].BoneIndex[6] + " " + (index + 6) + " ");
                    vertices.Append(boneMappingData[i].BoneIndex[7] + " " + (index + 7) + " ");
                    index += 4;
                }
                index += 4;
            }
            vertexWeights.V = new ColladaIntArrayString { Value_As_String = vertices.ToString().TrimEnd() };
            #endregion

            // create the extra element for the FCOLLADA profile
            controller.Extra = new ColladaExtra[1];
            controller.Extra[0] = new ColladaExtra
            {
                Technique = new ColladaTechnique[1]
            };
            controller.Extra[0].Technique[0] = new ColladaTechnique
            {
                profile = "FCOLLADA",
                UserProperties = "SkinController"
            };

            // Add the parts to their parents
            controller.Skin = skin;
            libraryController.Controller = new ColladaController[1];
            libraryController.Controller[0] = controller;
            DaeObject.Library_Controllers = libraryController;
        }
    }

    private ColladaNode CreateJointNode(CompiledBone bone)
    {
        var boneName = bone.BoneName.Replace(' ', '_');

        ColladaNode tmpNode = new()
        {
            ID = boneName,      // ID, name and sID must be the same or the controller can't seem to find the bone.
            Name = boneName,
            sID = boneName,
            Type = ColladaNodeType.JOINT
        };
        if (bone.ControllerID != -1 && bone.ControllerID != uint.MaxValue)
            controllerIdToBoneName.Add(bone.ControllerID, boneName);  // Use sanitized name to match joint node ID

        Matrix4x4 localMatrix = bone.LocalTransformMatrix.ConvertToTransformMatrix();

        // Use matrix with sid="transform" for Blender compatibility
        // Blender's Collada importer expects this exact SID for animation channel targeting
        tmpNode.Matrix =
        [
            new ColladaMatrix
            {
                sID = "transform",
                Value_As_String = CreateStringFromMatrix4x4(localMatrix)
            }
        ];

        // Recursively call this for each of the child bones to this bone.
        if (bone.NumberOfChildren > 0)
        {
            List<ColladaNode> childNodes = [];
            var allChildBones = _cryData.SkinningInfo?.GetChildBones(bone) ?? [];
            foreach (CompiledBone childBone in allChildBones)
            {
                childNodes.Add(CreateJointNode(childBone));
            }
            tmpNode.node = childNodes.ToArray();
        }
        return tmpNode;
    }

    /// <summary>Retrieves the worldtobone (bind pose matrix) for the bone.</summary>
    private static string GetBindPoseArray(List<CompiledBone> compiledBones)
    {
        StringBuilder value = new();
        for (int i = 0; i < compiledBones.Count; i++)
        {
            value.Append(CreateStringFromMatrix4x4(compiledBones[i].BindPoseMatrix) + " ");
        }
        return value.ToString().TrimEnd();
    }
}
