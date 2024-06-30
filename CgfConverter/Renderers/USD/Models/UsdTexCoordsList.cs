using CgfConverter.Models.Structs;
using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;
public class UsdTexCoordsList : UsdAttribute
{
    public List<UV> Values { get; set; }

    public UsdTexCoordsList(string name, List<UV> values, bool isUniform = false) : base(name, isUniform)
    {
        Values = values;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");
        sb.Append($"texCoord2f[] primvars:{Name} = [");
        sb.AppendJoin(", ", Values.Select(v => $"({v.U:F7}, {1.0 - v.V:F7})"));
        sb.AppendLine("] (");
        sb.AppendIndent(indentLevel + 1);
        sb.AppendLine("interpolation = \"vertex\"");
        sb.AppendIndent(indentLevel);
        sb.Append(')');
        sb.CleanNumbers();

        return sb.ToString();
    }
}
