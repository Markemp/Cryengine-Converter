using CgfConverter.Renderers.USD.Attributes;
using System.Collections.Generic;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// SkelAnimation prim contains animation data for skeletal animation.
/// Stores joint transforms as time-sampled arrays.
/// </summary>
[UsdElement("SkelAnimation")]
public class UsdSkelAnimation : UsdPrim
{
    public UsdSkelAnimation(string name, List<UsdProperty>? properties = null) : base(name, properties)
    {
    }

    public override string Serialize(int indentLevel)
    {
        // Serialization handled by UsdSerializer using reflection
        return string.Empty;
    }
}
