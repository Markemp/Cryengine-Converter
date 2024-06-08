using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Articulated_Systems;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Models;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Scenes;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

