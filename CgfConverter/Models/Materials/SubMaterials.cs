using System.Collections.Generic;
using System.Xml.Serialization;

namespace CgfConverter.Models.Materials;

[XmlRoot(ElementName = "SubMaterials")]
public class SubMaterials
{
    [XmlElement("Material", typeof(Material))]
    [XmlElement("MaterialRef", typeof(MaterialRef))]
    public List<MaterialBase> Materials { get; set; } = [];
}
