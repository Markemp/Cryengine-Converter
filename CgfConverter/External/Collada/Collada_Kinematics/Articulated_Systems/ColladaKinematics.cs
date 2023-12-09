using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "kinematics", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaKinematics
{

    [XmlElement(ElementName = "instance_kinematics_model")]
    public ColladaInstanceKinematicsModel[] Instance_Kinematics_Model;


    [XmlElement(ElementName = "technique_common")]
    public ColladaTechniqueCommonKinematics Technique_Common;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

