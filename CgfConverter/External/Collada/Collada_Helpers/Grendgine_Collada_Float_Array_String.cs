namespace CgfConverter.Collada
{
    public partial class ColladaFloatArrayString
    {
        public float[] Value()
        {
            return Grendgine_Collada_Parse_Utils.String_To_Float(this.Value_As_String);
        }
    }
}

