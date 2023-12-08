using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "texenv", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_TexEnv
    {
        [XmlAttribute("operator")]
        public Grendgine_Collada_TexEnv_Operator Operator;

        [XmlAttribute("sampler")]
        public string Sampler;

        [XmlElement(ElementName = "constant")]
        public ColladaConstantAttribute Constant;
    }
}

