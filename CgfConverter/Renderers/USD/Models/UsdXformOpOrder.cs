﻿using CgfConverter.Renderers.USD.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdXformOpOrder : UsdAttribute
{
    public List<string> Values { get; set; } = new List<string>();

    public UsdXformOpOrder(string name, List<string> values, bool isUniform = false)
        : base(name, isUniform)
    {
        Values = values;
    }

    public override string Serialize()
    {
        var sb = new StringBuilder();

        if (IsUniform)
            sb.Append("uniform ");

        sb.Append($"token[] {Name} = [{string.Join(", ", Values.Select(v => $"\"{v}\""))}]");

        return sb.ToString();
    }
}
