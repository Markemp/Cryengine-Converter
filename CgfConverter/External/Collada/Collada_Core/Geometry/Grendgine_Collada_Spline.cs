using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Spline
    {
        [XmlAttribute("closed")]
        public bool Closed;


        [XmlElement(ElementName = "source")]
        public Grendgine_Collada_Source[] Source;

        [XmlElement(ElementName = "control_vertices")]
        public Grendgine_Collada_Control_Vertices Control_Vertices;


        [XmlElement(ElementName = "extra")]
        public Grendgine_Collada_Extra[] Extra;
    }
}

