using CgfConverter.Renderers.Gltf.Models;
using System.IO;

namespace CgfConverter.Renderers.Gltf;

public class GltfModelRenderer : BaseGltfRenderer, IRenderer
{
    private readonly CryEngine _cryData;

    public GltfModelRenderer(Args argsHandler, CryEngine cryEngine)
        : base(argsHandler, Path.GetFileName(cryEngine.InputFile))
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

        // Only omit skins if there's no skinning data available
        bool omitSkins = _cryData.SkinningInfo is not { HasSkinningInfo: true };

        // For each root node in the crydata, add to the scene nodes.
        CreateGltfNodeInto(CurrentScene.Nodes, _cryData, omitSkins);

        return Root;
    }
}
