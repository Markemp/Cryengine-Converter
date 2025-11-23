using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// Represents a single 4x4 matrix for USD.
/// Used for geomBindTransform.
/// </summary>
public class UsdMatrix4d : UsdAttribute
{
    public Matrix4x4 Matrix { get; set; }

    public UsdMatrix4d(string name, Matrix4x4 matrix, bool isUniform = false)
        : base(name, isUniform)
    {
        Matrix = matrix;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        var m = Matrix;

        // USD matrices are row-major format
        sb.Append($"matrix4d {Name} = ( ");
        sb.Append($"({m.M11}, {m.M12}, {m.M13}, {m.M14}), ");
        sb.Append($"({m.M21}, {m.M22}, {m.M23}, {m.M24}), ");
        sb.Append($"({m.M31}, {m.M32}, {m.M33}, {m.M34}), ");
        sb.Append($"({m.M41}, {m.M42}, {m.M43}, {m.M44}) )");

        return sb.ToString();
    }
}
