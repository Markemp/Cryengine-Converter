using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Format_Hint_Precision
    {
        DEFAULT,
        LOW,
        MID,
        HIGH,
        MAX
    }
}

