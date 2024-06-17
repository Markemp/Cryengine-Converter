using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace CgfConverter.Renderers.USD;

public class UsdSerializer
{
    public static string Serialize(object obj)
    {
        var sb = new StringBuilder();
        SerializeObject(obj, sb, 0);
        return sb.ToString();
    }

    private static void SerializeObject(object obj, StringBuilder sb, int indentLevel)
    {
        if (obj is null) return;

        var type = obj.GetType();
        var elementAttr = type.GetCustomAttribute<UsdElementAttribute>();
        if (elementAttr is not null)
        {
            AppendIndent(sb, indentLevel);
            sb.AppendLine($"def {elementAttr.ElementName} \"{type.Name}\"");
            AppendIndent(sb, indentLevel);
            sb.AppendLine("{");

            foreach (var prop in type.GetProperties())
            {
                var propAttr = prop.GetCustomAttribute<UsdPropertyAttribute>();
                if (propAttr != null)
                {
                    var value = prop.GetValue(obj);
                    if (value is IList<Vector3> vectorList)
                    {
                        AppendIndent(sb, indentLevel + 1);
                        //sb.AppendLine($"{propAttr.PropertyName} = {SerializeVector3List(vectorList)}");
                    }
                    else
                    {
                        AppendIndent(sb, indentLevel + 1);
                        sb.AppendLine($"{propAttr.PropertyName} = {value}");
                    }
                }

                var nestedElementAttr = prop.GetCustomAttribute<UsdElementAttribute>();
                if (nestedElementAttr != null)
                {
                    var nestedObj = prop.GetValue(obj);
                    SerializeObject(nestedObj, sb, indentLevel + 1);
                }
            }

            AppendIndent(sb, indentLevel);
            sb.AppendLine("}");
        }
    }

    //private static string SerializeVector3List(IList<Vector3> vectors)
    //{
    //    var sb = new StringBuilder();
    //    sb.Append("[");
    //    for (int i = 0; i < vectors.Count; i++)
    //    {
    //        var vector = vectors[i];
    //        sb.Append($"({vector.X}, {vector.Y}, {vector.Z})");
    //        if (i < vectors.Count - 1)
    //        {
    //            sb.Append(", ");
    //        }
    //    }
    //    sb.Append("]");
    //    return sb.ToString();
    //}

    private static void AppendIndent(StringBuilder sb, int indentLevel)
    {
        sb.Append(new string(' ', indentLevel * 4));
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class UsdElementAttribute : Attribute
{
    public string ElementName { get; }
    public UsdElementAttribute(string elementName)
    {
        ElementName = elementName;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class UsdPropertyAttribute : Attribute
{
    public string PropertyName { get; }
    public UsdPropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}
