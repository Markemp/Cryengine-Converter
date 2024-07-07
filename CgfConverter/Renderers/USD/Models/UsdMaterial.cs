using CgfConverter.Renderers.USD.Attributes;
using System.Collections.Generic;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("Material")]
public class UsdMaterial : UsdPrim
{
    public UsdMaterial(string name, List<UsdProperty>? properties = null) : base(name, properties) { }

    public override string Serialize(int indentLevel)
    {
        throw new System.NotImplementedException();
    }
}
