using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaExtra
{

    [XmlAttribute("id")]
    public string ID;
    [XmlAttribute("name")]
    public string Name;
    [XmlAttribute("type")]
    public string Type;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;
}

