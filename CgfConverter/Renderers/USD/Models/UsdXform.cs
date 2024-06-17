using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.Renderers.USD.Models;

public class UsdXform : UsdPrim
{
    public Matrix4x4? Transform { get; set; }
    public List<string>? XformOpOrder { get; set; }
    public UsdSkelRoot? Armature { get; set; }
    public List<UsdXform>? Xforms { get; set; }
    public List<UsdScope>? Scopes { get; set; }

    public UsdXform(string name) : base(name) { }

    public override string Serialize()
    {
        throw new System.NotImplementedException();
    }
}
