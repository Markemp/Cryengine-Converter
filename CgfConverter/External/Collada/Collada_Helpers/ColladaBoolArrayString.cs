namespace CgfConverter.Collada;

public partial class ColladaBoolArrayString
{
    public bool[] Value()
    {
        return ColladaParseUtils.String_To_Bool(this.Value_As_String);
    }


}
