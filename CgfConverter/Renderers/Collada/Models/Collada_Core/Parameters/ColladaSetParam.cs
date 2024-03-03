using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSetParam
{
    [XmlAttribute("ref")]
    public string Ref;

    /// <summary>
    /// The element is the type and the element text is the value or space delimited list of values
    /// </summary>
    [XmlAnyElement]
    public XmlElement[] Data;
}

