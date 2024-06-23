using System;

namespace CgfConverter.Renderers.USD.Models;
public class UsdMesh : UsdPrim
{
    public UsdMesh(string name) : base(name)
    {
        Name = name;
    }

    public override string Serialize(int indentLevel)
    {
        throw new NotImplementedException();
    }
}
