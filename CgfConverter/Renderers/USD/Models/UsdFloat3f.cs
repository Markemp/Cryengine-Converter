using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdFloat3f : UsdAttribute
{
    public string? Value { get; set; }

    public UsdFloat3f(string name, string? value = null) : base(name)
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
            sb.Append($"float3 {Name}");
        else
        {
            // Connection paths (starting with </) use angle brackets as-is
            // Other values need to be wrapped in angle brackets
            if (Value.StartsWith("</") && Value.EndsWith(">"))
                sb.Append($"float3 {Name} = {Value}");
            else
            {
                var stringValue = FormatStringValue($"<{Value}>");
                sb.Append($"float3 {Name} = {stringValue}");
            }
        }


        return sb.ToString();
    }
}
