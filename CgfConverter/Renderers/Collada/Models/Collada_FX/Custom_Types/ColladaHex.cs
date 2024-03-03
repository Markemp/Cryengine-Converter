using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaHex
{
    [XmlAttribute("format")]
    public string Format;

    [XmlText()]
    public string Value;
    //TODO: this is a hex array
}

