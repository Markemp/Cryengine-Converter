using Extensions;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdVector3d
{
    public Vector3 Value { get; set; }

    public UsdVector3d(Vector3 value)
    {
        Value = value;
    }

    public string Serialize()
    {
        var sb = new StringBuilder();

        sb.Append($"({Value.X:F7}, {Value.Y:F7}, {Value.Z:F7})");
        sb.CleanNumbers();

        return sb.ToString();
    }
}
