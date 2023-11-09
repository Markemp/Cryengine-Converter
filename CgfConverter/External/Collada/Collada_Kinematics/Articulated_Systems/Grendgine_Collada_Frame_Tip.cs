using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "frame_tip", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Frame_Tip
    {
        [XmlAttribute("link")]
        public string Link;

        [XmlElement(ElementName = "translate")]
        public ColladaTranslate[] Translate;

        [XmlElement(ElementName = "rotate")]
        public ColladaRotate[] Rotate;
    }
}

