using System.Collections.Generic;
using System.Xml.Serialization;

namespace CgfConverter.Terrain.Xml;

[XmlRoot(ElementName = "Mission")]
public class Mission
{
    [XmlAttribute(AttributeName = "Name")] public string? Name { get; set; }

    [XmlAttribute(AttributeName = "Description")]
    public string? Description { get; set; }

    [XmlAttribute(AttributeName = "Time")] public string? Time { get; set; }

    [XmlAttribute(AttributeName = "PlayerEquipPack")]
    public string? PlayerEquipPack { get; set; }

    [XmlAttribute(AttributeName = "MusicScript")]
    public string? MusicScript { get; set; }

    [XmlAttribute(AttributeName = "Script")]
    public string? Script { get; set; }

    [XmlAttribute(AttributeName = "CGFCount")]
    public int CgfCount { get; set; }
    
    [XmlArray(ElementName = "Missions")]
    [XmlArrayItem(ElementName = "Mission")]
    public Mission[]? Missions { get; set; }

    [XmlArray(ElementName = "Objects")]
    [XmlArrayItem("Object", Type=typeof(Object))]
    [XmlArrayItem("Entity", Type=typeof(Entity))]
    public ObjectOrEntity[]? ObjectsAndEntities;
}