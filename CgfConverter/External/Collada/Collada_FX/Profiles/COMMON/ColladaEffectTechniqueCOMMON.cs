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
        public Grendgine_Collada_Blinn Blinn;

        [XmlElement(ElementName = "constant")]
        public Grendgine_Collada_Constant Constant;

        [XmlElement(ElementName = "lambert")]
        public Grendgine_Collada_Lambert Lambert;

        [XmlElement(ElementName = "phong")]
        public ColladaPhong Phong;
    }
}

