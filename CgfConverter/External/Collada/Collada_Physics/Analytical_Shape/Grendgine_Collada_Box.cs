using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "box", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Box
    {
        [XmlElement(ElementName = "half_extents")]
        public ColladaFloatArrayString Half_Extents;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

