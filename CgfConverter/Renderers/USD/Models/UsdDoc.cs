using System.Collections.Generic;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

public class UsdDoc
{
    public UsdHeader Header { get; set; } = new();

    public List<UsdPrim> Prims { get; set; } = new();

    // fields
    private StringBuilder _sb = new();
}
