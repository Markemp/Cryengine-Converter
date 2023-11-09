using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Vertex_Weights
    {
        [XmlAttribute("count")]
        public int Count;

        [XmlElement(ElementName = "input")]
        public ColladaInputShared[] Input;

        [XmlElement(ElementName = "vcount")]
        public ColladaIntArrayString VCount;

        [XmlElement(ElementName = "v")]
        public ColladaIntArrayString V;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

