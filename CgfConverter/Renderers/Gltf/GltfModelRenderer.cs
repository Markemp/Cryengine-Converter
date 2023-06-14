using CgfConverter.Renderers.Gltf.Models;
using System;
using System.IO;

namespace CgfConverter.Renderers.Gltf;

public class GltfModelRenderer : BaseGltfRenderer, IRenderer
{
    private readonly CryEngine _cryEngine;

    public GltfModelRenderer(ArgsHandler argsHandler, CryEngine cryEngine, bool writeText, bool writeBinary)
        : base(argsHandler, Path.GetFileName(cryEngine.InputFile), writeText, writeBinary)
    {
        _cryEngine = cryEngine;
    }

    public int Render()
    {
        Reset("Scene");

        if (!CreateModelNode(out var node, _cryEngine))
            return Log.E<int>("Model could not be written.");

        CurrentScene.Nodes.Add(AddNode(node));

        Save(_cryEngine.InputFile);
        return 1;
    }

    public GltfRoot GenerateGltfObject()
    {
        Reset("Scene");

        if (!CreateModelNode(out var node, _cryEngine))
            throw new ApplicationException("Model could not be written.");

        CurrentScene.Nodes.Add(AddNode(node));
        
        return _root;
    }
}