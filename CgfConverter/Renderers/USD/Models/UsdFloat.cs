using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdFloat : UsdAttribute
{
    public string? Value { get; set; }

    public UsdFloat(string name, string? value = null, bool isUniform = false) : base(name, isUniform)
    {
        Value = value;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        if (Value is null)
            sb.Append($"float {Name}");
        else
        {
            // Connection paths (starting with </) use angle brackets as-is
            // Float values are output directly
            if (Value.StartsWith("</") && Value.EndsWith(">"))
                sb.Append($"float {Name} = {Value}");
            else
                sb.Append($"float {Name} = {Value}");
        }

        return sb.ToString();
    }
}
