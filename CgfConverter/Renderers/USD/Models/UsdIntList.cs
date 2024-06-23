using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdIntList : UsdAttribute
{
    public List<int> Values { get; set; }

    public UsdIntList(string name, List<int> values) : base(name)
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
