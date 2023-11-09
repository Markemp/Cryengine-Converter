using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_FX_Opaque_Channel
    {
        A_ONE,
        RGB_ZERO,
        A_ZERO,
        RGB_ONE
    }
}

