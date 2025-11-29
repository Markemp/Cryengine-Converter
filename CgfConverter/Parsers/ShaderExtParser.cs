using CgfConverter.Models.Shaders;
using CgfConverter.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace CgfConverter.Parsers;

/// <summary>
/// Parser for CryEngine shader .ext files.
/// Extracts property definitions that control material interpretation.
/// </summary>
public class ShaderExtParser
{
    private readonly TaggedLogger _log;

    public ShaderExtParser(TaggedLogger logger)
    {
        _log = logger;
    }

    /// <summary>
    /// Parse a shader .ext file and return the shader definition.
    /// </summary>
    /// <param name="filePath">Path to the .ext file</param>
    /// <returns>Parsed shader definition, or null if parsing fails</returns>
    public ShaderDefinition? ParseShaderFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _log.D($"Shader file not found: {filePath}");
            return null;
        }

        try
        {
            var shaderName = Path.GetFileNameWithoutExtension(filePath);
            var content = File.ReadAllText(filePath);

            return ParseShaderContent(shaderName, content);
        }
        catch (Exception ex)
        {
            _log.D($"Error parsing shader file {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Parse shader content string into a shader definition.
    /// </summary>
    public ShaderDefinition ParseShaderContent(string shaderName, string content)
    {
        var shader = new ShaderDefinition { ShaderName = shaderName };

        // Extract version
        var versionMatch = Regex.Match(content, @"Version\s*\(([^)]+)\)", RegexOptions.IgnoreCase);
        if (versionMatch.Success)
            shader.Version = versionMatch.Groups[1].Value.Trim();

        // Check for UsesCommonGlobalFlags
        shader.UsesCommonGlobalFlags = content.Contains("UsesCommonGlobalFlags", StringComparison.OrdinalIgnoreCase);

        // Extract all Property blocks
        var properties = ParsePropertyBlocks(content);
        foreach (var property in properties)
        {
            if (!string.IsNullOrEmpty(property.Name))
                shader.Properties[property.Name] = property;
        }

        _log.D($"Parsed shader {shaderName}: {shader.Properties.Count} properties");
        return shader;
    }

    /// <summary>
    /// Extract all Property {} blocks from shader content.
    /// </summary>
    private List<ShaderProperty> ParsePropertyBlocks(string content)
    {
        var properties = new List<ShaderProperty>();

        // Find all Property blocks using regex
        // Match: Property\n{\n  ... content ...\n}
        var propertyBlockPattern = @"Property\s*\{([^}]+)\}";
        var matches = Regex.Matches(content, propertyBlockPattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            var blockContent = match.Groups[1].Value;
            var property = ParsePropertyBlock(blockContent);
            if (property != null)
                properties.Add(property);
        }

        return properties;
    }

    /// <summary>
    /// Parse a single Property block content.
    /// </summary>
    private ShaderProperty? ParsePropertyBlock(string blockContent)
    {
        var property = new ShaderProperty();

        // Parse each line in the block
        var lines = blockContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Skip comments
            if (trimmed.StartsWith("//"))
                continue;

            // Parse Name = value
            if (TryParseKeyValue(trimmed, "Name", out var name))
            {
                property.Name = name.Trim();
            }
            // Parse Mask = 0xHEX
            else if (TryParseKeyValue(trimmed, "Mask", out var mask))
            {
                property.Mask = ParseHexValue(mask);
            }
            // Parse Property (display name)
            else if (TryParseKeyValue(trimmed, "Property", out var propName))
            {
                property.Property = propName.Trim();
            }
            // Parse Description
            else if (TryParseKeyValue(trimmed, "Description", out var description))
            {
                property.Description = description.Trim();
            }
            // Parse DependencySet
            else if (TryParseKeyValue(trimmed, "DependencySet", out var depSet))
            {
                property.DependencySet = depSet.Trim();
            }
            // Parse DependencyReset
            else if (TryParseKeyValue(trimmed, "DependencyReset", out var depReset))
            {
                property.DependencyReset = depReset.Trim();
            }
            // Parse Hidden
            else if (trimmed.Equals("Hidden", StringComparison.OrdinalIgnoreCase))
            {
                property.Hidden = true;
            }
        }

        // Only return valid properties with a name
        return string.IsNullOrEmpty(property.Name) ? null : property;
    }

    /// <summary>
    /// Try to parse a "Key = Value" or "Key (Value)" line.
    /// </summary>
    private bool TryParseKeyValue(string line, string key, out string value)
    {
        value = string.Empty;

        // Try "Key = Value" format
        var equalsPattern = $@"^\s*{key}\s*=\s*(.+)$";
        var match = Regex.Match(line, equalsPattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            value = match.Groups[1].Value.Trim();
            return true;
        }

        // Try "Key (Value)" format
        var parensPattern = $@"^\s*{key}\s*\(([^)]*)\)";
        match = Regex.Match(line, parensPattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            value = match.Groups[1].Value.Trim();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parse hex value string (e.g., "0x2000", "0x80") to long.
    /// </summary>
    private long ParseHexValue(string hexString)
    {
        hexString = hexString.Trim();

        // Remove "0x" prefix if present
        if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hexString = hexString.Substring(2);

        // Parse as hex
        if (long.TryParse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
            return value;

        _log.D($"Failed to parse hex value: {hexString}");
        return 0;
    }

    /// <summary>
    /// Load all shader definitions from a directory.
    /// </summary>
    /// <param name="shadersDirectory">Directory containing .ext files</param>
    /// <returns>Dictionary of shader definitions keyed by shader name (case-insensitive)</returns>
    public Dictionary<string, ShaderDefinition> LoadShadersFromDirectory(string shadersDirectory)
    {
        var shaders = new Dictionary<string, ShaderDefinition>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(shadersDirectory))
        {
            _log.D($"Shaders directory not found: {shadersDirectory}");
            return shaders;
        }

        var extFiles = Directory.GetFiles(shadersDirectory, "*.ext", SearchOption.TopDirectoryOnly);
        _log.D($"Loading {extFiles.Length} shader files from {shadersDirectory}");

        foreach (var extFile in extFiles)
        {
            var shader = ParseShaderFile(extFile);
            if (shader != null)
            {
                shaders[shader.ShaderName] = shader;
            }
        }

        _log.D($"Loaded {shaders.Count} shader definitions");
        return shaders;
    }
}
