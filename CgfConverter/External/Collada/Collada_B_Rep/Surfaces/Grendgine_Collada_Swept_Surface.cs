using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Swept_Surface
    {

        [XmlElement(ElementName = "curve")]
        public ColladaCurve Curve;

        [XmlElement(ElementName = "origin")]
        public ColladaOrigin Origin;

        [XmlElement(ElementName = "direction")]
        public Grendgine_Collada_Float_Array_String Direction;

        [XmlElement(ElementName = "axis")]
        public Grendgine_Collada_Float_Array_String Axis;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

