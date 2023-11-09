using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_SID_Name_String
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlAttribute("name")]
        public string Name;

        [XmlTextAttribute()]
        public string Value;
    }
}

