using CgfConverter.Renderers.Usd.UsdCore;
using System.IO;
using System.Text;

namespace CgfConverter.Renderers.Usd;

public class UsdRenderer : BaseRenderer
{
    public UsdRenderer(ArgsHandler argsHandler, CryEngine cryEngine) : base(argsHandler, cryEngine) { }

    public override void Render(string outputDir = null, bool preservePath = true)
    {
        var sb = new StringBuilder();

        UsdMetadata metadata= new UsdMetadata();
        sb =  metadata.Generate(sb);

        using StreamWriter file = new (@"hereIam.usda");
        file.Write(sb);
    }
}
