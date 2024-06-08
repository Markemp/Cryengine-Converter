using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Transformation;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Analytical_Shape;

namespace CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Surfaces;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSurface
{
    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("sid")]
    public string sID;

    // ggerber 1.4.1 attribue
    [XmlAttribute("type")]
    public string Type;

    [XmlElement(ElementName = "cone")]
    public ColladaCone Cone;

    [XmlElement(ElementName = "plane")]
    public ColladaPlane Plane;

    [XmlElement(ElementName = "cylinder")]
    public ColladaCylinderBRep Cylinder;

    [XmlElement(ElementName = "nurbs_surface")]
    public ColladaNurbsSurface Nurbs_Surface;

    [XmlElement(ElementName = "sphere")]
    public ColladaSphere Sphere;

    [XmlElement(ElementName = "torus")]
    public ColladaTorus Torus;

    [XmlElement(ElementName = "swept_surface")]
    public ColladaSweptSurface Swept_Surface;

    [XmlElement(ElementName = "orient")]
    public ColladaOrient[] Orient;

    [XmlElement(ElementName = "origin")]
    public ColladaOrigin Origin;

    //ggerber 1.4.1
    [XmlElement(ElementName = "init_from")]
    public ColladaInitFrom Init_From;

}
