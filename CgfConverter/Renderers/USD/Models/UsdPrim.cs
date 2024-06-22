using System.Collections.Generic;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public abstract class UsdPrim
{
    protected UsdPrim(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public List<UsdPrim>? Children { get; set; }

    public abstract string Serialize();

    protected string SerializeParameters()
    {
        var sb = new StringBuilder();

        if (Parameters is null)
            return sb.ToString();

        foreach (var param in Parameters)
            sb.AppendLine($"    {param.Key} = {SerializeParameter(param.Value)}");

        return sb.ToString();
    }

    protected static string? SerializeParameter(object param)
    {
        if (param is null)
            return null;

        if (param is Dictionary<string, object> dict)
        {
            var entries = new List<string>();
            foreach (var kv in dict)
                entries.Add($"{kv.Key} = {SerializeParameter(kv.Value)}");
            return $"{{ {string.Join(", ", entries)} }}";
        }
        else if (param is List<string> list)
            return $"[{string.Join(", ", list)}]";
        else if (param is bool b)
            return b ? "1" : "0";
        else
            return param.ToString();
    }

    protected string? SerializeChildren()
    {
        if (Children is null)
            return null;

        var sb = new StringBuilder();

        foreach (var child in Children)
            if (child is UsdPrim prim)
                sb.Append(prim.Serialize());

        return sb.ToString();
    }
}
