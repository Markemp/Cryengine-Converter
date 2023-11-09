using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
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
        public ColladaInputShared[] Targets;

        [XmlElement(ElementName = "source")]
        public ColladaSource[] Source;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

