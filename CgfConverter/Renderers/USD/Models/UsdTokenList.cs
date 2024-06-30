using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdTokenList : UsdAttribute
{
    public List<string> Values { get; set; }

    public UsdTokenList(string name, List<string> values) : base(name)
    {
        Values = values;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        sb.Append($"token[] {Name} = [");
        sb.AppendJoin(", ", Values.Select(v => $"\"{v}\""));
        sb.Append(']');

        return sb.ToString();
    }
}
