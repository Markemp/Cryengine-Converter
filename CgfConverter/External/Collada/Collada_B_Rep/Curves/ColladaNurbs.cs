using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaNurbs
    {
        [XmlAttribute("degree")]
        public int Degree;
        [XmlAttribute("closed")]
        public bool Closed;

        [XmlElement(ElementName = "source")]
        public ColladaSource[] Source;

        [XmlElement(ElementName = "control_vertices")]
        public ColladaControlVertices Control_Vertices;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

