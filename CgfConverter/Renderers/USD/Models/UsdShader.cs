using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("Shader")]
public class UsdShader : UsdPrim
{
    public string InfoId { get; set; }
    public Dictionary<string, object> Inputs { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> Outputs { get; set; } = new Dictionary<string, object>();

    protected UsdShader(string name) : base(name)
    {
    }

    public override string Serialize(int indentLevel)
    {
        throw new System.NotImplementedException();
    }

    protected string SerializeInputs(int indentLevel)
    {
        var sb = new StringBuilder();
        foreach (var input in Inputs)
        {
            sb.AppendIndent(indentLevel);
            if (input.Value is string connect)
            {
                sb.AppendLine($"{input.Key}.connect = {connect}");
            }
            else
            {
                sb.AppendLine($"{input.Key} = {input.Value}");
            }
        }
        return sb.ToString();
    }

    protected string SerializeOutputs(int indentLevel)
    {
        var sb = new StringBuilder();
        foreach (var output in Outputs)
        {
            sb.AppendIndent(indentLevel);
            sb.AppendLine($"{output.Key}");
        }
        return sb.ToString();
    }
}
