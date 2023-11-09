using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]

    public partial class Grendgine_Collada_Torus
    {
        [XmlElement(ElementName = "radius")]
        public Grendgine_Collada_Float_Array_String Radius;


        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

