using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "physics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaPhysicsModel
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlElement(ElementName = "rigid_body")]
    public ColladaRigidBody[] Rigid_Body;

    [XmlElement(ElementName = "rigid_constraint")]
    public ColladaRigidConstraint[] Rigid_Constraint;

    [XmlElement(ElementName = "instance_physics_model")]
    public ColladaInstancePhysicsModel[] Instance_Physics_Model;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

