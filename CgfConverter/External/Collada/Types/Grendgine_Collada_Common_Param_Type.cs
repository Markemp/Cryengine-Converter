using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Common_Param_Type
    {
        [XmlElement(ElementName = "param")]
        public Grendgine_Collada_Param Param;
    }
}

