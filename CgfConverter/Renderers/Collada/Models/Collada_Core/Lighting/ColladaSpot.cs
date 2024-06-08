using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Lighting;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSpot
{
    [XmlElement(ElementName = "color")]
    public ColladaColor Color;

    [XmlElement(ElementName = "constant_attenuation")]
    [System.ComponentModel.DefaultValue(typeof(float), "1.0")]
    public ColladaSIDFloat Constant_Attenuation;

    [XmlElement(ElementName = "linear_attenuation")]
    [System.ComponentModel.DefaultValue(typeof(float), "0.0")]
    public ColladaSIDFloat Linear_Attenuation;

    [XmlElement(ElementName = "quadratic_attenuation")]
    [System.ComponentModel.DefaultValue(typeof(float), "0.0")]
    public ColladaSIDFloat Quadratic_Attenuation;

    [XmlElement(ElementName = "falloff_angle")]
    [System.ComponentModel.DefaultValue(typeof(float), "180.0")]
    public ColladaSIDFloat Falloff_Angle;

    [XmlElement(ElementName = "falloff_exponent")]
    [System.ComponentModel.DefaultValue(typeof(float), "0.0")]
    public ColladaSIDFloat Falloff_Exponent;


}

