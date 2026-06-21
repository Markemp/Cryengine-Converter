namespace CgfConverter.Models.Shaders;

/// <summary>
/// Represents a single property definition from a CryEngine shader .ext file.
/// Properties define how texture channels and material attributes are interpreted.
/// </summary>
public class ShaderProperty
{
    /// <summary>Property flag name (e.g., "%ALPHAGLOW", "%ENVIRONMENT_MAP")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Hex mask value used in GenMask (e.g., 0x2000, 0x80)</summary>
    public long Mask { get; set; }

    /// <summary>Short property name shown in editor (e.g., "Glow in Diffuse alpha")</summary>
    public string? Property { get; set; }

    /// <summary>
    /// Description of what the property does
    /// (e.g., "Use alpha channel of diffuse texture for glow")
    /// </summary>
    public string? Description { get; set; }

    /// <summary>Texture dependency to set when this property is enabled (e.g., "$TEX_Gloss")</summary>
    public string? DependencySet { get; set; }

    /// <summary>Texture dependency to reset when this property is enabled (e.g., "$TEX_EnvCM")</summary>
    public string? DependencyReset { get; set; }

    /// <summary>Whether this property is hidden in the editor UI</summary>
    public bool Hidden { get; set; }

    public override string ToString() => $"{Name} (Mask: 0x{Mask:X})";
}
