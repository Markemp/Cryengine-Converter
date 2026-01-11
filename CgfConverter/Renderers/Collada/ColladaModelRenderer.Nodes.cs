using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Controller;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Scene;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Transform;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Materials;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Technique_Common;
using CgfConverter.Renderers.Collada.Collada.Enums;
using System.Collections.Generic;
using System.Linq;

namespace CgfConverter.Renderers.Collada;

/// <summary>
/// ColladaModelRenderer partial class - Visual scene and node hierarchy creation
/// </summary>
public partial class ColladaModelRenderer
{
    public void WriteLibrary_VisualScenes()
    {
        ColladaLibraryVisualScenes libraryVisualScenes = new();

        List<ColladaVisualScene> visualScenes = [];
        ColladaVisualScene visualScene = new();
        List<ColladaNode> nodes = [];

        // THERE CAN BE MULTIPLE ROOT NODES IN EACH FILE!  Check to see if the parentnodeid ~0 and be sure to add a node for it.
        List<ColladaNode> positionNodes = [];
        List<ChunkNode> positionRoots = _cryData.Nodes.Where(a => a.ParentNodeID == ~0).ToList();
        foreach (ChunkNode root in positionRoots)
        {
            positionNodes.Add(CreateNode(root, false));
        }
        nodes.AddRange(positionNodes.ToArray());

        visualScene.Node = nodes.ToArray();
        visualScene.ID = "Scene";
        visualScenes.Add(visualScene);

        libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
        DaeObject.Library_Visual_Scene = libraryVisualScenes;
    }

    public void WriteLibrary_VisualScenesWithSkeleton()
    {
        ColladaLibraryVisualScenes libraryVisualScenes = new();

        List<ColladaVisualScene> visualScenes = [];
        ColladaVisualScene visualScene = new();
        List<ColladaNode> nodes = [];

        List<ChunkNode> positionRoots = _cryData.Nodes.Where(a => a.ParentNodeID == ~0).ToList();

        // Check to see if there is a CompiledBones chunk.  If so, add a Node.
        if (_cryData.Chunks.Any(a => a.ChunkType == ChunkType.CompiledBones ||
            a.ChunkType == ChunkType.CompiledBonesSC ||
            a.ChunkType == ChunkType.CompiledBones_Ivo ||
            a.ChunkType == ChunkType.CompiledBones_Ivo2))
        {
            ColladaNode boneNode = CreateJointNode(_cryData.SkinningInfo.RootBone);
            nodes.Add(boneNode);
        }

        var hasGeometry = _cryData.Nodes.Any(x => x.MeshData is not null);

        if (hasGeometry)
        {
            foreach (var node in positionRoots)
            {
                var colladaNode = CreateNode(node, true);
                colladaNode.Instance_Controller = new ColladaInstanceController[1];
                colladaNode.Instance_Controller[0] = new ColladaInstanceController
                {
                    URL = "#Controller",
                    Skeleton = new ColladaSkeleton[1]
                };

                var skeleton = colladaNode.Instance_Controller[0].Skeleton[0] = new ColladaSkeleton();
                skeleton.Value = $"#{_cryData.SkinningInfo.CompiledBones[0].BoneName}".Replace(' ', '_');
                colladaNode.Instance_Controller[0].Bind_Material = new ColladaBindMaterial[1];
                ColladaBindMaterial bindMaterial = colladaNode.Instance_Controller[0].Bind_Material[0] = new ColladaBindMaterial();

                // Create an Instance_Material for each material
                bindMaterial.Technique_Common = new ColladaTechniqueCommonBindMaterial();
                colladaNode.Instance_Controller[0].Bind_Material[0].Technique_Common.Instance_Material = CreateInstanceMaterials(node);

                foreach (ChunkNode child in node.Children)
                    CreateChildNodes(child, true);

                nodes.Add(colladaNode);
            }
        }

        visualScene.Node = nodes.ToArray();
        visualScene.ID = "Scene";
        visualScenes.Add(visualScene);

        libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
        DaeObject.Library_Visual_Scene = libraryVisualScenes;
    }

    private ColladaInstanceMaterialGeometry[] CreateInstanceMaterials(ChunkNode node)
    {
        List<ColladaInstanceMaterialGeometry> instanceMaterials = [];

        var matIndices = node.MeshData?.GeometryInfo?.GeometrySubsets?.Select(x => x.MatID) ?? [];

        foreach (var index in matIndices)
        {
            var matName = GetMaterialName(node.MaterialFileName, node.Materials.SubMaterials[index].Name);
            ColladaInstanceMaterialGeometry instanceMaterial = new();
            instanceMaterial.Target = $"#{matName}-material";
            instanceMaterial.Symbol = $"{matName}-material";
            instanceMaterials.Add(instanceMaterial);
        }

        return instanceMaterials.ToArray();
    }

    private ColladaNode CreateNode(ChunkNode nodeChunk, bool isControllerNode)
    {
        ColladaNode colladaNode = new();

        string nodeName = nodeChunk.Name;
        int nodeID = nodeChunk.ID;

        if (nodeChunk.ChunkHelper is not null || nodeChunk.MeshData?.GeometryInfo is null)
            colladaNode = CreateSimpleNode(nodeChunk, isControllerNode);
        else
        {
            ColladaGeometry geometryLibraryObject = DaeObject.Library_Geometries.Geometry.Where(a => a.Name == nodeChunk.Name).FirstOrDefault();
            ChunkMesh geometryMesh = nodeChunk.MeshData;
            colladaNode = CreateGeometryNode(nodeChunk, geometryMesh, isControllerNode);
        }

        colladaNode.node = CreateChildNodes(nodeChunk, isControllerNode);
        return colladaNode;
    }

    /// <summary>This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics </summary>
    private ColladaNode CreateSimpleNode(ChunkNode nodeChunk, bool isControllerNode)
    {
        // This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics
        ColladaNode colladaNode = new()
        {
            Type = ColladaNodeType.NODE,
            Name = nodeChunk.Name,
            ID = nodeChunk.Name
        };

        ColladaMatrix matrix = new()
        {
            sID = "transform",
            Value_As_String = CreateStringFromMatrix4x4(nodeChunk.LocalTransform)
        };
        colladaNode.Matrix = new ColladaMatrix[1] { matrix };

        colladaNode.node = CreateChildNodes(nodeChunk, isControllerNode);
        return colladaNode;
    }

    /// <summary>Used by CreateNode and CreateSimpleNodes to create all the child nodes for the given node.</summary>
    private ColladaNode[]? CreateChildNodes(ChunkNode nodeChunk, bool isControllerNode)
    {
        List<ColladaNode> childNodes = [];
        foreach (ChunkNode childNodeChunk in nodeChunk.Children)
        {
            if (_args.IsNodeNameExcluded(childNodeChunk.Name))
            {
                Log.D($"Excluding child node {childNodeChunk.Name}");
                continue;
            }

            ColladaNode childNode = CreateNode(childNodeChunk, isControllerNode);
            childNodes.Add(childNode);
        }
        return childNodes.ToArray();
    }

    private ColladaNode CreateGeometryNode(ChunkNode nodeChunk, ChunkMesh tmpMeshChunk, bool isControllerNode)
    {
        ColladaNode colladaNode = new();
        var meshSubsets = nodeChunk.MeshData.GeometryInfo.GeometrySubsets;
        var nodeType = ColladaNodeType.NODE;
        colladaNode.Type = nodeType;
        colladaNode.Name = nodeChunk.Name;
        colladaNode.ID = nodeChunk.Name;

        // Make the lists necessary for this Node.
        List<ColladaBindMaterial> bindMaterials = [];
        List<ColladaMatrix> matrices = [];
        ColladaMatrix matrix = new()
        {
            Value_As_String = CreateStringFromMatrix4x4(nodeChunk.LocalTransform),
            sID = "transform"
        };

        matrices.Add(matrix);          // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
        colladaNode.Matrix = matrices.ToArray();

        // Each node will have one instance geometry, although it could be a list.
        if (!isControllerNode)
        {
            List<ColladaInstanceGeometry> instanceGeometries = [];
            ColladaInstanceGeometry instanceGeometry = new()
            {
                Name = nodeChunk.Name,
                URL = "#" + nodeChunk.Name + "-mesh"  // this is the ID of the geometry.
            };
            ColladaBindMaterial bindMaterial = new()
            {
                Technique_Common = new ColladaTechniqueCommonBindMaterial
                {
                    Instance_Material = new ColladaInstanceMaterialGeometry[meshSubsets.Count]
                }
            };
            bindMaterials.Add(bindMaterial);
            instanceGeometry.Bind_Material = bindMaterials.ToArray();
            instanceGeometries.Add(instanceGeometry);

            colladaNode.Instance_Geometry = instanceGeometries.ToArray();
            colladaNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material = CreateInstanceMaterials(nodeChunk);
        }

        return colladaNode;
    }

    /// <summary> Adds the scene element to the Collada document. </summary>
    private void WriteScene()
    {
        ColladaScene scene = new();
        ColladaInstanceVisualScene visualScene = new()
        {
            URL = "#Scene",
            Name = "Scene"
        };
        scene.Visual_Scene = visualScene;
        DaeObject.Scene = scene;
    }
}
