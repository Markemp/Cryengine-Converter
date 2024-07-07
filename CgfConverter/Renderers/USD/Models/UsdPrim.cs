using CgfConverter.Renderers.USD.Attributes;
using System.Collections.Generic;

namespace CgfConverter.Renderers.USD.Models;

public abstract class UsdPrim
{
    public string Name { get; set; }

    public List<UsdProperty>? Properties { get; set; }

    public List<UsdAttribute> Attributes { get; set; } = [];

    public List<UsdPrim> Children { get; set; } = [];

    protected UsdPrim(string name, List<UsdProperty>? properties = null)
    {
        Name = name;
        Properties = properties;
    }

    public abstract string Serialize(int indentLevel);

    //protected string SerializeAttributes(int indentLevel)
    //{
    //    var sb = new StringBuilder();
    //    foreach (var attribute in Attributes)
    //    {
    //        sb.AppendIndent(indentLevel);
    //        sb.AppendLine(attribute.Serialize(indentLevel));
    //    }
    //    return sb.ToString();
    //}

    //protected string SerializeProperties(int indentLevel)
    //{
    //    if (Properties is null || Properties.Count == 0)
    //        return string.Empty;

    //    var sb = new StringBuilder();
        
    //    sb.Append('(');
    //    foreach (var property in Properties)
    //    {
    //        sb.AppendFormat(" {0} = {1}", kvp, SerializePropertyValue(kvp.Value));
    //    }
    //    sb.AppendIndent(indentLevel);
    //    sb.AppendLine(" )");
    //    return sb.ToString();
    //}

    //private string SerializePropertyValue(object value)
    //{
    //    if (value is string)
    //        return $"\"{value}\"";
    //    else if (value is IEnumerable<string> list)
    //        return $"[{string.Join(", ", list.Select(v => $"\"{v}\""))}]";
    //    else
    //        return value.ToString();
    //}
}
