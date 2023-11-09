using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "instance_effect", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaInstanceEffect
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("url")]
        public string URL;

        [XmlElement(ElementName = "setparam")]
        public Grendgine_Collada_Set_Param[] Set_Param;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;

        [XmlElement(ElementName = "technique_hint")]
        public Grendgine_Collada_Technique_Hint[] Technique_Hint;

    }
}

