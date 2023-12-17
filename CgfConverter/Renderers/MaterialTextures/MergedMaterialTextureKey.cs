namespace CgfConverter.Renderers.MaterialTextures;

public sealed class MergedMaterialTextureKey : IMaterialTextureKey
{
    public MergedMaterialTextureKey(
        IMaterialTextureKey? r,
        IMaterialTextureKey? g,
        IMaterialTextureKey? b,
        IMaterialTextureKey? a)
    {
        RedSource = r;
        GreenSource = g;
        BlueSource = b;
        AlphaSource = a;
    }

    public IMaterialTextureKey? RedSource { get; }

    public IMaterialTextureKey? GreenSource { get; }

    public IMaterialTextureKey? BlueSource { get; }

    public IMaterialTextureKey? AlphaSource { get; }

    public bool Equals(IMaterialTextureKey? other) =>
        other is MergedMaterialTextureKey k
        && Equals(k.RedSource, RedSource)
        && Equals(k.GreenSource, GreenSource)
        && Equals(k.BlueSource, BlueSource)
        && Equals(k.AlphaSource, AlphaSource);
}
