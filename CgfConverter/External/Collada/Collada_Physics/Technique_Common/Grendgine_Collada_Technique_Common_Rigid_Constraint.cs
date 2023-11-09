using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Rigid_Constraint : Grendgine_Collada_Technique_Common
    {

        [XmlElement(ElementName = "enabled")]
        public Grendgine_Collada_SID_Bool Enabled;

        [XmlElement(ElementName = "interpenetrate")]
        public Grendgine_Collada_SID_Bool Interpenetrate;

        [XmlElement(ElementName = "limits")]
        public Grendgine_Collada_Constraint_Limits Limits;


        [XmlElement(ElementName = "spring")]
        public Grendgine_Collada_Constraint_Spring Spring;


    }
}

