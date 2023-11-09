using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Physics_Material : Grendgine_Collada_Technique_Common
    {

        [XmlElement(ElementName = "dynamic_friction")]
        public ColladaSIDFloat Dynamic_Friction;

        [XmlElement(ElementName = "restitution")]
        public ColladaSIDFloat Restitution;

        [XmlElement(ElementName = "static_friction")]
        public ColladaSIDFloat Static_Friction;
    }
}

