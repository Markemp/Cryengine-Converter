using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaKinematicsAxisInfoLimits
{
    [XmlElement(ElementName = "min")]
    public ColladaCommonFloatOrParamType Min;
    [XmlElement(ElementName = "max")]
    public ColladaCommonFloatOrParamType Max;
}

