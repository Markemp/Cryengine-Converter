using System.Collections.Generic;

namespace CgfConverter.Models.Shaders;

/// <summary>
/// Represents a complete CryEngine shader definition loaded from a .ext file.
/// Contains all property definitions that control how materials using this shader are interpreted.
/// </summary>
public class ShaderDefinition
{
    /// <summary>Name of the shader (e.g., "MechCockpit", "Illum", "Glass")</summary>
    public string ShaderName { get; set; } = string.Empty;

    /// <summary>Shader version from the .ext file (e.g., "1.00", "2.00")</summary>
    public string? Version { get; set; }

    /// <summary>Whether the shader declares UsesCommonGlobalFlags (informational only)</summary>
    public bool UsesCommonGlobalFlags { get; set; }

    /// <summary>
    /// All properties defined in this shader, indexed by property name.
    /// Key is the property name (e.g., "%ALPHAGLOW"), value is the property definition.
    /// </summary>
    public Dictionary<string, ShaderProperty> Properties { get; set; } = new();

    /// <summary>Gets a property by name, returns null if not found</summary>
    public ShaderProperty? GetProperty(string name)
    {
        Properties.TryGetValue(name, out var property);
        return property;
    }

    public override string ToString() => $"{ShaderName} v{Version} ({Properties.Count} properties)";
}
