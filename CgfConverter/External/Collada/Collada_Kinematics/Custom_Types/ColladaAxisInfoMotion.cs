using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAxisInfoMotion : ColladaAxisInfo
{

    [XmlElement(ElementName = "bind")]
    public ColladaBind[] Bind;

    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "setparam")]
    public ColladaNewParam[] Set_Param;

    [XmlElement(ElementName = "speed")]
    public ColladaCommonFloatOrParamType Speed;

    [XmlElement(ElementName = "acceleration")]
    public ColladaCommonFloatOrParamType Acceleration;

    [XmlElement(ElementName = "deceleration")]
    public ColladaCommonFloatOrParamType Deceleration;

    [XmlElement(ElementName = "jerk")]
    public ColladaCommonFloatOrParamType Jerk;



}

