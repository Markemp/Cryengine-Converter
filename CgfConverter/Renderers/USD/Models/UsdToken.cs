using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdToken : UsdAttribute
{
    public string Value { get; set; }

    public UsdToken(string name, string value, bool isUniform = false) : base(name, isUniform)
    {
        Value = value;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        sb.Append($"token {Name} = \"{Value}\"");

        return sb.ToString();
    }
}
