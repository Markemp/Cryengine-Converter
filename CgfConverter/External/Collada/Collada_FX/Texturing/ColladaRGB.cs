using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "annotate", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaRGB
{
    [XmlAttribute("operator")]
    [System.ComponentModel.DefaultValueAttribute(ColladaRGBOperator.ADD)]
    public ColladaRGBOperator Operator;

    [XmlAttribute("scale")]
    public float Scale;

    [XmlElement(ElementName = "argument")]
    public ColladaArgument_RGB[] Argument;
}

