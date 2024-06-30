﻿using CgfConverter.Renderers.USD.Attributes;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("Material")]
public class UsdMaterial : UsdPrim
{
    public UsdMaterial(string name) : base(name) { }

    public override string Serialize(int indentLevel)
    {
        throw new System.NotImplementedException();
    }
}
