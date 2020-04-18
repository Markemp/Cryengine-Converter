using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "profile_BRIDGE", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Profile_BRIDGE : Grendgine_Collada_Profile
    {
        [XmlAttribute("platform")]
        public string Platform;

        [XmlAttribute("url")]
        public string URL;
    }
}

