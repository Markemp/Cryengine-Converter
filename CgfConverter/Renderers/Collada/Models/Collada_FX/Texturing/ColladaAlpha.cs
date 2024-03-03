using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "alpha", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaAlpha
{
    [XmlAttribute("operator")]
    [System.ComponentModel.DefaultValue(ColladaAlphaOperator.ADD)]
    public ColladaAlphaOperator Operator;

    [XmlAttribute("scale")]
    public float Scale;

    [XmlElement(ElementName = "argument")]
    public ColladaArgumentAlpha[] Argument;
}

