using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static CgfConverter.Utilities;

namespace CgfConverter.Renderers.USD;
public class UsdRenderer : IRenderer
{
    protected readonly ArgsHandler _args;
    protected readonly CryEngine _cryData;

    private readonly FileInfo usdOutputFile;
    private UsdSerializer usdSerializer;

    public UsdRenderer(ArgsHandler argsHandler, CryEngine cryEngine)
    {
        _args = argsHandler;
        _cryData = cryEngine;
        usdOutputFile = _args.FormatOutputFileName(".usda", _cryData.InputFile);
        usdSerializer = new UsdSerializer();
    }

    public int Render()
    {
        var usdDoc = GenerateUsdObject();

        Log(LogLevelEnum.Debug);
        Log(LogLevelEnum.Debug, "*** Starting Write USD ***");
        Log(LogLevelEnum.Debug);

        WriteUsdToFile(usdDoc);

        return 0;
    }

    public void WriteUsdToFile(UsdDoc usdDoc)
    {
        TextWriter writer = new StreamWriter(usdOutputFile.FullName);
        usdSerializer.Serialize(usdDoc, writer);
        writer.Close();
    }

    public UsdDoc GenerateUsdObject()
    {
        Log(LogLevelEnum.Debug, "Number of models: {0}", _cryData.Models.Count);
        for (int i = 0; i < _cryData.Models.Count; i++)
        {
            Log(LogLevelEnum.Debug, "\tNumber of nodes in model: {0}", _cryData.Models[i].NodeMap.Count);
        }

        // Create the usd doc
        var usdDoc = new UsdDoc { Header = new UsdHeader() };
        usdDoc.Prims.Add(new UsdXform("root"));

        // Create the node hierarchy as Xforms
        usdDoc.Prims[0].Children = CreateNodeHierarchy();

        return usdDoc;
    }

    private List<UsdPrim>? CreateNodeHierarchy()
    {
        // Get all the root nodes
        List<ChunkNode> rootNodes = _cryData.Models[0].NodeMap.Values.Where(a => a.ParentNodeID == ~0).ToList();
        List<UsdPrim> nodes = new();

        foreach (ChunkNode root in rootNodes)
        {
            Log(LogLevelEnum.Debug, "Root node: {0}", root.Name);
            nodes.Add(CreateNode(root));
        }

        return nodes;
    }

    private UsdXform CreateNode(ChunkNode node)
    {
        var xform = new UsdXform(node.Name);

        xform.Attributes.Add(new UsdMatrix4d("xformOp:transform", node.Transform));
        xform.Attributes.Add(new UsdXformOpOrder("xformOpOrder", ["xformOp:transform"], true));

        // If it's a geometry node, add a UsdMesh
        var modelIndex = node._model.IsIvoFile ? 1 : 0;
        ChunkNode geometryNode = _cryData.Models.Last().NodeMap.Values.Where(a => a.Name == node.Name).FirstOrDefault();

        var meshPrim = CreateMeshPrim(node);
        if (meshPrim is not null)
            xform.Children.Add(meshPrim);

        // Get all the children of the node
        var children = node.AllChildNodes;
        if (children is not null)
        {
            foreach (var childNode in children)
            {
                xform.Children.Add(CreateNode(childNode));
            }
        }

        return xform;
    }

    private UsdPrim? CreateMeshPrim(ChunkNode nodeChunk)
    {
        // Find the object node that corresponds with this node chunk.  If it's
        // a mesh chunk, create a UsdMesh prim.
        var modelIndex = nodeChunk._model.IsIvoFile ? 1 : 0;

        string nodeName = nodeChunk.Name;
        var geometryNodeChunk = _cryData.Models.Last().NodeMap.Values.Where(x => x.Name == nodeName).FirstOrDefault();
        var meshNodeId = geometryNodeChunk?.ObjectNodeID;

        var objectNodeChunkType = _cryData.Models.Last().ChunkMap[nodeChunk.ObjectNodeID].GetType();
        if (!objectNodeChunkType.Name.Contains("ChunkMesh"))
            return null;
        var meshChunk = (ChunkMesh)_cryData.Models.Last().ChunkMap[nodeChunk.ObjectNodeID];

        if (meshChunk.MeshSubsetsData == 0
            || meshChunk.NumVertices == 0
            || nodeChunk._model.ChunkMap[meshChunk.MeshSubsetsData].ID == 0)
            return null;




        UsdMesh meshPrim = new(nodeChunk.Name);
        //UsdAttribute doubleSided = new("doubleSided", node.DoubleSided);
        //meshPrim.Attributes.Add()


        return meshPrim;
    }
}
