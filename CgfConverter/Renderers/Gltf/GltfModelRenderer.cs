using CgfConverter.Renderers.Gltf.Models;
using System.IO;
using System.Linq;

namespace CgfConverter.Renderers.Gltf;

public class GltfModelRenderer : BaseGltfRenderer, IRenderer
{
    public GltfModelRenderer(ArgsHandler argsHandler, CryEngine cryEngine, bool writeText, bool writeBinary)
        : base(argsHandler, Path.GetFileName(cryEngine.InputFile), writeText, writeBinary)
    {
        _cryData = cryEngine;
    }

    public int Render()
    {
        // Create the root object
        Reset("Scene");

        // For each root node in the crydata, add to the scene nodes.
        foreach (var cryNode in _cryData.Models[0].NodeMap.Values.Where(a => a.ParentNodeID == ~0).ToList())
        {
            // CurrentScene.Nodes has the index for the nodes in GltfRoot.Nodes
            // This is only for node chunks.  Bones are handled next.
            CurrentScene.Nodes.Add(CreateGltfNode(cryNode));
        }

        Save(_cryData.InputFile);
        return 1;
    }


    public GltfRoot GenerateGltfObject()
    {
        Reset("Scene");

        // For each root node in the crydata, add to the scene nodes.
        foreach (var cryNode in _cryData.Models[0].NodeMap.Values.Where(a => a.ParentNodeID == ~0).ToList())
        {
            // CurrentScene.Nodes has the index for the nodes in GltfRoot.Nodes
            CurrentScene.Nodes.Add(CreateGltfNode(cryNode));
        }
        
        return _gltfRoot;
    }
}