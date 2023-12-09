using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "instance_physics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaInstancePhysicsModel
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("url")]
    public string URL;

    [XmlAttribute("parent")]
    public string Parent;

    [XmlElement(ElementName = "instance_force_field")]
    public ColladaInstanceForceField[] Instance_Force_Field;

    [XmlElement(ElementName = "instance_rigid_body")]
    public ColladaInstanceRigidBody[] Instance_Rigid_Body;

    [XmlElement(ElementName = "instance_rigid_constraint")]
    public ColladaInstanceRigidConstraint[] Instance_Rigid_Constraint;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

