using System.Xml.Serialization;

namespace CgfConverter.Models.Materials;

/// <summary>
/// Path to a material file
/// </summary>
/// <example>Layer Name="Primary" Path="Materials/vehicles/manufacturer/AEGS/metals/thermal_tiles_01_base.mtl" Submtl="BUGGED" TintColor="0.33716366,0.33716366,0.33716366" WearTint="1,1,1" GlossMult="0.89999998" WearGloss="1" UVTiling="1" HeightBias="0" HeightScale="1" PaletteTint="0"/></example>
[XmlRoot(ElementName = "MatLayers")]
public class MatLayers : MaterialBase
{
    [XmlElement("Layer")]
    public Layer[]? Layers { get; set; }
}

[XmlRoot(ElementName = "Layer")]
public class Layer
{
    [XmlAttribute(AttributeName = "Name")]
    public string? Name { get; set; }
    /// <summary>
    /// Path to the material file, relative to the object directory
    /// </summary>
    [XmlAttribute(AttributeName = "Path")]
    public string? Path { get; set; }

    [XmlAttribute(AttributeName = "Submtl")]
    public string? Submtl { get; set; }

    [XmlAttribute(AttributeName = "TintColor")]
    public string? TintColor { get; set; }

    /// <summary>
    /// Wear tint.  Color in format "1,1,1"
    /// </summary>
    [XmlAttribute(AttributeName = "WearTint")]
    public string? WearTint { get; set; }

    [XmlAttribute(AttributeName = "GlossMult")]
    public string? GlossMult { get; set; }

    [XmlAttribute(AttributeName = "WearGloss")]
    public string? WearGloss { get; set; }

    [XmlAttribute(AttributeName = "UVTiling")]
    public string? UVTiling { get; set; }

    [XmlAttribute(AttributeName = "HeightBias")]
    public string? HeightBias { get; set; }

    [XmlAttribute(AttributeName = "HeightScale")]
    public string? HeightScale { get; set; }

    [XmlAttribute(AttributeName = "PaletteTint")]
    public string? PaletteTint { get; set; }
}
