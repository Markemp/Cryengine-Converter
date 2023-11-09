using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "frame_origin", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public class Grendgine_Collada_Frame_Origin
    {
        [XmlAttribute("link")]
        public string Link;

        [XmlElement(ElementName = "translate")]
        public ColladaTranslate[] Translate;

        [XmlElement(ElementName = "rotate")]
        public ColladaRotate[] Rotate;
    }
}

