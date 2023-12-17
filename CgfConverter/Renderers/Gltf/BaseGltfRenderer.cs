using System.Collections.Generic;
using System.IO;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Renderers.MaterialTextures;
using CgfConverter.Utils;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    protected readonly TaggedLogger Log;
    protected readonly ArgsHandler Args;
    private readonly bool _writeText;
    private readonly bool _writeBinary;
    private readonly Dictionary<(string MaterialFile, string SubMaterialName), WrittenMaterial> _materialMap = new();

    private readonly List<byte[]> _bytesList = new();
    private readonly Dictionary<string, byte[]> _filesList = new();
    protected GltfRoot Root = new();

    private MaterialTextureManager _materialTextureManager;

    private int _currentOffset;

    public BaseGltfRenderer(ArgsHandler argsHandler, string logTag, bool writeText, bool writeBinary)
    {
        Log = new TaggedLogger(logTag);
        Args = argsHandler;
        _materialTextureManager = new MaterialTextureManager(argsHandler);
        _writeText = writeText;
        _writeBinary = writeBinary;
    }

    protected void Reset(string sceneName)
    {
        _materialMap.Clear();

        _bytesList.Clear();
        _materialTextureManager.Clear();
        _currentOffset = 0;
        Root = new GltfRoot();
        Root.Scenes.Add(new GltfScene
        {
            Name = sceneName,
        });
        Root.ExtensionsUsed.Add("KHR_materials_specular");
        Root.ExtensionsUsed.Add("KHR_materials_pbrSpecularGlossiness");
        Root.ExtensionsUsed.Add("KHR_materials_emissive_strength");
    }

    protected void Save(string referenceName, string? layerName = null)
    {
        string baseDir = Args.FormatOutputFileName(".glb", referenceName, layerName).DirectoryName!;
        foreach ((string k, byte[] v) in _filesList)
            File.WriteAllBytes(Path.Join(baseDir, k), v);

        if (_writeBinary)
        {
            var glbf = Args.FormatOutputFileName(".glb", referenceName, layerName);

            using var glb = glbf.Open(FileMode.Create, FileAccess.Write);
            CompileToBinary(glb);

            Log.I($"Saved: {glbf.Name} in {glbf.DirectoryName}");
        }

        if (_writeText)
        {
            var gltfFile = Args.FormatOutputFileName(".gltf", referenceName, layerName);
            var glbFile = Args.FormatOutputFileName(".bin", referenceName, layerName);

            using var gltf = gltfFile.Open(FileMode.Create, FileAccess.Write);
            using var bin = glbFile.Open(FileMode.Create, FileAccess.Write);
            CompileToPair(Path.GetFileName(bin.Name), gltf, bin);

            Log.I($"Saved: {gltfFile.Name} and {glbFile.Name} in {gltfFile.DirectoryName}");
        }
    }
}
