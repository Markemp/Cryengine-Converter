using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "attachment_full", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Attachment_Full
    {
        [XmlAttribute("joint")]
        public string Joint;

        [XmlElement(ElementName = "translate")]
        public ColladaTranslate[] Translate;

        [XmlElement(ElementName = "rotate")]
        public ColladaRotate[] Rotate;

        [XmlElement(ElementName = "link")]
        public Grendgine_Collada_Link Link;

    }
}

