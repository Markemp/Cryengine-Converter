using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System;
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
        sb.Append($"({FormatMatrixValue(m.M11)}, {FormatMatrixValue(m.M12)}, {FormatMatrixValue(m.M13)}, {FormatMatrixValue(m.M14)}), ");
        sb.Append($"({FormatMatrixValue(m.M21)}, {FormatMatrixValue(m.M22)}, {FormatMatrixValue(m.M23)}, {FormatMatrixValue(m.M24)}), ");
        sb.Append($"({FormatMatrixValue(m.M31)}, {FormatMatrixValue(m.M32)}, {FormatMatrixValue(m.M33)}, {FormatMatrixValue(m.M34)}), ");
        sb.Append($"({FormatMatrixValue(m.M41)}, {FormatMatrixValue(m.M42)}, {FormatMatrixValue(m.M43)}, {FormatMatrixValue(m.M44)}) )");

        return sb.ToString();
    }

    /// <summary>
    /// Formats a matrix value for USD output.
    /// Values less than 1e-8 are rounded to zero.
    /// Other values are formatted with max 6 decimal places.
    /// </summary>
    private static string FormatMatrixValue(float value)
    {
        // Round very small values to zero
        if (Math.Abs(value) < 1e-8f)
            return "0";

        // Format with max 6 decimal places, stripping trailing zeros
        return value.ToString("0.######");
    }
}
