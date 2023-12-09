using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "alpha", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaAlpha
{
    [XmlAttribute("operator")]
    [System.ComponentModel.DefaultValueAttribute(ColladaAlphaOperator.ADD)]
    public ColladaAlphaOperator Operator;

    [XmlAttribute("scale")]
    public float Scale;

    [XmlElement(ElementName = "argument")]
    public ColladaArgumentAlpha[] Argument;
}

