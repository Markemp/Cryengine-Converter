namespace CgfConverter.Collada
{
    public partial class Grendgine_Collada_Float_Array_String
    {
        public float[] Value()
        {
            return Grendgine_Collada_Parse_Utils.String_To_Float(this.Value_As_String);
        }
    }
}

