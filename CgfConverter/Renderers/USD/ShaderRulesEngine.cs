using CgfConverter.Models.Materials;
using CgfConverter.Models.Shaders;
using CgfConverter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CgfConverter.Renderers.USD;

/// <summary>
/// Interprets shader StringGenMask flags and generates material rules for USD export.
/// Converts CryEngine shader semantics into USD material node connections.
/// </summary>
public class ShaderRulesEngine
{
    private readonly TaggedLogger _log;

    public ShaderRulesEngine(TaggedLogger logger)
    {
        _log = logger;
    }

    /// <summary>
    /// Generate material rules from a material's shader definition and StringGenMask.
    /// </summary>
    /// <param name="material">The material to process</param>
    /// <param name="shaderDef">The shader definition (can be null if shader not found)</param>
    /// <returns>List of material rules to apply</returns>
    public List<MaterialRule> GenerateRules(Material material, ShaderDefinition? shaderDef)
    {
        var rules = new List<MaterialRule>();

        if (shaderDef == null)
        {
            _log.D($"No shader definition found for material {material.Name}, using default behavior");
            return rules;
        }

        if (string.IsNullOrEmpty(material.StringGenMask))
        {
            _log.D($"Material {material.Name} has no StringGenMask, using default behavior");
            return rules;
        }

        // Parse StringGenMask into individual flags (split by '%', filter empty)
        var flags = material.StringGenMask
            .Split('%', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => $"%{f.Trim()}") // Re-add % prefix
            .ToList();

        _log.D($"Processing {flags.Count} shader flags for material {material.Name}");

        foreach (var flag in flags)
        {
            var property = shaderDef.GetProperty(flag);
            if (property == null)
            {
                _log.D($"Unknown shader property: {flag} (shader: {shaderDef.ShaderName})");
                continue;
            }

            var rule = CreateRuleFromProperty(property);
            if (rule != null)
            {
                rules.Add(rule);
                _log.D($"  {flag}: {rule.Description}");
            }
        }

        return rules;
    }

    /// <summary>
    /// Create a material rule from a shader property definition.
    /// </summary>
    private MaterialRule? CreateRuleFromProperty(ShaderProperty property)
    {
        var rule = new MaterialRule
        {
            PropertyName = property.Name,
            Description = property.Description ?? property.Property ?? property.Name
        };

        // Map property names to specific rules
        switch (property.Name.ToUpperInvariant())
        {
            // Alpha channel routing
            case "%ALPHAGLOW":
                rule.Type = RuleType.ChannelRouting;
                rule.SourceTexture = "Diffuse";
                rule.SourceChannel = "alpha";
                rule.TargetInput = "emissiveColor";
                break;

            case "%GLOSS_DIFFUSEALPHA":
                rule.Type = RuleType.ChannelRouting;
                rule.SourceTexture = "Diffuse";
                rule.SourceChannel = "alpha";
                rule.TargetInput = "specular";  // Specular mask
                break;

            case "%SPECULARPOW_GLOSSALPHA":
                rule.Type = RuleType.ChannelRouting;
                rule.SourceTexture = "Specular";
                rule.SourceChannel = "alpha";
                rule.TargetInput = "roughness";  // Gloss map (invert for roughness)
                break;

            // Texture enables
            case "%ENVIRONMENT_MAP":
                rule.Type = RuleType.TextureEnable;
                rule.SourceTexture = "Environment";
                rule.TargetInput = "environment";
                break;

            case "%GLOSS_MAP":
                rule.Type = RuleType.TextureEnable;
                rule.SourceTexture = "Gloss";
                rule.TargetInput = "roughness";
                break;

            case "%DETAIL_BUMP_MAPPING":
            case "%DETAIL_TEXTURE_IS_SET":
                rule.Type = RuleType.TextureEnable;
                rule.SourceTexture = "Detail";
                rule.TargetInput = "detail";
                break;

            // Vertex colors
            case "%VERTCOLORS":
                rule.Type = RuleType.VertexColors;
                break;

            // Properties we can ignore for USD export
            case "%ANISO_SPECULAR":
            case "%OFFSETBUMPMAPPING":
            case "%PARALLAX_OCCLUSION_MAPPING":
            case "%DISPLACEMENT_MAPPING":
            case "%PHONG_TESSELLATION":
            case "%PN_TESSELLATION":
            case "%TESSELLATION":
            case "%DECAL":
            case "%BLENDLAYER":
            case "%DIRTLAYER":
            case "%ALPHAMASK_DETAILMAP":
            case "%DETAIL_TEXTURE_IS_NORMALMAP":
                rule.Type = RuleType.PropertyModifier;
                rule.Metadata = new Dictionary<string, object> { ["ignore"] = true };
                _log.D($"Ignoring unsupported property: {property.Name}");
                return null; // Don't add to rules list

            // Glass-specific
            case "%DIRT_MAP":
            case "%TINT_MAP":
            case "%TINT_COLOR_MAP":
            case "%BLUR_REFRACTION":
            case "%DEPTH_FOG":
            case "%UNLIT":
            case "%BILINEAR_FP16":
                rule.Type = RuleType.PropertyModifier;
                rule.Metadata = new Dictionary<string, object> { ["shader_specific"] = true };
                _log.D($"Skipping shader-specific property: {property.Name}");
                return null;

            // Unknown property
            default:
                _log.D($"Unhandled shader property: {property.Name}");
                rule.Type = RuleType.Unknown;
                return null;
        }

        return rule;
    }

    /// <summary>
    /// Check if a rule type should prevent default alpha-to-opacity connection.
    /// </summary>
    public bool ShouldOverrideDefaultAlphaConnection(List<MaterialRule> rules)
    {
        // If %ALPHAGLOW is present, don't connect diffuse alpha to opacity
        return rules.Any(r => r.PropertyName == "%ALPHAGLOW");
    }

    /// <summary>
    /// Get the target input for a texture channel based on active rules.
    /// </summary>
    /// <param name="rules">Active material rules</param>
    /// <param name="textureType">Texture map type (e.g., "Diffuse", "Specular")</param>
    /// <param name="channel">Channel name (e.g., "alpha", "rgb")</param>
    /// <returns>Target USD input name, or null if no rule applies</returns>
    public string? GetChannelTarget(List<MaterialRule> rules, string textureType, string channel)
    {
        var rule = rules.FirstOrDefault(r =>
            r.Type == RuleType.ChannelRouting &&
            r.SourceTexture?.Equals(textureType, StringComparison.OrdinalIgnoreCase) == true &&
            r.SourceChannel?.Equals(channel, StringComparison.OrdinalIgnoreCase) == true);

        return rule?.TargetInput;
    }

    /// <summary>
    /// Check if a texture type is enabled by shader rules.
    /// </summary>
    public bool IsTextureEnabled(List<MaterialRule> rules, string textureType)
    {
        return rules.Any(r =>
            r.Type == RuleType.TextureEnable &&
            r.SourceTexture?.Equals(textureType, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Check if vertex colors should be used based on shader rules.
    /// </summary>
    public bool UseVertexColors(List<MaterialRule> rules)
    {
        return rules.Any(r => r.Type == RuleType.VertexColors);
    }
}
