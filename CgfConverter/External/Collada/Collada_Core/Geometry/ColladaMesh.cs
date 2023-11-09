using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaMesh
{
    [XmlElement(ElementName = "source")]
    public ColladaSource[] Source;

    [XmlElement(ElementName = "vertices")]
    public ColladaVertices Vertices;

    [XmlElement(ElementName = "lines")]
    public ColladaLines[] Lines;

    [XmlElement(ElementName = "linestrips")]
    public ColladaLinestrips[] Linestrips;

    [XmlElement(ElementName = "polygons")]
    public ColladaPolygons[] Polygons;

    [XmlElement(ElementName = "polylist")]
    public ColladaPolylist[] Polylist;

    [XmlElement(ElementName = "triangles")]
    public ColladaTriangles[] Triangles;

    [XmlElement(ElementName = "trifans")]
    public ColladaTrifans[] Trifans;

    [XmlElement(ElementName = "tristrips")]
    public ColladaTristrips[] Tristrips;



    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

