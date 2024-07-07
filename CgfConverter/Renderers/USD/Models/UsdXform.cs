using CgfConverter.Renderers.USD.Attributes;
using System.Collections.Generic;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("Xform")]
public class UsdXform : UsdPrim
{
    /// <summary>Full path to the prim.  /root/_materials/primName</summary>
    public string Path { get; set; }

    public UsdXform(string name, string parentPath, List<UsdProperty>? properties = null, bool isUniform = false)
        : base(name, properties)
    {
        Path = $"{parentPath}/{name}";
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
