using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Format_Hint
    {
        [XmlAttribute("channels")]
        public Grendgine_Collada_Format_Hint_Channels Channels;

        [XmlAttribute("range")]
        public Grendgine_Collada_Format_Hint_Range Range;

        [XmlAttribute("precision")]
        [System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Format_Hint_Precision.DEFAULT)]
        public Grendgine_Collada_Format_Hint_Precision Precision;

        [XmlAttribute("space")]
        public string Hint_Space;

    }
}

