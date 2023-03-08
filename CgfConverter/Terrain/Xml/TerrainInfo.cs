using System.Xml.Serialization;

namespace CgfConverter.Terrain.Xml;

[XmlRoot(ElementName = "TerrainInfo")]
public class TerrainInfo
{
    [XmlAttribute(AttributeName = "HeightmapSize")]
    public string? HeightmapSize { get; set; }

    [XmlAttribute(AttributeName = "UnitSize")]
    public string? UnitSize { get; set; }

    [XmlAttribute(AttributeName = "SectorSize")]
    public string? SectorSize { get; set; }

    [XmlAttribute(AttributeName = "SectorsTableSize")]
    public string? SectorsTableSize { get; set; }

    [XmlAttribute(AttributeName = "HeightmapZRatio")]
    public string? HeightmapZRatio { get; set; }

    [XmlAttribute(AttributeName = "OceanWaterLevel")]
    public string? OceanWaterLevel { get; set; }
}