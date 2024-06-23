namespace CgfConverter.Renderers.USD.Models;

public class UsdShader : UsdPrim
{
    public string? InfoId { get; set; }

    public UsdShader(string name) : base(name) { }

    public override string Serialize(int indentLevel)
    {
        throw new System.NotImplementedException();
    }
}
