using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public abstract class UsdPrim
{
    public string Name { get; set; }

    public Dictionary<string, object>? Properties { get; set; }
    public List<UsdAttribute> Attributes { get; set; } = [];
    public List<UsdPrim> Children { get; set; } = [];

    protected UsdPrim(string name)
    {
        Name = name;
    }

    public abstract string Serialize(int indentLevel);

    protected string SerializeAttributes(int indentLevel)
    {
        var sb = new StringBuilder();
        foreach (var attribute in Attributes)
        {
            sb.AppendIndent(indentLevel);
            sb.AppendLine(attribute.Serialize(indentLevel));
        }
        return sb.ToString();
    }

    protected string SerializeProperties()
    {
        if (Properties.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("(");
        foreach (var kvp in Properties)
        {
            sb.AppendFormat(" {0} = {1}", kvp.Key, SerializePropertyValue(kvp.Value));
        }
        sb.Append(" )");
        return sb.ToString();
    }

    private string SerializePropertyValue(object value)
    {
        if (value is string)
        {
            return $"\"{value}\"";
        }
        else if (value is IEnumerable<string> list)
        {
            return $"[{string.Join(", ", list.Select(v => $"\"{v}\""))}]";
        }
        else
        {
            return value.ToString();
        }
    }

    protected string SerializeChildren(int indentLevel)
    {
        var sb = new StringBuilder();
        if (Children is not null && Children.Count > 0)
        {
            foreach (var child in Children)
            {
                sb.Append(child.Serialize(indentLevel + 1));
            }
        }
        
        return sb.ToString();
    }

    protected void AppendIndent(StringBuilder sb, int indentLevel)
    {
        sb.Append(new string(' ', indentLevel * 4));
    }
}
