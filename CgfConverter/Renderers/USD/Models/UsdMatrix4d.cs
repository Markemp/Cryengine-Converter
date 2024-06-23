using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdMatrix4d : UsdAttribute
{
    public Matrix4x4 Value { get; set; }

    public UsdMatrix4d(string name, Matrix4x4 value, bool isUniform = false)
        : base(name, isUniform)
    {
        Value = value;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        sb.Append($"matrix4d {Name} = (");
        sb.Append($" ({Value.M11:F7}, {Value.M12:F7}, {Value.M13:F7}, {Value.M14:F7}),");
        sb.Append($" ({Value.M21:F7}, {Value.M22:F7}, {Value.M23:F7}, {Value.M24:F7}),");
        sb.Append($" ({Value.M31:F7}, {Value.M32:F7}, {Value.M33:F7}, {Value.M34:F7}),");
        sb.Append($" ({Value.M41:F7}, {Value.M42:F7}, {Value.M43:F7}, {Value.M44:F7}) )");
        sb.CleanNumbers();

        return sb.ToString();
    }
}
