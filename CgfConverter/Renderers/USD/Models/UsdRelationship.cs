using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// Represents a USD relationship (rel).
/// Used for skel:skeleton relationships between meshes and skeletons.
/// </summary>
public class UsdRelationship : UsdAttribute
{
    public string Target { get; set; }

    public UsdRelationship(string name, string target)
        : base(name, false)
    {
        Target = target;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);

        sb.Append($"rel {Name} = {Target}");

        return sb.ToString();
    }
}
