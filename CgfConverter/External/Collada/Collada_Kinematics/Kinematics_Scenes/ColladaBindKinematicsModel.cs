using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "bind_kinematics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBindKinematicsModel
{
    [XmlAttribute("node")]
    public string Node;

    [XmlElement(ElementName = "param")]
    public ColladaParam Param;

    [XmlElement(ElementName = "SIDREF")]
    public string SIDREF;


}

