using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdVector3dList : UsdAttribute
{
    public List<Vector3> Values { get; set; } = [];

    public UsdVector3dList(string name, List<Vector3> vectors) : base(name)
    {
        Values = vectors;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        sb.Append($"float3[] {Name} = [");
        sb.AppendJoin(", ", Values.Select(v => $"({v.X:F7}, {v.Y:F7}, {v.Z:F7})"));
        sb.Append(']');

        sb.CleanNumbers();

        return sb.ToString();
    }
}
