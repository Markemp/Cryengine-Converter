using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "constant", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Constant
    {
        [XmlElement(ElementName = "emission")]
        public ColladaFXCommonColorOrTextureType Eission;

        [XmlElement(ElementName = "reflective")]
        public ColladaFXCommonColorOrTextureType Reflective;

        [XmlElement(ElementName = "reflectivity")]
        public ColladaFXCommonFloatOrParamType Reflectivity;



        [XmlElement(ElementName = "transparent")]
        public ColladaFXCommonColorOrTextureType Transparent;

        [XmlElement(ElementName = "transparency")]
        public ColladaFXCommonFloatOrParamType Transparency;

        [XmlElement(ElementName = "index_of_refraction")]
        public ColladaFXCommonFloatOrParamType Index_Of_Refraction;
    }
}

