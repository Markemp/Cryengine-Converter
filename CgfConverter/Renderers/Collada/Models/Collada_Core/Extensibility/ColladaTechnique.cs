using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility
{
    /// <summary>
    /// This is the core <technique>
    /// </summary>
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaTechnique
    {
        [XmlAttribute("profile")]
        public string profile;
        [XmlAttribute("xmlns")]
        public string xmlns;

        [XmlAnyElement]
        public XmlElement[] Data;

        [XmlElement(ElementName = "bump")]
        public ColladaBumpMap[] Bump { get; set; }

        [XmlElement(ElementName = "user_properties")]
        public string UserProperties { get; set; }

    }
}

