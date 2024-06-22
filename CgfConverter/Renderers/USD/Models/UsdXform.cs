using CgfConverter.Renderers.USD.Attributes;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("Xform")]
public class UsdXform : UsdPrim
{
    [UsdProperty("xformOp")]
    public Matrix4x4? Transform { get; set; }

    [UsdProperty("xformOp:translate")]
    public List<string>? XformOpOrder { get; set; }

    public UsdSkelRoot? Armature { get; set; }

    public List<UsdXform>? Xforms { get; set; }

    public List<UsdScope>? Scopes { get; set; }

    public UsdXform(string name) : base(name) { }

    public override string Serialize()
    {
        var sb = new StringBuilder();

        sb.AppendLine(base.SerializeParameters());
        sb.AppendLine(SerializeChildren());

        return sb.ToString();
    }
}
