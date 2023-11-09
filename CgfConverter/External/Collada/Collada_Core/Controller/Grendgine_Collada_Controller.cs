using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Controller
    {

        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;


        [XmlElement(ElementName = "skin")]
        public Grendgine_Collada_Skin Skin;

        [XmlElement(ElementName = "morph")]
        public Grendgine_Collada_Morph Morph;


        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

