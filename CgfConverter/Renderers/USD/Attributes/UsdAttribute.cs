using System.Text;

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

    public abstract string Serialize();

    protected void AppendIndent(StringBuilder sb, int indentLevel)
    {
        sb.Append(new string(' ', indentLevel * 4));
    }
}
