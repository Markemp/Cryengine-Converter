using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Morph
    {

        [XmlAttribute("source")]
        public string Source_Attribute;

        [XmlAttribute("method")]
        public string Method;

        [XmlArray("targets")]
        public Grendgine_Collada_Input_Shared[] Targets;

        [XmlElement(ElementName = "source")]
        public Grendgine_Collada_Source[] Source;

        [XmlElement(ElementName = "extra")]
        public Grendgine_Collada_Extra[] Extra;
    }
}

