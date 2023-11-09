using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Library_Lights
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;


        [XmlElement(ElementName = "light")]
        public Grendgine_Collada_Light[] Light;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

