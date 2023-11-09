using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "linker", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Linker
    {
        [XmlAttribute("platform")]
        public string Platform;

        [XmlAttribute("target")]
        public string Target;

        [XmlAttribute("options")]
        public string Options;

        [XmlElement(ElementName = "binary")]
        public Grendgine_Collada_Binary[] Binary;
    }
}

