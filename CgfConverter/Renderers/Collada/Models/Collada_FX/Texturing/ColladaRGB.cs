using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "annotate", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaRGB
{
    [XmlAttribute("operator")]
    [System.ComponentModel.DefaultValue(ColladaRGBOperator.ADD)]
    public ColladaRGBOperator Operator;

    [XmlAttribute("scale")]
    public float Scale;

    [XmlElement(ElementName = "argument")]
    public ColladaArgument_RGB[] Argument;
}

