using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Shells
    {
        [XmlAttribute("count")]
        public int Count;

        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("id")]
        public string ID;

        [XmlElement(ElementName = "vcount")]
        public ColladaIntArrayString VCount;

        [XmlElement(ElementName = "p")]
        public ColladaIntArrayString P;

        [XmlElement(ElementName = "input")]
        public ColladaInputShared[] Input;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

