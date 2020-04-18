using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "attachment_full", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Attachment_Full
    {
        [XmlAttribute("joint")]
        public string Joint;

        [XmlElement(ElementName = "translate")]
        public Grendgine_Collada_Translate[] Translate;

        [XmlElement(ElementName = "rotate")]
        public Grendgine_Collada_Rotate[] Rotate;

        [XmlElement(ElementName = "link")]
        public Grendgine_Collada_Link Link;

    }
}

