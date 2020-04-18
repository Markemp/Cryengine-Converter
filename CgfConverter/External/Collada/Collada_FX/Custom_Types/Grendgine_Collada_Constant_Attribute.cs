using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Constant_Attribute
    {
        [XmlAttribute("value")]
        public string Value_As_String;
        //TODO: needs to be 4 float array

        [XmlAttribute("param")]
        public string Param_As_String;
        //TODO: needs to be 4 float array
    }
}

