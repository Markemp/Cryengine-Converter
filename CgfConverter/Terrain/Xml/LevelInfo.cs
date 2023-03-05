using System.Xml.Serialization;

namespace CgfConverter.Terrain.Xml;

[XmlRoot(ElementName = "LevelInfo")]
public class LevelInfo
{
    [XmlAttribute(AttributeName = "SandboxVersion")]
    public string? SandboxVersion { get; set; }

    [XmlAttribute(AttributeName = "Name")]
    public string? Name { get; set; }

    [XmlAttribute(AttributeName = "HeightmapSize")]
    public string? HeightmapSize { get; set; }

    [XmlAttribute(AttributeName = "HeightmapUnitSize")]
    public string? HeightmapUnitSize { get; set; }

    [XmlAttribute(AttributeName = "HeightmapMaxHeight")]
    public string? HeightmapMaxHeight { get; set; }

    [XmlAttribute(AttributeName = "WaterLevel")]
    public string? WaterLevel { get; set; }

    [XmlAttribute(AttributeName = "TerrainSectorSizeInMeters")]
    public string? TerrainSectorSizeInMeters { get; set; }

    [XmlAttribute(AttributeName = "SegmentedWorld")]
    public string? SegmentedWorld { get; set; }

    [XmlElement(ElementName = "TerrainInfo")]
    public TerrainInfo? TerrainInfo { get; set; }

    [XmlArray(ElementName = "Missions")]
    [XmlArrayItem(ElementName = "Mission")]
    public Mission[]? Missions { get; set; }
}