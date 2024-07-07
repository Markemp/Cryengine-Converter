using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdToken<T> : UsdAttribute
{
    public T Value { get; set; }

    public UsdToken(string name, T value, bool isUniform = false) : base(name, isUniform)
    {
        Value = value;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        string typeName = Value is IEnumerable && !(Value is string) ? "token[]" : "token";
        if (Value is null)
            sb.Append($"{typeName} {Name}");
        else
            sb.Append($"{typeName} {Name} = {FormatValue(Value)}");

        return sb.ToString();
    }

    private string FormatValue(T value)
    {
        switch (value)
        {
            case string stringValue:
                return FormatStringValue(stringValue);
            case IEnumerable<string> stringList:
                return $"[{string.Join(", ", stringList.Select(FormatStringValue))}]";
            case IEnumerable<Vector3> vector3List:
                return $"[{string.Join(", ", vector3List.Select(v => $"({v.X}, {v.Y}, {v.Z})"))}]";
            case IEnumerable<Matrix4x4> matrixList:
                return $"[{string.Join(", ", matrixList.Select(x => new UsdMatrix4d("", x)))}]";
            case IEnumerable<T> genericList:
                return $"[{string.Join(", ", genericList.Select(v => v.ToString()))}]";
            default:
                return value.ToString();
        }
    }
}
