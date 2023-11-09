using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "profile_GLES2", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Profile_GLES2 : Grendgine_Collada_Profile
    {
        [XmlAttribute("platform")]
        public string Platform;

        [XmlAttribute("language")]
        public string Language;

        [XmlElement(ElementName = "newparam")]
        public ColladaNewParam[] New_Param;

        [XmlElement(ElementName = "technique")]
        public Grendgine_Collada_Technique_GLES2[] Technique;

        [XmlElement(ElementName = "code")]
        public Grendgine_Collada_Code[] Code;

        [XmlElement(ElementName = "include")]
        public Grendgine_Collada_Include[] Include;
    }
}

