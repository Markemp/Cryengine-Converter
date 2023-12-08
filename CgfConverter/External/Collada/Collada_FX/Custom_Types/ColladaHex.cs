using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaHex
{
    [XmlAttribute("format")]
    public string Format;

    [XmlTextAttribute()]
    public string Value;
    //TODO: this is a hex array
}

