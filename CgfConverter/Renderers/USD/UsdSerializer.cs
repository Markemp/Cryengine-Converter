using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
using Extensions;
using Microsoft.Extensions.ObjectPool;
using System.IO;
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

    private void SerializeObject(UsdPrim prim, StringBuilder sb, int indentLevel)
    {
        var type = prim.GetType();
        var elementAttr = type.GetCustomAttribute<UsdElementAttribute>();

        if (elementAttr is not null)
         {
            sb.AppendIndent(indentLevel);
            if (prim.Properties is null)
            {
                sb.AppendLine($"def {elementAttr.ElementName} \"{prim.Name}\"");
                sb.AppendIndent(indentLevel);
                sb.AppendLine("{");
            }
            else
            {
                sb.AppendLine($"def {elementAttr.ElementName} \"{prim.Name}\" (");
                foreach (var property in prim.Properties)
                {
                    foreach (var key in property.Properties.Keys)
                    {
                        sb.AppendIndent(indentLevel + 1);
                        if (property.IsPrepend)
                            sb.AppendFormat("prepend {0} = {1}", key, property.Properties[key]);
                        else
                            sb.AppendFormat("{0} = {1}", key, property.Properties[key]);
                    }
                }
                sb.AppendLine();
                sb.AppendIndent(indentLevel);
                sb.AppendLine(")");
                sb.AppendIndent(indentLevel);
                sb.AppendLine("{");
            }

            foreach (var attribute in prim.Attributes)
            {
                sb.AppendLine(attribute.Serialize(indentLevel + 1));
            }

            if (prim.Children is not null)
                foreach (var child in prim.Children)
                {
                    SerializeObject(child, sb, indentLevel + 1);
                }
            sb.AppendIndent(indentLevel);
            sb.AppendLine("}");
        }
    }

    private static void AppendIndent(StringBuilder sb, int indentLevel)
    {
        sb.Append(new string(' ', indentLevel * 4));
    }
}
