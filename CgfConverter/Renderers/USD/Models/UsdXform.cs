using CgfConverter.Renderers.USD.Attributes;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("Xform")]
public class UsdXform : UsdPrim
{
    public UsdXform(string name, bool isUniform = false)
        : base(name)
    {
        //Attributes.Add(new UsdMatrix4d("xformOp:transform", transform, isUniform));
        //Attributes.Add(new UsdXformOpOrder("xformOpOrder", xformOpOrder, isUniform));
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();

        AppendIndent(sb, indentLevel);
        sb.AppendLine($"def Xform \"{Name}\"");
        AppendIndent(sb, indentLevel);
        sb.AppendLine("{");
        sb.Append(SerializeAttributes(indentLevel + 1));
        sb.Append(SerializeChildren(indentLevel + 1));
        AppendIndent(sb, indentLevel);
        sb.AppendLine("}");

        return sb.ToString();
    }
}
