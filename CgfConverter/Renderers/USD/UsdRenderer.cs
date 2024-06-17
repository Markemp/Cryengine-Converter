using CgfConverter.Renderers.USD.Models;
using System.IO;
using static CgfConverter.Utilities;

namespace CgfConverter.Renderers.USD;
public class UsdRenderer : IRenderer
{
    protected readonly ArgsHandler _args;
    protected readonly CryEngine _cryData;

    private readonly FileInfo usdOutputFile;
    private UsdDoc? usdDoc;

    public UsdRenderer(ArgsHandler argsHandler, CryEngine cryEngine)
    {
        _args = argsHandler;
        _cryData = cryEngine;
        usdOutputFile = _args.FormatOutputFileName(".usda", _cryData.InputFile);
    }

    public int Render()
    {
        GenerateUsdObject();

        Log(LogLevelEnum.Debug);
        Log(LogLevelEnum.Debug, "*** Starting Write USD ***");
        Log(LogLevelEnum.Debug);

        return 0;
    }

    public void GenerateUsdObject()
    {
        Log(LogLevelEnum.Debug, "Number of models: {0}", _cryData.Models.Count);
        for (int i = 0; i < _cryData.Models.Count; i++)
        {
            Log(LogLevelEnum.Debug, "\tNumber of nodes in model: {0}", _cryData.Models[i].NodeMap.Count);
        }

        // Create the usd doc
        usdDoc = new UsdDoc
        {
            Header = new UsdHeader()
        };

        // Create the root xform
        usdDoc.Prims.Add(new UsdXform("root"));
    }
}
