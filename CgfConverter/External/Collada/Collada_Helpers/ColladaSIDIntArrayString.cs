namespace CgfConverter.Collada;

public partial class ColladaSIDIntArrayString
{
    public int[] Value()
    {
        return ColladaParseUtils.String_To_Int(this.Value_As_String);
    }

}

