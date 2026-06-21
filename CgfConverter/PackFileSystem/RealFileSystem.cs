using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;

namespace CgfConverter.PackFileSystem;

public class RealFileSystem : IPackFileSystem
{
    private readonly string _rootPath;

    // Cache: lowercased directory path → actual entry names in that directory
    private readonly Dictionary<string, string[]> _dirFileCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string[]> _dirSubdirCache = new(StringComparer.OrdinalIgnoreCase);

    // Cache: lowercased full path → resolved actual path on disk
    private readonly Dictionary<string, string?> _resolvedPathCache = new(StringComparer.OrdinalIgnoreCase);

    public RealFileSystem(string rootPath)
    {
        rootPath = Path.GetFullPath(rootPath);
        if (!Directory.Exists(rootPath))
            throw new FileNotFoundException();

        _rootPath = FileHandlingExtensions.CombineAndNormalizePath(rootPath) + Path.DirectorySeparatorChar;
    }

    public Stream GetStream(string path)
    {
        var fullPath = ResolvePath(path);
        if (fullPath is null)
            throw new FileNotFoundException(path);

        try
        {
            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }
        catch (IOException ioe) when (ioe.HResult == unchecked((int)0x8007007B))
        {
            throw new FileNotFoundException(path);
        }
    }

    public bool Exists(string path) => ResolvePath(path) is not null;

    public string[] Glob(string pattern)
    {
        var normalizedPattern = FileHandlingExtensions.CombineAndNormalizePath(_rootPath, pattern);
        var filePattern = Path.GetFileName(normalizedPattern);

        // Direct file reference with no wildcards — just check if it exists
        if (!filePattern.Contains('*') && !filePattern.Contains('?'))
        {
            var resolved = ResolvePath(pattern);
            if (resolved is not null)
                return [resolved[_rootPath.Length..]];
            return [];
        }

        // Special case: Double wildcard
        if (pattern.Contains("**"))
        {
            if (pattern.StartsWith("**"))
            {
                var fileExtension = Path.GetExtension(pattern);
                if (!string.IsNullOrEmpty(fileExtension))
                {
                    try
                    {
                        var files = Directory.GetFiles(_rootPath, "*" + fileExtension, SearchOption.TopDirectoryOnly);
                        return files.Select(f => f[_rootPath.Length..]).ToArray();
                    }
                    catch (Exception)
                    {
                        return [];
                    }
                }
            }

            return [];
        }

        // For simple wildcards, resolve the directory case-insensitively, then search
        var directoryPattern = Path.GetDirectoryName(normalizedPattern) ?? _rootPath;
        var resolvedDir = ResolveDirectoryPath(directoryPattern);

        if (resolvedDir is null)
            return [];

        try
        {
            var files = Directory.GetFiles(resolvedDir, filePattern, SearchOption.TopDirectoryOnly);
            return files.Select(f => f[_rootPath.Length..]).ToArray();
        }
        catch (Exception)
        {
            return [];
        }
    }

    public byte[] ReadAllBytes(string path)
    {
        var fullPath = ResolvePath(path);
        if (fullPath is null)
            throw new FileNotFoundException(path);

        try
        {
            return File.ReadAllBytes(fullPath);
        }
        catch (IOException ioe) when (ioe.HResult == unchecked((int)0x8007007B))
        {
            throw new FileNotFoundException(path);
        }
    }

    /// <summary>
    /// Resolves a relative path to an actual path on disk, using case-insensitive matching
    /// when a direct lookup fails. Returns null if the file cannot be found.
    /// </summary>
    private string? ResolvePath(string relativePath)
    {
        var directPath = FileHandlingExtensions.CombineAndNormalizePath(_rootPath, relativePath);

        // Fast path: direct lookup (always works on Windows, works on Linux when casing matches)
        if (File.Exists(directPath))
            return directPath;

        // Check resolved path cache
        if (_resolvedPathCache.TryGetValue(directPath, out var cached))
            return cached;

        // Slow path: walk segments case-insensitively
        var resolved = ResolvePathCaseInsensitive(directPath);
        _resolvedPathCache[directPath] = resolved;
        return resolved;
    }

    /// <summary>
    /// Resolves a directory path case-insensitively. Returns null if the directory cannot be found.
    /// </summary>
    private string? ResolveDirectoryPath(string fullDirPath)
    {
        if (Directory.Exists(fullDirPath))
            return fullDirPath;

        // Walk segments from root
        var relativePart = fullDirPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase)
            ? fullDirPath[_rootPath.Length..]
            : fullDirPath;

        var segments = relativePart.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar,
            StringSplitOptions.RemoveEmptyEntries);

        var currentDir = _rootPath.TrimEnd(Path.DirectorySeparatorChar);

        foreach (var segment in segments)
        {
            var subdirs = GetCachedSubdirectories(currentDir);
            var match = subdirs.FirstOrDefault(d =>
                string.Equals(Path.GetFileName(d), segment, StringComparison.OrdinalIgnoreCase));

            if (match is null)
                return null;

            currentDir = match;
        }

        return currentDir;
    }

    /// <summary>
    /// Walks path segments from root, matching each case-insensitively against the actual
    /// directory contents. The final segment is matched against files.
    /// </summary>
    private string? ResolvePathCaseInsensitive(string fullPath)
    {
        // Split into root-relative segments
        if (!fullPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
            return null;

        var relativePart = fullPath[_rootPath.Length..];
        var segments = relativePart.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar,
            StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
            return null;

        var currentDir = _rootPath.TrimEnd(Path.DirectorySeparatorChar);

        // Resolve directory segments (all except last)
        for (int i = 0; i < segments.Length - 1; i++)
        {
            var subdirs = GetCachedSubdirectories(currentDir);
            var match = subdirs.FirstOrDefault(d =>
                string.Equals(Path.GetFileName(d), segments[i], StringComparison.OrdinalIgnoreCase));

            if (match is null)
                return null;

            currentDir = match;
        }

        // Resolve the final segment (file name)
        var fileName = segments[^1];
        var files = GetCachedFiles(currentDir);
        var fileMatch = files.FirstOrDefault(f =>
            string.Equals(Path.GetFileName(f), fileName, StringComparison.OrdinalIgnoreCase));

        return fileMatch;
    }

    private string[] GetCachedSubdirectories(string directory)
    {
        if (_dirSubdirCache.TryGetValue(directory, out var cached))
            return cached;

        try
        {
            var subdirs = Directory.GetDirectories(directory);
            _dirSubdirCache[directory] = subdirs;
            return subdirs;
        }
        catch (Exception)
        {
            var empty = Array.Empty<string>();
            _dirSubdirCache[directory] = empty;
            return empty;
        }
    }

    private string[] GetCachedFiles(string directory)
    {
        if (_dirFileCache.TryGetValue(directory, out var cached))
            return cached;

        try
        {
            var files = Directory.GetFiles(directory);
            _dirFileCache[directory] = files;
            return files;
        }
        catch (Exception)
        {
            var empty = Array.Empty<string>();
            _dirFileCache[directory] = empty;
            return empty;
        }
    }
}
