using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaKinematicsAxisInfoLimits
{
    [XmlElement(ElementName = "min")]
    public ColladaCommonFloatOrParamType Min;
    [XmlElement(ElementName = "max")]
    public ColladaCommonFloatOrParamType Max;
}

