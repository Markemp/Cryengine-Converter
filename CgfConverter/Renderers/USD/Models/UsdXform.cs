using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("Xform")]
public class UsdXform : UsdPrim
{
    public UsdXform(string name, bool isUniform = false)
        : base(name)
    {
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();

        //sb.AppendIndent(indentLevel)
        //    .AppendLine($"def Xform \"{Name}\"")
        //    .AppendIndent(indentLevel)
        //    .AppendLine("{")
        //        .Append(SerializeAttributes(indentLevel + 1))
        //        .Append(SerializeChildren(indentLevel + 1))
        //    .AppendIndent(indentLevel)
        //    .AppendLine("}");

        return sb.ToString();
    }
}
