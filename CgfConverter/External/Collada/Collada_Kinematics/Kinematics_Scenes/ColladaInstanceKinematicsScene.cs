using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "instance_kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

