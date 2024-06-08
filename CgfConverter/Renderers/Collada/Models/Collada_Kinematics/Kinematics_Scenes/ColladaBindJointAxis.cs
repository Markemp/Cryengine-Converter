using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Scenes;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "bind_joint_axis", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBindJointAxis
{
    [XmlAttribute("target")]
    public string Target;


    [XmlElement(ElementName = "axis")]
    public ColladaCommonSIDREFOrParamType Axis;

    [XmlElement(ElementName = "value")]
    public ColladaCommonFloatOrParamType Value;


}

