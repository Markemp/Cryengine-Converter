using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "articulated_system", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Articulated_System
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;


        [XmlElement(ElementName = "kinematics")]
        public Grendgine_Collada_Kinematics Kinematics;

        [XmlElement(ElementName = "motion")]
        public Grendgine_Collada_Motion Motion;


        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

