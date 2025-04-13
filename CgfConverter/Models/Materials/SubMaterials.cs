using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CgfConverter.Models.Materials;

[XmlRoot(ElementName = "SubMaterials")]
public class SubMaterials
{
    [XmlIgnore]
    private List<ISubMaterialItem> _orderedItems = [];

    [XmlIgnore]
    public IReadOnlyList<ISubMaterialItem> OrderedItems => _orderedItems;

    [XmlElement(ElementName = "Material")]
    public List<Material> Material { get; set; } = [];

    /// <summary>
    /// Link to a file with a single material.  `Name` is the path to the material file.
    /// </summary>
    [XmlElement(ElementName = "MaterialRef")]
    public List<MaterialRef>? MaterialRef { get; set; } = [];

    public Material GetMaterial(int index)
    {
        if (index < 0 || index >= _orderedItems.Count)
            throw new IndexOutOfRangeException($"Material index {index} is out of range.");

        return _orderedItems[index].ResolvedMaterial;
    }


}
