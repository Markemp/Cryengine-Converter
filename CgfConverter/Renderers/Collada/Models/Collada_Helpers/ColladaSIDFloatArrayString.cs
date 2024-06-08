using CgfConverter.Renderers.Collada.Collada.Collada_Helpers;

namespace CgfConverter.Collada;

public partial class ColladaSIDFloatArrayString
{
    public float[] Value()
    {
        return ColladaParseUtils.String_To_Float(this.Value_As_String);
    }

}

