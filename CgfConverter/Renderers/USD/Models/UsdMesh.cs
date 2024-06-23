using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;
public class UsdMesh : UsdPrim
{
    public UsdMesh(string name) : base(name)
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
