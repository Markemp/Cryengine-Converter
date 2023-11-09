using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaNurbsSurface
{
    [XmlAttribute("degree_u")]
    public int Degree_U;
    [XmlAttribute("closed_u")]
    public bool Closed_U;
    [XmlAttribute("degree_v")]
    public int Degree_V;
    [XmlAttribute("closed_v")]
    public bool Closed_V;

    [XmlElement(ElementName = "source")]
    public ColladaSource[] Source;

    [XmlElement(ElementName = "control_vertices")]
    public ColladaControlVertices Control_Vertices;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

