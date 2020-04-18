using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "bind_kinematics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Bind_Kinematics_Model
    {
        [XmlAttribute("node")]
        public string Node;

        [XmlElement(ElementName = "param")]
        public Grendgine_Collada_Param Param;

        [XmlElement(ElementName = "SIDREF")]
        public string SIDREF;


    }
}

