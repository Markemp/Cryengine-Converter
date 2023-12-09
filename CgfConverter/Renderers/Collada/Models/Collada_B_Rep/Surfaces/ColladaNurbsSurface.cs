using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;

namespace CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Surfaces;

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

