using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaSIDFloat
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlTextAttribute()]
        public float Value;

    }
}

