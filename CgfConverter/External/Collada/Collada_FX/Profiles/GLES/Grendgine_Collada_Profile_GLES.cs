using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "profile_GLES", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Profile_GLES : Grendgine_Collada_Profile
    {
        [XmlAttribute("platform")]
        public string Platform;

        [XmlElement(ElementName = "newparam")]
        public ColladaNewParam[] New_Param;

        [XmlElement(ElementName = "technique")]
        public Grendgine_Collada_Technique_GLES[] Technique;
    }
}

