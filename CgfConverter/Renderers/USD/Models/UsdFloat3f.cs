using CgfConverter.Renderers.USD.Attributes;

namespace CgfConverter.Renderers.USD.Models;

public sealed record UsdFloat3f
{
    public UsdFloat3f(string name, bool isUniform = false)
    {
    }

    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }

    public override string ToString() => $"({X:F7}, {Y:F7}, {Z:F7})";
}
