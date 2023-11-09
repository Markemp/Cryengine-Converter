using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_Common_Kinematics_Model : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "newparam")]
        public ColladaNewParam[] New_Param;

        [XmlElement(ElementName = "joint")]
        public Grendgine_Collada_Joint[] Joint;

        [XmlElement(ElementName = "instance_joint")]
        public Grendgine_Collada_Instance_Joint[] Instance_Joint;

        [XmlElement(ElementName = "link")]
        public Grendgine_Collada_Link[] Link;

        [XmlElement(ElementName = "formula")]
        public Grendgine_Collada_Formula[] Formula;

        [XmlElement(ElementName = "instance_formula")]
        public ColladaInstanceFormula[] Instance_Formula;

    }
}

