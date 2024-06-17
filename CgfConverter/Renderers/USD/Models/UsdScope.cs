using System.Collections.Generic;

namespace CgfConverter.Renderers.USD.Models;

public class UsdScope : UsdPrim
{
    public UsdScope(string name) : base(name) { }

    public override string Serialize()
    {
        throw new System.NotImplementedException();
    }
}
