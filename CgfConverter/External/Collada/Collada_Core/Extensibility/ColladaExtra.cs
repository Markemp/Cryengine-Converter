using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaExtra
    {

        [XmlAttribute("id")]
        public string ID;
        [XmlAttribute("name")]
        public string Name;
        [XmlAttribute("type")]
        public string Type;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "technique")]
        public Grendgine_Collada_Technique[] Technique;
    }
}

