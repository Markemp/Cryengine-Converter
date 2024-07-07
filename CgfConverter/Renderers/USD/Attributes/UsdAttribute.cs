using System;
using System.Collections;
using System.Linq;

namespace CgfConverter.Renderers.USD.Attributes;

public abstract class UsdAttribute
{
    public string Name { get; set; }
    public bool IsUniform { get; set; }

    protected UsdAttribute(string name, bool isUniform = false)
    {
        Name = name;
        IsUniform = isUniform;
    }

    public abstract string Serialize(int indentLevel);

    protected string FormatStringValue(string value)
    {
        if (value.StartsWith("<") && value.EndsWith(">"))
            return value;  // Return as is for paths with < >

        return $"\"{value}\"";  // Use quotes for regular strings
    }
}

public class UsdAttribute<T> : UsdAttribute
{
    public T Value { get; set; }

    public UsdAttribute(string name, T value, bool isUniform = false)
        : base(name, isUniform)
    {
        Value = value;
    }

    public override string Serialize(int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        var typeName = Value switch
        {
            bool => "bool",
            int => "int",
            float => "float",
            double => "double",
            string => "string",
            IEnumerable => "string[]",
            _ => throw new NotSupportedException($"Unsupported type {Value.GetType()}")
        };


        if (Value is IEnumerable enumerable && !(Value is string))
        {
            var items = string.Join(", ", enumerable.Cast<object>().Select(item => item.ToString()));
            return $"{indent}{typeName}{Name} = [{items}]";
        }
        else
        {
            return $"{indent}{typeName} {Name} = {Value}";
        }
    }
}
