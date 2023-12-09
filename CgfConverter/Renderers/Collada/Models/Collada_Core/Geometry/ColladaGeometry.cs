using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Geometry;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Analytical_Shape;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaGeometry
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlElement(ElementName = "brep")]
    public ColladaBRep B_Rep;

    [XmlElement(ElementName = "convex_mesh")]
    public ColladaConvexMesh Convex_Mesh;

    [XmlElement(ElementName = "spline")]
    public ColladaSpline Spline;

    [XmlElement(ElementName = "mesh")]
    public ColladaMesh Mesh;


    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

