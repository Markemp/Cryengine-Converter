namespace CgfConverter.Renderers.USD.Models;

public class UsdMaterial : UsdPrim
{
    public string? OutputsSurfaceConnect { get; set; }

    public UsdMaterial(string name) : base(name) { }

    public override string Serialize()
    {
        throw new System.NotImplementedException();
    }
}
