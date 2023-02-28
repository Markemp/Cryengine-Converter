using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CgfConverter.Renderers.Gltf.Models;

namespace CgfConverter.Renderers.Gltf;

public partial class GltfRenderer : BaseRenderer
{
    private readonly GltfWriter _gltf;
    private readonly Dictionary<uint, int> _controllerIdToNodeIndex;
    private readonly bool _writeText, _writeBinary;

    public GltfRenderer(ArgsHandler argsHandler, CryEngine cryEngine, bool writeText, bool writeBinary) : base(
        argsHandler,
        cryEngine)
    {
        _gltf = new GltfWriter();
        _controllerIdToNodeIndex = new Dictionary<uint, int>();
        _writeText = writeText;
        _writeBinary = writeBinary;
    }

    public override void Render(string? outputDir = null, bool preservePath = true)
    {
        preservePath = false;

        var glbOutputFile = new FileInfo(GetOutputFile("glb", outputDir, preservePath));
        var gltfOutputFile = new FileInfo(GetOutputFile("gltf", outputDir, preservePath));
        var gltfBinOutputFile = new FileInfo(GetOutputFile("bin", outputDir, preservePath));

        _gltf.Add(new GltfScene
        {
            Name = "Scene",
        });

        if (CryData.Models.Count == 1) // Single file model
            WriteGeometries(CryData.Models[0]);
        else
            WriteGeometries(CryData.Models[1]);

        // This MUST be called after WriteGeometries, where _controllerIdToNodeIndex gets populated.
        WriteAnimations();

        if (_writeBinary)
        {
            using var glb = glbOutputFile.Open(FileMode.Create, FileAccess.Write);
            _gltf.CompileToBinary(glb);
        }

        if (_writeText)
        {
            using var gltf = gltfOutputFile.Open(FileMode.Create, FileAccess.Write);
            using var bin = gltfBinOutputFile.Open(FileMode.Create, FileAccess.Write);
            _gltf.CompileToPair(gltfBinOutputFile.Name, gltf, bin);
        }
    }

    private static Vector3 SwapAxes(Vector3 val) => new(val.X, val.Z, val.Y);

    private static Quaternion SwapAxes(Quaternion val) => new(val.X, val.Z, val.Y, -val.W);

    private static Matrix4x4 SwapAxes(Matrix4x4 val) => new(
        val.M11, val.M13, val.M12, val.M14,
        val.M31, val.M33, val.M32, val.M34,
        val.M21, val.M23, val.M22, val.M24,
        val.M41, val.M43, val.M42, val.M44);
}