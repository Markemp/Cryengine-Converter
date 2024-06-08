using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "argument", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaArgument_RGB
{
    [XmlAttribute("source")]
    public ColladaArgumentSource Source;

    [XmlAttribute("operand")]
    [System.ComponentModel.DefaultValue(ColladaArgumentRGBOperand.SRC_COLOR)]
    public ColladaArgumentRGBOperand Operand;

    [XmlAttribute("sampler")]
    public string Sampler;

}

