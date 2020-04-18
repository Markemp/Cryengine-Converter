using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "usertype", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_UserType
    {
        [XmlAttribute("typename")]
        public string TypeName;

        [XmlAttribute("source")]
        public string Source;

        [XmlElement(ElementName = "setparam")]
        public Grendgine_Collada_Set_Param[] SetParam;
    }
}

