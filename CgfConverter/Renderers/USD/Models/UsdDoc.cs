using System.Collections.Generic;

namespace CgfConverter.Renderers.USD.Models;

public class UsdDoc
{
    public UsdHeader Header { get; set; } = new();

    public List<UsdPrim> Prims { get; set; } = new();
}
