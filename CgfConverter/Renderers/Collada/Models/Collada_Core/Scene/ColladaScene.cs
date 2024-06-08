using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Scenes;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Physics_Scene;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Scene;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaScene
{

    [XmlElement(ElementName = "instance_visual_scene")]
    public ColladaInstanceVisualScene Visual_Scene;

    [XmlElement(ElementName = "instance_physics_scene")]
    public ColladaInstancePhysicsScene[] Physics_Scene;

    [XmlElement(ElementName = "instance_kinematics_scene")]
    public ColladaInstanceKinematicsScene Kinematics_Scene;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

