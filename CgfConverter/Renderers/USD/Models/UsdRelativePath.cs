using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdRelativePath : UsdAttribute
{
    public string Value { get; set; }

    public UsdRelativePath(string name, string value, bool isUniform = false) : base(name, isUniform)
    {
        Value = value;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        sb.Append($"rel material:binding = <{Value}>");

        return sb.ToString();
    }
}
