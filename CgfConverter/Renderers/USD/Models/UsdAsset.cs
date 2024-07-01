using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdAsset : UsdAttribute
{
    public string Value { get; set; }

    public UsdAsset(string name, string value, bool isUniform = false) : base(name, isUniform)
    {
        Value = value;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        sb.Append($"asset inputs:file = @{Value}@");

        return sb.ToString();
    }
}
