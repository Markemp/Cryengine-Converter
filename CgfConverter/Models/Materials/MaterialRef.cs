using CgfConverter.Utils;
using System;
using System.IO;
using System.Xml.Serialization;

namespace CgfConverter.Models.Materials;

/// <summary>
/// Path to a material file with a single material
/// </summary>
[XmlRoot(ElementName = "MaterialRef")]
public class MaterialRef : ISubMaterialItem
{
    /// <summary>
    /// Path to the material file
    /// </summary>
    /// <example>Name="objects/characters/humans/shared/head/lashes"</example>
    [XmlAttribute(AttributeName = "Name")]
    public string Name { get; set; } = string.Empty;

    [XmlIgnore]
    private Material? _resolvedMaterial;

    [XmlIgnore]
    public Material ResolvedMaterial {
        get {
            if (_resolvedMaterial == null)
                throw new InvalidOperationException($"MaterialRef '{Name}' has not been resolved yet.");
            return _resolvedMaterial;
        }
    }

    // Method to load the actual material from the reference path
    public void ResolveMaterial()
    {
        var filePath = string.IsNullOrWhiteSpace(Path.GetExtension(Name))
            ? Path.Combine(Path.GetDirectoryName(Name) ?? string.Empty, ".mtl")
            : Name;
            
        _resolvedMaterial = MaterialUtilities.FromFile(filePath, Path.GetFileNameWithoutExtension(Name));
    }
}
