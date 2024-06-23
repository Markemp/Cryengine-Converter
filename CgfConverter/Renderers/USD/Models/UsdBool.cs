using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdBool : UsdAttribute
{
    public bool Value { get; set; }

    public UsdBool(string name, bool value, bool isUniform = false) : base(name, isUniform)
    {
        Value = value;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        sb.AppendLine($"bool {Name} = {(Value == true ? 1 : 0)}");

        return sb.ToString();
    }
}
