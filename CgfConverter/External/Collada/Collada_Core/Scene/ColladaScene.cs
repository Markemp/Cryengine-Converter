using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaScene
{

    [XmlElement(ElementName = "instance_visual_scene")]
    public ColladaInstanceVisualScene Visual_Scene;

    [XmlElement(ElementName = "instance_physics_scene")]
    public Grendgine_Collada_Instance_Physics_Scene[] Physics_Scene;

    [XmlElement(ElementName = "instance_kinematics_scene")]
    public Grendgine_Collada_Instance_Kinematics_Scene Kinematics_Scene;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

