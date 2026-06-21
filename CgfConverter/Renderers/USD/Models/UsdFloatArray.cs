using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// Represents an array of floats for USD.
/// </summary>
public class UsdFloatArray : UsdAttribute
{
    public List<float> Values { get; set; }
    public int? ElementSize { get; set; }
    public string? Interpolation { get; set; }

    public UsdFloatArray(string name, List<float> values, int? elementSize = null, string? interpolation = null, bool isUniform = false)
        : base(name, isUniform)
    {
        Values = values;
        ElementSize = elementSize;
        Interpolation = interpolation;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        sb.Append($"float[] {Name} = [");
        sb.Append(string.Join(", ", Values));
        sb.Append("]");

        if (ElementSize.HasValue || Interpolation != null)
        {
            sb.AppendLine(" (");
            if (ElementSize.HasValue)
            {
                sb.AppendIndent(indentLevel + 1);
                sb.AppendLine($"elementSize = {ElementSize.Value}");
            }
            if (Interpolation != null)
            {
                sb.AppendIndent(indentLevel + 1);
                sb.AppendLine($"interpolation = \"{Interpolation}\"");
            }
            sb.AppendIndent(indentLevel);
            sb.Append(")");
        }

        return sb.ToString();
    }
}
