using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdColor3f : UsdAttribute
{
    public string? Value { get; set; }

    public UsdColor3f(string name, string? value, bool isUniform = false) : base(name, isUniform)
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
            sb.Append($"color3f {Name}");
        else
        {
            var stringValue = FormatStringValue($"<{Value}>");
            sb.Append($"color3f {Name} = {stringValue}");
        }

        return sb.ToString();
    }
}
