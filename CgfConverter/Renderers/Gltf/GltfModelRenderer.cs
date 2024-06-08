using CgfConverter.Renderers.Gltf.Models;
using System.IO;

namespace CgfConverter.Renderers.Gltf;

public class GltfModelRenderer : BaseGltfRenderer, IRenderer
{
    private readonly CryEngine _cryData;

    public GltfModelRenderer(ArgsHandler argsHandler, CryEngine cryEngine, bool writeText, bool writeBinary)
        : base(argsHandler, Path.GetFileName(cryEngine.InputFile), writeText, writeBinary)
    {
        _cryData = cryEngine;
    }

    public int Render()
    {
        GenerateGltfObject();
        Save(_cryData.InputFile);
        return 1;
    }

    public GltfRoot GenerateGltfObject()
    {
        // Create the root object.
        Reset("Scene");

        // For each root node in the crydata, add to the scene nodes.
        CreateGltfNodeInto(CurrentScene.Nodes, _cryData);

        return Root;
    }
}
