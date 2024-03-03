namespace CgfConverter.Renderers.MaterialTextures;

public sealed class InvertedMaterialTextureKey : IMaterialTextureKey
{
    public InvertedMaterialTextureKey(IMaterialTextureKey parent)
    {
        Parent = parent;
    }

    public IMaterialTextureKey Parent { get; }

    public bool Equals(IMaterialTextureKey? other) =>
        other is InvertedMaterialTextureKey k
        && k.Parent.Equals(Parent);
}
