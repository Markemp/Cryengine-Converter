using CgfConverter.Renderers.USD.Attributes;
using System.Collections.Generic;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// SkelRoot prim marks the scope of skeletal data.
/// Must be parent to both Skeleton and skinned Meshes.
/// </summary>
[UsdElement("SkelRoot")]
public class UsdSkelRoot : UsdPrim
{
    public UsdSkelRoot(string name, List<UsdProperty>? properties = null) : base(name, properties)
    {
    }

    public override string Serialize(int indentLevel)
    {
        // Serialization handled by UsdSerializer using reflection
        return string.Empty;
    }
}
