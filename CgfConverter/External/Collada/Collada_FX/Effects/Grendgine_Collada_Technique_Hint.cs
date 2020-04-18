using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "technique_hint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_Hint
    {
        [XmlAttribute("platform")]
        public string Platform;

        [XmlAttribute("ref")]
        public string Ref;

        [XmlAttribute("profile")]
        public string Profile;


    }
}

