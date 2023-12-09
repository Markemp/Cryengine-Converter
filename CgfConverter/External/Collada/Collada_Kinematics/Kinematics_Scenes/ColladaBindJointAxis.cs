using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "bind_joint_axis", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBindJointAxis
{
    [XmlAttribute("target")]
    public string Target;


    [XmlElement(ElementName = "axis")]
    public ColladaCommonSIDREFOrParamType Axis;

    [XmlElement(ElementName = "value")]
    public ColladaCommonFloatOrParamType Value;


}

