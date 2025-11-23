using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// Represents an array of 4x4 matrices for USD.
/// Used for bindTransforms and restTransforms in skeletons.
/// </summary>
public class UsdMatrix4dArray : UsdAttribute
{
    public List<Matrix4x4> Matrices { get; set; }

    public UsdMatrix4dArray(string name, List<Matrix4x4> matrices, bool isUniform = false)
        : base(name, isUniform)
    {
        Matrices = matrices;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        sb.Append($"matrix4d[] {Name} = [");

        for (int i = 0; i < Matrices.Count; i++)
        {
            var m = Matrices[i];

            // USD matrices are row-major format
            // Clean up very small values to avoid scientific notation clutter
            sb.Append($"( ({FormatMatrixValue(m.M11)}, {FormatMatrixValue(m.M12)}, {FormatMatrixValue(m.M13)}, {FormatMatrixValue(m.M14)}), ");
            sb.Append($"({FormatMatrixValue(m.M21)}, {FormatMatrixValue(m.M22)}, {FormatMatrixValue(m.M23)}, {FormatMatrixValue(m.M24)}), ");
            sb.Append($"({FormatMatrixValue(m.M31)}, {FormatMatrixValue(m.M32)}, {FormatMatrixValue(m.M33)}, {FormatMatrixValue(m.M34)}), ");
            sb.Append($"({FormatMatrixValue(m.M41)}, {FormatMatrixValue(m.M42)}, {FormatMatrixValue(m.M43)}, {FormatMatrixValue(m.M44)}) )");

            if (i < Matrices.Count - 1)
                sb.Append(", ");
        }

        sb.Append("]");

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
        // 0.###### means: integer part required, up to 6 optional decimal digits
        return value.ToString("0.######");
    }
}
