using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{

    /// <summary>
    /// This is the core <technique>
    /// </summary>
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique
    {
        [XmlAttribute("profile")]
        public string profile;
        [XmlAttribute("xmlns")]
        public string xmlns;

        [XmlAnyElement]
        public XmlElement[] Data;

        [XmlElement(ElementName = "bump")]
        public Grendgine_Collada_BumpMap[] Bump { get; set; }

        [XmlElement(ElementName = "user_properties")]
        public string UserProperties { get; set; }

    }
}

