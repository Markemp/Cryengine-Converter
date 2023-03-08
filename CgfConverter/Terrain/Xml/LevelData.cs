using System.Xml.Serialization;

namespace CgfConverter.Terrain.Xml;

[XmlRoot(ElementName = "LevelData")]
public class LevelData
{
    [XmlAttribute(AttributeName = "SandboxVersion")]
    public string? SandboxVersion { get; set; }
    
    [XmlElement(ElementName = "LevelInfo")]
    public LevelInfo? LevelInfo { get; set; }

    [XmlArray(ElementName = "Layers")]
    [XmlArrayItem(ElementName = "Layer")]
    public Layer[]? Layers { get; set; }
}