using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaKinematicsScene
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlElement(ElementName = "instance_kinematics_model")]
    public ColladaInstanceKinematicsModel[] Instance_Kinematics_Model;

    [XmlElement(ElementName = "instance_articulated_system")]
    public ColladaInstanceArticulatedSystem[] Instance_Articulated_System;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

