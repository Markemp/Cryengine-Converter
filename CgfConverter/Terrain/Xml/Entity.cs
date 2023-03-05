using System.Xml.Serialization;

namespace CgfConverter.Terrain.Xml;

[XmlRoot(ElementName = "Entity")]
public class Entity : ObjectOrEntity
{
}