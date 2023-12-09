using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "argument", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaArgumentAlpha
{
    [XmlAttribute("source")]
    public ColladaArgumentSource Source;

    [XmlAttribute("operand")]
    [System.ComponentModel.DefaultValueAttribute(ColladaArgumentAlphaOperand.SRC_ALPHA)]
    public ColladaArgumentAlphaOperand Operand;

    [XmlAttribute("sampler")]
    public string Sampler;

}

