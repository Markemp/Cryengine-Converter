using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdVector3d : UsdAttribute
{
    public Vector3 Value { get; set; }

    public UsdVector3d(string name, Vector3 value) : base(name)
    {
        Name = name;
        Value = value;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();

        sb.Append($"({Value.X:F7}, {Value.Y:F7}, {Value.Z:F7})");
        sb.CleanNumbers();

        return sb.ToString();
    }
}
