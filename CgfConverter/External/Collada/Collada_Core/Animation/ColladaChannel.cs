using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaChannel
{
    [XmlAttribute("source")]
    public string Source;

    [XmlAttribute("target")]
    public string Target;

}

