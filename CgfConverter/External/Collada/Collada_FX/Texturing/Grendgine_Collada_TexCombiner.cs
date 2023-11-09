using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "texcombiner", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_TexCombiner
    {

        [XmlElement(ElementName = "constant")]
        public Grendgine_Collada_Constant_Attribute Constant;

        [XmlElement(ElementName = "RGB")]
        public Grendgine_Collada_RGB RGB;

        [XmlElement(ElementName = "alpha")]
        public Grendgine_Collada_Alpha Alpha;
    }
}

