using System.Collections.Generic;

namespace CgfConverter.Renderers.USD.Attributes;

public class UsdProperty
{
    public bool IsPrepend { get; set; }

    public Dictionary<string, object>? Properties { get; set; }

    public UsdProperty(Dictionary<string, object>? properties = null, bool isPrepend = false)
    {
        IsPrepend = isPrepend;
        Properties = properties;
    }
}
