using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// Represents an array of tokens for USD.
/// Used for joint names in skeletons.
/// </summary>
public class UsdTokenArray : UsdAttribute
{
    public List<string> Tokens { get; set; }

    public UsdTokenArray(string name, List<string> tokens, bool isUniform = false)
        : base(name, isUniform)
    {
        Tokens = tokens;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        if (IsUniform)
            sb.Append("uniform ");

        sb.Append($"token[] {Name} = [");

        // Each token is quoted
        var quotedTokens = Tokens.Select(t => $"\"{t}\"");
        sb.Append(string.Join(", ", quotedTokens));

        sb.Append("]");

        return sb.ToString();
    }
}
