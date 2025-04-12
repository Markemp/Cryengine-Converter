using System.Collections.Generic;
using System.Xml.Serialization;

namespace CgfConverter.Models.Materials;

[XmlRoot(ElementName = "SubMaterials")]
public class SubMaterials
{
    [XmlElement(ElementName = "Material")]
    public List<Material> Material { get; set; } = [];

    /// <summary>
    /// Link to a file with a single material.  `Name` is the path to the material file.
    /// </summary>
    public List<MaterialRef>? MaterialRef { get; set; }
}
