using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "argument", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaArgument_RGB
{
    [XmlAttribute("source")]
    public ColladaArgumentSource Source;

    [XmlAttribute("operand")]
    [System.ComponentModel.DefaultValueAttribute(ColladaArgumentRGBOperand.SRC_COLOR)]
    public ColladaArgumentRGBOperand Operand;

    [XmlAttribute("sampler")]
    public string Sampler;

}

