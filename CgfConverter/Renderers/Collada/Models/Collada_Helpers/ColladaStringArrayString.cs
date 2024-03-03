namespace CgfConverter.Collada;

public partial class ColladaStringArrayString
{
    public string[] Value()
    {
        return this.Value_Pre_Parse.Split(' ');
    }
}
