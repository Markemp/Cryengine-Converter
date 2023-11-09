using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "size_ratio", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Size_Ratio
    {
        [XmlAttribute("width")]
        public float Width;

        [XmlAttribute("height")]
        public float Height;
    }
}

