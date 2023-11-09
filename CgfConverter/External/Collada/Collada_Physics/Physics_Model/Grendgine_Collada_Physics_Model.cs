using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "physics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Physics_Model
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;

        [XmlElement(ElementName = "rigid_body")]
        public Grendgine_Collada_Rigid_Body[] Rigid_Body;

        [XmlElement(ElementName = "rigid_constraint")]
        public Grendgine_Collada_Rigid_Constraint[] Rigid_Constraint;

        [XmlElement(ElementName = "instance_physics_model")]
        public Grendgine_Collada_Instance_Physics_Model[] Instance_Physics_Model;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

