using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]

public partial class ColladaAxisInfoKinematics : ColladaAxisInfo
{
    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "active")]
    public ColladaCommonBoolOrParamType Active;

    [XmlElement(ElementName = "locked")]
    public ColladaCommonBoolOrParamType Locked;

    [XmlElement(ElementName = "index")]
    public ColladaKinematicsAxisInfoIndex[] Index;

    [XmlElement(ElementName = "limits")]
    public ColladaKinematicsAxisInfoLimits Limits;

    [XmlElement(ElementName = "formula")]
    public ColladaFormula[] Formula;

    [XmlElement(ElementName = "instance_formula")]
    public ColladaInstanceFormula[] Instance_Formula;
}

