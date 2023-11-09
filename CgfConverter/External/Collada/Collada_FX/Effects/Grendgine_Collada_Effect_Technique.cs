using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Effect_Technique
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlAttribute("id")]
        public string id;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

