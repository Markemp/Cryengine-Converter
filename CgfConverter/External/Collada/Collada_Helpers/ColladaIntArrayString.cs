namespace CgfConverter.Collada;

public partial class ColladaIntArrayString
{
    public int[] Value()
    {
        return ColladaParseUtils.String_To_Int(this.Value_As_String);
    }

}

