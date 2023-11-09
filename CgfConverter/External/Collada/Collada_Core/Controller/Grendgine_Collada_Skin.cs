using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Skin
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlAttribute("source")]
        public string source;

        [XmlElement(ElementName = "bind_shape_matrix")]
        public Grendgine_Collada_Float_Array_String Bind_Shape_Matrix;

        [XmlElement(ElementName = "source")]
        public ColladaSource[] Source;

        [XmlElement(ElementName = "joints")]
        public Grendgine_Collada_Joints Joints;

        [XmlElement(ElementName = "vertex_weights")]
        public Grendgine_Collada_Vertex_Weights Vertex_Weights;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

