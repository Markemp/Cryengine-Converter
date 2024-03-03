namespace CgfConverter.Renderers.MaterialTextures;

public sealed class ChannelMaterialTextureKey : IMaterialTextureKey
{
    public ChannelMaterialTextureKey(IMaterialTextureKey parent, MaterialTextureChannel channel)
    {
        Parent = parent;
        Channel = channel;
    }

    public IMaterialTextureKey Parent { get; }

    public MaterialTextureChannel Channel { get; }

    public bool Equals(IMaterialTextureKey? other) =>
        other is ChannelMaterialTextureKey k
        && k.Parent.Equals(Parent)
        && k.Channel == Channel;
}
