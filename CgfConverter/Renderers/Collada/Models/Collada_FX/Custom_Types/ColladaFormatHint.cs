using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Enums;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaFormatHint
{
    [XmlAttribute("channels")]
    public ColladaFormatHintChannels Channels;

    [XmlAttribute("range")]
    public ColladaFormatHintRange Range;

    [XmlAttribute("precision")]
    [System.ComponentModel.DefaultValue(ColladaFormatHintPrecision.DEFAULT)]
    public ColladaFormatHintPrecision Precision;

    [XmlAttribute("space")]
    public string Hint_Space;

}

