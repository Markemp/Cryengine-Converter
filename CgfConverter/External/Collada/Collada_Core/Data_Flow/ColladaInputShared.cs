using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaInputShared : ColladaInputUnshared
    {
        [XmlAttribute("offset")]
        public int Offset;

        [XmlAttribute("set")]
        public int Set;

    }
}

