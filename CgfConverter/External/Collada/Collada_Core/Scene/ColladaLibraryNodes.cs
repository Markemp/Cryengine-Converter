using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaLibraryNodes
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;


    [XmlElement(ElementName = "node")]
    public ColladaNode[] Node;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

