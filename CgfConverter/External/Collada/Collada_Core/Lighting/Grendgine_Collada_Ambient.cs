using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Ambient
    {
        [XmlElement(ElementName = "color")]
        public Grendgine_Collada_Color Color;

    }
}

