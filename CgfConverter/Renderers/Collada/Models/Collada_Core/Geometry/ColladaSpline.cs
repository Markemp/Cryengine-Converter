using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSpline
{
    [XmlAttribute("closed")]
    public bool Closed;


    [XmlElement(ElementName = "source")]
    public ColladaSource[] Source;

    [XmlElement(ElementName = "control_vertices")]
    public ColladaControlVertices Control_Vertices;


    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

