using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Channel
    {
        [XmlAttribute("source")]
        public string Source;

        [XmlAttribute("target")]
        public string Target;

    }
}

