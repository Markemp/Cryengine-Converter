using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdFloat2 : UsdAttribute
{
    public string Value { get; set; }
    public string SourceShaderName { get; set; }

    public UsdFloat2(string name, string matPath, string sourceShaderName) : base(name)
    {
        Value = matPath;
        SourceShaderName = sourceShaderName;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        sb.Append($"float2 {Name} = {Value}/{SourceShaderName}.outputs:result");

        return sb.ToString();
    }
}
