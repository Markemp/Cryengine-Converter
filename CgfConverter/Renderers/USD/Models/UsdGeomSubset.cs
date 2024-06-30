using CgfConverter.Renderers.USD.Attributes;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("GeomSubset")]
public class UsdGeomSubset : UsdPrim
{
    public UsdGeomSubset(string name) : base(name) { }

    public override string Serialize(int indentLevel)
    {
        throw new System.NotImplementedException();
    }
}
