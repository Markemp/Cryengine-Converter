using CgfConverter.Renderers.USD.Attributes;
using System.Collections.Generic;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("Mesh")]
public class UsdMesh : UsdPrim
{
    public UsdMesh(string name, List<UsdProperty>? properties = null) : base(name, properties)
    {
        Name = name;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();

        //sb.AppendIndent(indentLevel)
        //    .AppendLine($"def Mesh \"{Name}\"")
        //    .AppendIndent(indentLevel)
        //    .AppendLine("{")
        //        .Append(SerializeAttributes(indentLevel + 1))
        //        .Append(SerializeChildren(indentLevel + 1))
        //    .AppendIndent(indentLevel)
        //    .AppendLine("}");

        return sb.ToString();
    }
}
