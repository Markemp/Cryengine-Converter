using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdPointsList : UsdAttribute
{
    public List<Vector3> Values { get; set; }

    public UsdPointsList(string name, List<Vector3> value) : base(name)
    {
        Values = value;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        sb.Append($"point3f[] {Name} = [");
        sb.AppendJoin(", ", Values.Select(v => $"({v.X:F7}, {v.Y:F7}, {v.Z:F7})"));
        sb.Append(']');
        sb.CleanNumbers();

        return sb.ToString();
    }
}
