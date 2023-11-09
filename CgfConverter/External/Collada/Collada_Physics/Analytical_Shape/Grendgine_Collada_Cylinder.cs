using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "cylinder", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Cylinder
    {
        [XmlElement(ElementName = "height")]
        public float Height;

        [XmlElement(ElementName = "radius")]
        public Grendgine_Collada_Float_Array_String Radius;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

