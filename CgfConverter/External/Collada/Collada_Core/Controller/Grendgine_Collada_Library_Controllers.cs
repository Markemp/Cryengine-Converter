using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Library_Controllers
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;


        [XmlElement(ElementName = "controller")]
        public Grendgine_Collada_Controller[] Controller;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

