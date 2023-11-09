using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Perspective
    {
        [XmlElement(ElementName = "xfov")]
        public ColladaSIDFloat XFov;

        [XmlElement(ElementName = "yfov")]
        public ColladaSIDFloat YFov;

        [XmlElement(ElementName = "aspect_ratio")]
        public ColladaSIDFloat Aspect_Ratio;

        [XmlElement(ElementName = "znear")]
        public ColladaSIDFloat ZNear;

        [XmlElement(ElementName = "zfar")]
        public ColladaSIDFloat ZFar;
    }
}

