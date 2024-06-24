using CgfConverter.Renderers.USD.Attributes;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("Scope")]
public class UsdScope : UsdPrim
{
    public UsdScope(string name) : base(name) { }

    public override string Serialize(int indentLevel)
    {
        throw new System.NotImplementedException();
    }
}
