using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

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
    public Grendgine_Collada_Plane Plane;

    [XmlElement(ElementName = "cylinder")]
    public ColladaCylinderBRep Cylinder;

    [XmlElement(ElementName = "nurbs_surface")]
    public ColladaNurbsSurface Nurbs_Surface;

    [XmlElement(ElementName = "sphere")]
    public Grendgine_Collada_Sphere Sphere;

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

