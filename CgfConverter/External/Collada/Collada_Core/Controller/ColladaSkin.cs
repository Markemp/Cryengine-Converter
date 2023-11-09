using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSkin
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("source")]
    public string source;

    [XmlElement(ElementName = "bind_shape_matrix")]
    public ColladaFloatArrayString Bind_Shape_Matrix;

    [XmlElement(ElementName = "source")]
    public ColladaSource[] Source;

    [XmlElement(ElementName = "joints")]
    public ColladaJoints Joints;

    [XmlElement(ElementName = "vertex_weights")]
    public ColladaVertexWeights Vertex_Weights;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

