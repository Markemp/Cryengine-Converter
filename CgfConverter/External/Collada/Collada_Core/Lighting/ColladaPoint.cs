using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaPoint
{

    [XmlElement(ElementName = "color")]
    public ColladaColor Color;

    [XmlElement(ElementName = "constant_attenuation")]
    [System.ComponentModel.DefaultValueAttribute(typeof(float), "1.0")]
    public ColladaSIDFloat Constant_Attenuation;

    [XmlElement(ElementName = "linear_attenuation")]
    [System.ComponentModel.DefaultValueAttribute(typeof(float), "0.0")]
    public ColladaSIDFloat Linear_Attenuation;

    [XmlElement(ElementName = "quadratic_attenuation")]
    public ColladaSIDFloat Quadratic_Attenuation;
}

