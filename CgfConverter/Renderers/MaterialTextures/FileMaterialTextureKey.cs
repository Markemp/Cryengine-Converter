namespace CgfConverter.Renderers.MaterialTextures;

public sealed class FileMaterialTextureKey : IMaterialTextureKey
{
    public FileMaterialTextureKey(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public bool Equals(IMaterialTextureKey? other) =>
        other is FileMaterialTextureKey k && k.Path == Path;
}