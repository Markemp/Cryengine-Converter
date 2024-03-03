using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Animation;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaChannel
{
    [XmlAttribute("source")]
    public string Source;

    [XmlAttribute("target")]
    public string Target;

}

