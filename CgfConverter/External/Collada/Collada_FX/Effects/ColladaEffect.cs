using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "effect", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaEffect
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "annotate")]
        public ColladaAnnotate[] Annotate;

        [XmlElement(ElementName = "newparam")]
        public ColladaNewParam[] New_Param;

        [XmlElement(ElementName = "profile_BRIDGE")]
        public ColladaProfileBRIDGE[] Profile_BRIDGE;

        [XmlElement(ElementName = "profile_CG")]
        public ColladaProfileCG[] Profile_CG;

        [XmlElement(ElementName = "profile_GLES")]
        public ColladaProfileGLES[] Profile_GLES;

        [XmlElement(ElementName = "profile_GLES2")]
        public ColladaProfileGLES2[] Profile_GLES2;

        [XmlElement(ElementName = "profile_GLSL")]
        public ColladaProfileGLSL[] Profile_GLSL;

        [XmlElement(ElementName = "profile_COMMON")]
        public ColladaProfileCOMMON[] Profile_COMMON;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

