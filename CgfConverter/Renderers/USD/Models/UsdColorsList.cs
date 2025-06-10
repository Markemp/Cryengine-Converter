using CgfConverter.Models.Structs;
using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdColorsList : UsdAttribute
{
    public List<IRGBA> Values { get; set; } = new();

    public UsdColorsList(string name, List<IRGBA> vectors) : base(name)
    {
        Values = vectors;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        sb.Append($"color3f[] primvars:{Name} = [");
        sb.AppendJoin(", ", Values.Select(v => $"({v.R / 255:F7}, {v.G / 255:F7}, {v.B / 255:F7})"));
        sb.AppendLine("] (");
        sb.AppendIndent(indentLevel + 1);
        sb.AppendLine("interpolation = \"vertex\"");
        sb.AppendIndent(indentLevel);
        sb.Append(')');
        sb.CleanNumbers();

        return sb.ToString();
    }
}
