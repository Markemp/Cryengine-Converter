using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_SID_Float_Array_String
    {
        [XmlAttribute("sid")]
        public string sID;

        //TODO: cleanup to legit array

        [XmlTextAttribute()]
        public string Value_As_String;
    }
}

