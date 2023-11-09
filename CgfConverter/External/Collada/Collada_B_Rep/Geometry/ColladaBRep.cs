using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaBRep
{
    [XmlElement(ElementName = "curves")]
    public ColladaCurves Curves;

    [XmlElement(ElementName = "surface_curves")]
    public ColladaSurfaceCurves Surface_Curves;

    [XmlElement(ElementName = "surfaces")]
    public ColladaSurfaces Surfaces;

    [XmlElement(ElementName = "source")]
    public ColladaSource[] Source;

    [XmlElement(ElementName = "vertices")]
    public ColladaVertices Vertices;


    [XmlElement(ElementName = "edges")]
    public ColladaEdges Edges;

    [XmlElement(ElementName = "wires")]
    public ColladaWires Wires;

    [XmlElement(ElementName = "faces")]
    public ColladaFaces Faces;

    [XmlElement(ElementName = "pcurves")]
    public ColladaPCurves PCurves;

    [XmlElement(ElementName = "shells")]
    public ColladaShells Shells;

    [XmlElement(ElementName = "solids")]
    public ColladaSolids Solids;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

