namespace CgfConverter.Collada
{

    public partial class ColladaBoolArrayString
    {
        public bool[] Value()
        {
            return Grendgine_Collada_Parse_Utils.String_To_Bool(this.Value_As_String);
        }


    }
}