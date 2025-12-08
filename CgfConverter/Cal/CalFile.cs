using CgfConverter.PackFileSystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace CgfConverter.Cal;

/// <summary>
/// Parser for ArcheAge .cal animation list files.
/// Format is simple key-value pairs:
/// - #filepath = path - base path for animations
/// - $Include = path.cal - includes another cal file
/// - animation_name = relative/path.caf - animation entries
/// - // or -- prefixes indicate comments
/// </summary>
public class CalFile
{
    /// <summary>Base path for animation files (from #filepath directive)</summary>
    public string? FilePath { get; private set; }

    /// <summary>Animation entries (name -> relative path)</summary>
    public Dictionary<string, string> Animations { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Included .cal file paths</summary>
    public List<string> Includes { get; } = [];

    /// <summary>
    /// Parses a .cal file from a stream.
    /// </summary>
    public static CalFile Parse(Stream stream)
    {
        var cal = new CalFile();

        using var reader = new StreamReader(stream);
        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            // Trim whitespace
            line = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line))
                continue;
            if (line.StartsWith("//"))
                continue;
            if (line.StartsWith("--"))
                continue;
            if (line.StartsWith("---"))
                continue;

            // Find the = separator
            var equalsIndex = line.IndexOf('=');
            if (equalsIndex < 0)
                continue;

            var key = line[..equalsIndex].Trim();
            var value = line[(equalsIndex + 1)..].Trim();

            // Strip inline comments from value (// style)
            var commentIndex = value.IndexOf("//");
            if (commentIndex >= 0)
            {
                value = value[..commentIndex].Trim();
            }

            // Handle special directives
            if (key.Equals("#filepath", StringComparison.OrdinalIgnoreCase))
            {
                cal.FilePath = value;
            }
            else if (key.Equals("$Include", StringComparison.OrdinalIgnoreCase))
            {
                cal.Includes.Add(value);
            }
            else if (key.StartsWith("$") || key.StartsWith("#"))
            {
                // Other special directives (ignored for now)
                continue;
            }
            else if (key.StartsWith("_"))
            {
                // Locomotion group references (e.g., _NORMAL_WALK = lmg_files\normal_walk.lmg)
                // These aren't individual animations, skip them
                continue;
            }
            else
            {
                // Regular animation entry
                cal.Animations[key] = value;
            }
        }

        return cal;
    }

    /// <summary>
    /// Parses a .cal file and recursively resolves all $Include directives.
    /// </summary>
    public static CalFile ParseWithIncludes(string calPath, IPackFileSystem packFileSystem)
    {
        var mainCal = Parse(packFileSystem.GetStream(calPath));
        var calDirectory = Path.GetDirectoryName(calPath) ?? "";

        // Process includes recursively
        foreach (var includePath in mainCal.Includes)
        {
            var includedCal = TryLoadInclude(includePath, calDirectory, packFileSystem);
            if (includedCal is null)
                continue;

            // Merge included animations (don't override existing)
            foreach (var (name, path) in includedCal.Animations)
            {
                mainCal.Animations.TryAdd(name, path);
            }

            // If main cal doesn't have a filepath, inherit from included
            if (string.IsNullOrEmpty(mainCal.FilePath) && !string.IsNullOrEmpty(includedCal.FilePath))
            {
                mainCal.FilePath = includedCal.FilePath;
            }
        }

        return mainCal;
    }

    /// <summary>
    /// Attempts to load an included .cal file, trying multiple path resolutions.
    /// </summary>
    private static CalFile? TryLoadInclude(string includePath, string calDirectory, IPackFileSystem packFileSystem)
    {
        // Build list of paths to try
        var pathsToTry = new List<string>();

        // 1. Try path as-is (relative to pack filesystem root)
        pathsToTry.Add(includePath);

        // 2. Try with "game\" prefix (ArcheAge stores files under game/ subdirectory)
        pathsToTry.Add(Path.Combine("game", includePath));

        // 3. Try relative to the including cal file's directory
        if (!string.IsNullOrEmpty(calDirectory))
        {
            pathsToTry.Add(Path.Combine(calDirectory, includePath));
        }

        foreach (var path in pathsToTry)
        {
            try
            {
                return ParseWithIncludes(path, packFileSystem);
            }
            catch (FileNotFoundException)
            {
                // Try next path
            }
        }

        // Include not found at any path
        return null;
    }
}
