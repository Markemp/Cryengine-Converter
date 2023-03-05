using System.Xml.Serialization;

namespace CgfConverter.Terrain.Xml;

[XmlRoot(ElementName = "Properties")]
public class EntityProperties
{
    [XmlAttribute(AttributeName = "object_Model")]
    public string? ObjectModel;
}