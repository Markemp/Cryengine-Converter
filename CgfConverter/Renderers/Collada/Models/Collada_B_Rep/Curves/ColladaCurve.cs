using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Transformation;

namespace CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Curves;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCurve
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlElement(ElementName = "line")]
    public ColladaLine Line;

    [XmlElement(ElementName = "circle")]
    public ColladaCircle Circle;

    [XmlElement(ElementName = "ellipse")]
    public ColladaEllipse Ellipse;

    [XmlElement(ElementName = "parabola")]
    public ColladaParabola Parabola;

    [XmlElement(ElementName = "hyperbola")]
    public ColladaHyperbola Hyperbola;

    [XmlElement(ElementName = "nurbs")]
    public ColladaNurbs Nurbs;


    [XmlElement(ElementName = "orient")]
    public ColladaOrient[] Orient;

    [XmlElement(ElementName = "origin")]
    public ColladaOrigin Origin;
}

