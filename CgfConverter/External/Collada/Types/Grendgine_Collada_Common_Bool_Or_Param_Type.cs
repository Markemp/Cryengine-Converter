using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Common_Bool_Or_Param_Type : Grendgine_Collada_Common_Param_Type
    {
        [XmlElement(ElementName = "bool")]
        public bool Bool;
    }
}

