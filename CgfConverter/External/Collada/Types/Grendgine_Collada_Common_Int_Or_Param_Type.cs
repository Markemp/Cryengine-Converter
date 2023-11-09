using System;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Common_Int_Or_Param_Type : Grendgine_Collada_Common_Param_Type
    {
        [XmlTextAttribute()]
        public string Value_As_String;

    }
}

