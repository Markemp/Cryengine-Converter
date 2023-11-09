namespace CgfConverter.Collada
{
    public partial class ColladaIntArrayString
    {
        public int[] Value()
        {
            return Grendgine_Collada_Parse_Utils.String_To_Int(this.Value_As_String);
        }

    }
}

