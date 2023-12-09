using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Scenes;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "bind_kinematics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBindKinematicsModel
{
    [XmlAttribute("node")]
    public string Node;

    [XmlElement(ElementName = "param")]
    public ColladaParam Param;

    [XmlElement(ElementName = "SIDREF")]
    public string SIDREF;


}

