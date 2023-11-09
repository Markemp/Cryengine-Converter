using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "ref_attachment", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Ref_Attachment
    {
        [XmlAttribute("rigid_body")]
        public string Rigid_Body;

        [XmlElement(ElementName = "translate")]
        public ColladaTranslate[] Translate;

        [XmlElement(ElementName = "rotate")]
        public ColladaRotate[] Rotate;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

