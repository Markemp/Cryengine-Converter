using CgfConverter.Renderers.USD.Attributes;
using System.Collections.Generic;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// Skeleton prim contains joint hierarchy and bind pose data.
/// Requires SkelBindingAPI schema.
/// </summary>
[UsdElement("Skeleton")]
public class UsdSkeleton : UsdPrim
{
    public UsdSkeleton(string name, List<UsdProperty>? properties = null) : base(name, properties)
    {
        // Add SkelBindingAPI schema by default
        if (properties == null)
        {
            Properties = new List<UsdProperty>
            {
                new UsdProperty(new Dictionary<string, object>
                {
                    ["apiSchemas"] = "[\"SkelBindingAPI\"]"
                }, true)
            };
        }
    }

    public override string Serialize(int indentLevel)
    {
        // Serialization handled by UsdSerializer using reflection
        return string.Empty;
    }
}
