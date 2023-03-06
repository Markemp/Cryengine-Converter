using System.Collections.Generic;
using System.IO;
using CgfConverter.Materials;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Utils;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    protected readonly TaggedLogger Log;
    protected readonly ArgsHandler Args;
    private readonly bool _writeText;
    private readonly bool _writeBinary;
    private readonly Dictionary<Material, int> _materialMap = new();
    
    protected GltfRoot _root = new();
    private readonly List<byte[]> _bytesList = new();

    private int _currentOffset;

    public BaseGltfRenderer(ArgsHandler argsHandler, string logTag, bool writeText, bool writeBinary)
    {
        Log = new TaggedLogger(logTag);
        Args = argsHandler;
        _writeText = writeText;
        _writeBinary = writeBinary;
    }

    protected void Reset(string sceneName)
    {
        _materialMap.Clear();
        
        _bytesList.Clear();
        _currentOffset = 0;
        _root = new GltfRoot();
        _root.Scenes.Add(new GltfScene
        {
            Name = sceneName,
        });
    }

    protected void Save(string referenceName, string? layerName = null)
    {
        if (_writeBinary)
        {
            var glbf = Args.FormatOutputFileName(".glb", referenceName, layerName);
            using (var glb = glbf.OpenWrite())
                CompileToBinary(glb);
            Log.I($"Saved: {glbf.Name} in {glbf.DirectoryName}");
        }

        if (_writeText)
        {
            var gltfFile = Args.FormatOutputFileName(".gltf", referenceName, layerName);
            var glbFile = Args.FormatOutputFileName(".bin", referenceName, layerName);
            using (var gltf = gltfFile.OpenWrite())
            using (var bin = glbFile.OpenWrite())
                CompileToPair(Path.GetFileName(bin.Name), gltf, bin);
            Log.I($"Saved: {gltfFile.Name} and {glbFile.Name} in {gltfFile.DirectoryName}");
        }
    }
}