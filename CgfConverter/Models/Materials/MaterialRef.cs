using System.Xml.Serialization;

namespace CgfConverter.Models.Materials;

/// <summary>
/// Path to a material file with a single material
/// </summary>
[XmlRoot(ElementName = "MaterialRef")]
public class MaterialRef
{
    /// <summary>
    /// Path to the material file
    /// </summary>
    /// <example>Name="objects/characters/humans/shared/head/lashes"</example>
    [XmlAttribute(AttributeName = "Name")]
    public string Name { get; set; } = string.Empty;
}
