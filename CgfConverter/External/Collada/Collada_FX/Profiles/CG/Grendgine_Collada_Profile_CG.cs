using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "profile_CG", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Profile_CG : Grendgine_Collada_Profile
    {
        [XmlAttribute("platform")]
        public string Platform;

        [XmlElement(ElementName = "newparam")]
        public Grendgine_Collada_New_Param[] New_Param;

        [XmlElement(ElementName = "technique")]
        public Grendgine_Collada_Technique_CG[] Technique;

        [XmlElement(ElementName = "code")]
        public Grendgine_Collada_Code[] Code;

        [XmlElement(ElementName = "include")]
        public Grendgine_Collada_Include[] Include;
    }
}

