using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Scenes;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "instance_kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaInstanceKinematicsScene
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("url")]
    public string URL;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;

    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "setparam")]
    public ColladaSetParam[] Set_Param;


    [XmlElement(ElementName = "bind_kinematics_model")]
    public ColladaBindKinematicsModel[] Bind_Kenematics_Model;

    [XmlElement(ElementName = "bind_joint_axis")]
    public ColladaBindJointAxis[] Bind_Joint_Axis;
}

