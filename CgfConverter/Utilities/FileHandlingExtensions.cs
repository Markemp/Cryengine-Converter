using CgfConverter.PackFileSystem;
using CgfConverter.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extensions;

public static class FileHandlingExtensions
{
    /// <summary>Material texture file extensions used to search resolve texture paths to files on disk</summary>
    private static readonly string?[] TextureExtensions = { null, ".dds", ".png", ".tif" };

    /// <summary>Attempts to resolve a material path to the correct file extension, and normalizes the path separators</summary>
    public static string ResolveTextureFile(string imagePath, IPackFileSystem fs, List<string> dataDirs)
    {
        // Check cached location map if available
        static string NormalizePath(string path) => path.Replace('\\', '/');

        foreach (var ext in TextureExtensions)
        {
            var img = ext == null ? imagePath : Path.ChangeExtension(imagePath, ext);

            // 1. Check in objectDir (most common case for Cryengine)
            if (dataDirs is not null && dataDirs.Count > 0)
            {
                foreach (var dataDir in dataDirs)
                {
                    var texturePath = Path.Combine(dataDir, img);
                    if (fs.Exists(texturePath))
                        return NormalizePath(texturePath);

                    // Check for Armored warfare split dds files
                    if (fs.Exists(texturePath + ".1"))
                        return NormalizePath(texturePath) + ".1";
                }
            }

            // 2. Check direct path as provided
            if (fs.Exists(img))
                return NormalizePath(img);

            // 3. Check just the filename in current directory
            var filename = Path.GetFileName(img);
            if (fs.Exists(filename))
                return NormalizePath(filename);

            // 4. Check in textures subdirectory
            var texturesPath = Path.Combine("textures", filename);
            if (fs.Exists(texturesPath))
                return NormalizePath(texturesPath);
        }

        // Instead of expensive glob, try a few common locations with the known extensions
        var filenameWithoutExt = Path.GetFileNameWithoutExtension(imagePath);
        foreach (var ext in TextureExtensions)
        {
            if (ext == null) continue;

            var texturePath = Path.Combine("textures", filenameWithoutExt + ext);
            if (fs.Exists(texturePath))
                return NormalizePath(texturePath);
        }

        HelperMethods.Log(LogLevelEnum.Debug, $"Could not find extension for material texture \"{imagePath}\". Defaulting to .dds");
        return NormalizePath(Path.ChangeExtension(imagePath, ".dds"));
    }

    // Cache frequently normalized paths
    private static readonly Dictionary<string, string> _normalizedPathCache =
        new(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Combines and normalizes path components that may not exist yet in the local filesystem.
    /// </summary>
    /// <param name="pathComponents">Path fragments, which may include path separators.</param>
    /// <returns>Normalized path.</returns>
    public static string CombineAndNormalizePath(params string?[] pathComponents)
    {
        // Handle simple, common cases efficiently
        if (pathComponents.Length == 1 && pathComponents[0] != null)
        {
            string singlePath = pathComponents[0]!;

            // Check cache first
            if (_normalizedPathCache.TryGetValue(singlePath, out var cachedPath))
                return cachedPath;

            // Simple case: no path separator or ./ or ../ to process
            if (!singlePath.Contains('/') && !singlePath.Contains('\\') &&
                !singlePath.Contains("./") && !singlePath.Contains("../"))
            {
                var trimmed = singlePath.Trim();
                _normalizedPathCache[singlePath] = trimmed;
                return trimmed;
            }

            // For slightly more complex single path, normalize but still optimize
            if (_normalizedPathCache.Count > 1000)
                _normalizedPathCache.Clear(); // Prevent unlimited growth
        }

        // Original implementation for complex cases
        var parts = pathComponents.Where(x => x is not null).SelectMany(x => x!.Split('/', '\\')).ToList();
        for (var i = 0; i < parts.Count;)
        {
            if (parts[i] == "." || string.IsNullOrWhiteSpace(parts[i]))
            {
                parts.RemoveAt(i);
                continue;
            }

            parts[i] = parts[i].Trim();
            if (parts[i].Count(x => x == '.') == parts[i].Length)
            {
                parts.RemoveAt(i);
                if (i <= 0)
                    continue;

                parts.RemoveAt(i - 1);
                i--;
                continue;
            }

            i++;
        }

        var result = string.Join('\\', parts);

        // Cache the result if it's a single path component
        if (pathComponents.Length == 1 && pathComponents[0] != null)
            _normalizedPathCache[pathComponents[0]!] = result;

        return result;
    }
}
