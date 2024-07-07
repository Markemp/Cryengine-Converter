using CgfConverter.Renderers.USD.Attributes;
using System.Collections.Generic;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("GeomSubset")]
public class UsdGeomSubset : UsdPrim
{
    public UsdGeomSubset(string name, List<UsdProperty>? properties = null) : base(name, properties) { }

    public override string Serialize(int indentLevel)
    {
        throw new System.NotImplementedException();
    }
}
