using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace CgfConverter.Renderers.MaterialTextures;

public sealed class SolidChannelMaterialTextureKey : IMaterialTextureKey
{
    public SolidChannelMaterialTextureKey(float color, int width, int height)
    {
        Color = color;
        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public float Color { get; }

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public bool Equals(IMaterialTextureKey? other) =>
        other is SolidChannelMaterialTextureKey k && k.Color == Color;
}
