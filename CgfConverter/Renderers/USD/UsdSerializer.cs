using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace CgfConverter.Renderers.USD;

public class UsdSerializer
{
    DefaultObjectPoolProvider objectPoolProvider = new();
    public ObjectPool<StringBuilder> StringBuilderPool { get; }

    public UsdSerializer()
    {
        StringBuilderPool = objectPoolProvider.CreateStringBuilderPool();
    }

    public void Serialize(UsdDoc doc, TextWriter writer)
    {
        if (doc is null)
            return;

        var sb = StringBuilderPool.Get();
        var header = doc.Header.Serialize();
        sb.AppendLine(header);

        SerializeObject(doc.Prims[0], sb, 0);

        writer.Write(sb.ToString());
    }

    private static void SerializeObject(UsdPrim prim, StringBuilder sb, int indentLevel)
    {
        var type = prim.GetType();
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
                    var value = prop.GetValue(prim);
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

                //var nestedElementAttr = prop.GetCustomAttribute<UsdElementAttribute>();
                //if (nestedElementAttr != null)
                //{
                //    var nestedObj = prop.GetValue(prim);
                //    SerializeObject(nestedObj, sb, indentLevel + 1);
                //}
            }

            AppendIndent(sb, indentLevel);
            sb.AppendLine("}");
        }
    }

    //private static string SerializeVector3List(IList<Vector3> vectors)
    //{
    //    var sb = new StringBuilder();
    //    sb.Append('[');
    //    for (int i = 0; i < vectors.Count; i++)
    //    {
    //        var vector = vectors[i];
    //        sb.Append($"({vector.X}, {vector.Y}, {vector.Z})");
    //        if (i < vectors.Count - 1)
    //        {
    //            sb.Append(',');
    //        }
    //    }
    //    sb.Append(']');
    //    return sb.ToString();
    //}

    private static void AppendIndent(StringBuilder sb, int indentLevel)
    {
        sb.Append(new string(' ', indentLevel * 4));
    }
}
