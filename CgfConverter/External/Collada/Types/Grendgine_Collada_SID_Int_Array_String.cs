using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_SID_Int_Array_String
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlTextAttribute()]
        public string Value_As_String;
    }
}

