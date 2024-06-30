using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdUIntList : UsdAttribute
{
    public List<uint> Values { get; set; }

    public UsdUIntList(string name, List<uint> values) : base(name)
    {
        Values = values;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        sb.Append($"int[] {Name} = [");
        sb.AppendJoin(", ", Values.Select(v => $"{v}"));
        sb.Append(']');

        sb.CleanNumbers();

        return sb.ToString();
    }
}
