using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Formula
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("sid")]
        public string sID;


        [XmlElement(ElementName = "newparam")]
        public ColladaNewParam[] New_Param;

        [XmlElement(ElementName = "technique_common")]
        public Grendgine_Collada_Technique_Common_Formula Technique_Common;

        [XmlElement(ElementName = "technique")]
        public ColladaTechnique[] Technique;


        [XmlElement(ElementName = "target")]
        public Grendgine_Collada_Common_Float_Or_Param_Type Target;

    }
}

