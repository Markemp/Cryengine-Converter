using System.Collections.Generic;
using System.IO;
using System.Linq;
using CgfConverter;
using CgfConverter.PackFileSystem;

namespace Extensions;

public static class FileHandlingExtensions
{
    /// <summary>Material texture file extensions used to search resolve texture paths to files on disk</summary>
    private static readonly string?[] TextureExtensions = {null, ".dds", ".png", ".tif"};

    /// <summary>Attempts to resole a material path to the correct file extension, and normalizes the path separators</summary>
    public static string ResolveTextureFile(string mtl, IPackFileSystem fs)
    {
        foreach (var ext in TextureExtensions)
        {
            string testPath;
            var m = ext == null ? mtl : Path.ChangeExtension(mtl, ext);

            if (fs.Exists(testPath = m))
                return testPath.Replace('\\', '/');

            if (fs.Exists(testPath = Path.GetFileName(m)))
                return testPath.Replace('\\', '/');

            if (fs.Exists(testPath = Path.Combine("textures", Path.GetFileName(m))))
                return testPath.Replace('\\', '/');
        }

        if (fs.Glob($"**/{Path.GetFileNameWithoutExtension(mtl)}.*")
                .FirstOrDefault(x => TextureExtensions.Contains(Path.GetExtension(x)?.ToLowerInvariant())) is { } path)
            return path;

        Utilities.Log(LogLevelEnum.Debug, $"Could not find extension for material texture \"{mtl}\". Defaulting to .dds");
        return $"{mtl}.dds".Replace("\\", "/");
    }

    /// <summary>
    /// Combines and normalizes path components that may not exist yet in the local filesystem.
    /// </summary>
    /// <param name="pathComponents">Path fragments, which may include path separators.</param>
    /// <returns>Normalized path.</returns>
    public static string CombineAndNormalizePath(params string?[] pathComponents)
    {
        var parts = pathComponents.Where(x => x != null).SelectMany(x => x!.Split('/', '\\')).ToList();
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
                if (i <= 0) continue;

                parts.RemoveAt(i - 1);
                i--;
                continue;
            }

            i++;
        }

        return string.Join('\\', parts);
    }
}