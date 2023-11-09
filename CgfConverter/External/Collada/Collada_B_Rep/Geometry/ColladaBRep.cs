using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaBRep
    {


        [XmlElement(ElementName = "curves")]
        public ColladaCurves Curves;

        [XmlElement(ElementName = "surface_curves")]
        public ColladaSurfaceCurves Surface_Curves;

        [XmlElement(ElementName = "surfaces")]
        public Grendgine_Collada_Surfaces Surfaces;

        [XmlElement(ElementName = "source")]
        public ColladaSource[] Source;

        [XmlElement(ElementName = "vertices")]
        public ColladaVertices Vertices;


        [XmlElement(ElementName = "edges")]
        public Grendgine_Collada_Edges Edges;

        [XmlElement(ElementName = "wires")]
        public Grendgine_Collada_Wires Wires;

        [XmlElement(ElementName = "faces")]
        public Grendgine_Collada_Faces Faces;

        [XmlElement(ElementName = "pcurves")]
        public Grendgine_Collada_PCurves PCurves;

        [XmlElement(ElementName = "shells")]
        public Grendgine_Collada_Shells Shells;

        [XmlElement(ElementName = "solids")]
        public Grendgine_Collada_Solids Solids;




        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;

    }
}

