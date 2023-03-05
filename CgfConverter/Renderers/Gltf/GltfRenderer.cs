using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CgfConverter.Renderers.Gltf;

public class GltfRenderer : BaseRenderer
{
    private readonly bool _writeText, _writeBinary;

    public GltfRenderer(ArgsHandler argsHandler, CryEngine cryEngine, bool writeText, bool writeBinary)
        : base(argsHandler, cryEngine)
    {
        _writeText = writeText;
        _writeBinary = writeBinary;
    }

    public override void Render(string? outputDir = null, bool preservePath = true) =>
        new GltfRendererCommon(
                Args.PackFileSystem,
                Args.ExcludeNodeNames
                    .Select(x => new Regex(x, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    .ToList())
            .RenderSingleModel(CryData,
                _writeBinary ? new FileInfo(GetOutputFile("glb", outputDir, preservePath)) : null,
                _writeText ? new FileInfo(GetOutputFile("gltf", outputDir, preservePath)) : null,
                _writeText ? new FileInfo(GetOutputFile("bin", outputDir, preservePath)) : null);
}