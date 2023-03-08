using System.Xml.Serialization;

namespace CgfConverter.Terrain.Xml;

[XmlRoot(ElementName = "Layer")]
public class Layer
{
    [XmlAttribute(AttributeName = "Name")]
    public string? Name { get; set; }
    
    [XmlAttribute(AttributeName = "Parent")]
    public string? Parent { get; set; }
    
    [XmlAttribute(AttributeName = "Id")]
    public string? Id { get; set; }

    public int? IdValue
    {
        get => string.IsNullOrWhiteSpace(Id) || Id == "0" ? null : int.Parse(Id);
        set => Id = value.ToString();
    }
}