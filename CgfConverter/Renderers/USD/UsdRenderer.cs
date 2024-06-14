using System.IO;
using static CgfConverter.Utilities;
using static Extensions.FileHandlingExtensions;

namespace CgfConverter.Renderers.USD;
internal class UsdRenderer : IRenderer
{
    protected readonly ArgsHandler _args;
    protected readonly CryEngine _cryData;

    private readonly FileInfo usdOutputFile;

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

    }
}
