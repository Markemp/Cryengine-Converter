using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "instance_image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Image
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("url")]
        public string URL;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

