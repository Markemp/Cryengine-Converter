using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

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

