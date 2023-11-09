using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Format_Hint_Channels
    {
        RGB,
        RGBA,
        RGBE,
        L,
        LA,
        D
    }
}

