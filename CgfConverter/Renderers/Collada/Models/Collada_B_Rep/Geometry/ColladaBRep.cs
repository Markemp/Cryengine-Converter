using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Curves;
using CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Surfaces;
using CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Topology;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;

namespace CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Geometry;

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

