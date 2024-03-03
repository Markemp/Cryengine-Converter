using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Animation;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAnimation
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;


    [XmlElement(ElementName = "animation")]
    public ColladaAnimation[] Animation;

    [XmlElement(ElementName = "channel")]
    public ColladaChannel[] Channel;

    [XmlElement(ElementName = "source")]
    public ColladaSource[] Source;

    [XmlElement(ElementName = "sampler")]
    public ColladaSampler[] Sampler;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

