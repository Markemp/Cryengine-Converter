using System.Xml.Serialization;

namespace CgfConverter.Materials;

[XmlRoot(ElementName = "Texture")]
public record Texture(
    [XmlAttribute(AttributeName = "Map")]
    string Map,
    [XmlAttribute(AttributeName = "File")]
    string File
);
