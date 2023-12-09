using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "physics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaPhysicsScene
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;


    [XmlElement(ElementName = "instance_force_field")]
    public ColladaInstanceForceField[] Instance_Force_Field;

    [XmlElement(ElementName = "instance_physics_model")]
    public ColladaInstancePhysicsModel[] Instance_Physics_Model;

    [XmlElement(ElementName = "technique_common")]
    public ColladaTechniqueCommonPhysicsScene Technique_Common;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

