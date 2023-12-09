using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaEffectTechniqueCOMMON : ColladaEffectTechnique
    {

        [XmlElement(ElementName = "blinn")]
        public ColladaBlinn Blinn;

        [XmlElement(ElementName = "constant")]
        public ColladaConstant Constant;

        [XmlElement(ElementName = "lambert")]
        public ColladaLambert Lambert;

        [XmlElement(ElementName = "phong")]
        public ColladaPhong Phong;
    }
}

