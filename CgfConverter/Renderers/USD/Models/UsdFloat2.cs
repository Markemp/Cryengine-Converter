using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdFloat2 : UsdAttribute
{
    public string? Value { get; set; }

    public UsdFloat2(string name, string? value) : base(name)
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
            sb.Append($"float2 {Name}");
        else
        {
            var stringValue = FormatStringValue($"<{Value}>");
            sb.Append($"float2 {Name} = {stringValue}");
        }
            

        return sb.ToString();
    }
}
