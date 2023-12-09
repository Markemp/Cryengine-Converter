using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Effects;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.COMMON
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

