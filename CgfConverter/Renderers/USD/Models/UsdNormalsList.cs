﻿using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdNormalsList : UsdAttribute
{
    public List<Vector3> Values { get; set; } = [];

    public UsdNormalsList(string name, List<Vector3> vectors) : base(name)
    {
        Values = vectors;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        sb.Append($"normal3f[] {Name} = [");
        sb.AppendJoin(", ", Values.Select(v => $"({v.X:F7}, {v.Y:F7}, {v.Z:F7})"));
        sb.AppendLine("] (");
        sb.AppendIndent(indentLevel + 1);
        sb.AppendLine("interpolation = \"vertex\"");
        sb.AppendIndent(indentLevel);
        sb.Append(')');

        sb.CleanNumbers();

        return sb.ToString();
    }
}