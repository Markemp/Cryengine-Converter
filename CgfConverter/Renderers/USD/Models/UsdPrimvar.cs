using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdPrimvar<T>
{
    public string Name { get; set; }
    public List<T> Values { get; set; } = new List<T>();
    public string Interpolation { get; set; }

    public UsdPrimvar(string name, List<T> values, string interpolation = "vertex")
    {
        Name = name;
        Values = values;
        Interpolation = interpolation;
    }

    public string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);
        sb.AppendFormat("{0}[] primvars:{1} = [{2}] (",
            typeof(T).Name,
            Name,
            string.Join(", ", Values.Select(v => FormatValue(v))));
        sb.AppendFormat("interpolation = \"{0}\"", Interpolation);
        sb.Append(')');
        return sb.ToString();
    }

    private string FormatValue(T value)
    {
        if (value is Vector3 vector)
            return $"({vector.X:F7}, {vector.Y:F7}, {vector.Z:F7})";

        // Add more type handling if needed
        return value.ToString();
    }
}
