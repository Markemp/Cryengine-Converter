using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "rigid_body", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Rigid_Body
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("sid")]
        public string sID;

        [XmlAttribute("name")]
        public string Name;


        [XmlElement(ElementName = "technique_common")]
        public Grendgine_Collada_Technique_Common_Rigid_Body Technique_Common;

        [XmlElement(ElementName = "technique")]
        public ColladaTechnique[] Technique;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

