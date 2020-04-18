using System;
using System.Xml.Serialization;
namespace grendgine_collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_String_Array_String
    {
        //TODO: cleanup to legit array
        [XmlTextAttribute()]
        public string Value_Pre_Parse;

    }
}
