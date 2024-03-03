using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;
namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Analytical_Shape
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "convex_mesh", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaConvexMesh
    {

        [XmlAttribute("convex_hull_of")]
        public string Convex_Hull_Of;

        [XmlElement(ElementName = "source")]
        public ColladaSource[] Source;

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


        [XmlElement(ElementName = "vertices")]
        public ColladaVertices Vertices;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

