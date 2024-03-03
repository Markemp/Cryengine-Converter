using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAccessor
{
    [XmlAttribute("count")]
    public uint Count;

    [XmlAttribute("offset")]
    public uint Offset;

    [XmlAttribute("source")]
    public string Source;

    [XmlAttribute("stride")]
    public uint Stride;

    [XmlElement(ElementName = "param")]
    public ColladaParam[] Param;
}

