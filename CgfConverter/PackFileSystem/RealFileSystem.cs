using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Extensions;

namespace CgfConverter.PackFileSystem;

public class RealFileSystem : IPackFileSystem
{
    // Object dir
    private readonly string _rootPath;

    public RealFileSystem(string rootPath)
    {
        rootPath = Path.GetFullPath(rootPath);
        if (!Directory.Exists(rootPath))
            throw new FileNotFoundException();

        _rootPath = FileHandlingExtensions.CombineAndNormalizePath(rootPath) + "\\";
    }

    public Stream GetStream(string path)
    {
        try
        {
            return new FileStream(
                FileHandlingExtensions.CombineAndNormalizePath(_rootPath, path),
                FileMode.Open,
                FileAccess.Read);
        }
        catch (IOException ioe) when (ioe.HResult == unchecked((int) 0x8007007B)) // Path name is invalid
        {
            throw new FileNotFoundException(path);
        }
    }

    public bool Exists(string path) =>
        File.Exists(FileHandlingExtensions.CombineAndNormalizePath(_rootPath, path));

    public string[] Glob(string pattern)
    {
        // Simplified glob method that avoids recursive searching
        var normalizedPattern = FileHandlingExtensions.CombineAndNormalizePath(_rootPath, pattern);
        var directory = Path.GetDirectoryName(normalizedPattern) ?? _rootPath;
        var filePattern = Path.GetFileName(normalizedPattern);
        var foundPaths = new List<string>();
        
        // If it's a direct file reference with no wildcards, just check if it exists
        if (!filePattern.Contains("*") && !filePattern.Contains("?"))
        {
            var exactPath = FileHandlingExtensions.CombineAndNormalizePath(_rootPath, pattern);
            if (File.Exists(exactPath))
                return new[] { exactPath[_rootPath.Length..] };
            return Array.Empty<string>();
        }
        
        // Special case: Double wildcard
        if (pattern.Contains("**"))
        {
            // For specific common patterns we can still do a limited search
            // Example: "**/*.dds" - search for dds files in current dir only
            if (pattern.StartsWith("**"))
            {
                var fileExtension = Path.GetExtension(pattern);
                if (!string.IsNullOrEmpty(fileExtension))
                {
                    try
                    {
                        // Only search in the root directory without recursion
                        var files = Directory.GetFiles(_rootPath, "*" + fileExtension, SearchOption.TopDirectoryOnly);
                        return files.Select(f => f[_rootPath.Length..]).ToArray();
                    }
                    catch (Exception)
                    {
                        return Array.Empty<string>();
                    }
                }
            }
            
            // Default to empty result for other complex patterns
            return Array.Empty<string>();
        }

        // For simple wildcards, only search in the specified directory, not recursively
        try
        {
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, filePattern, SearchOption.TopDirectoryOnly);
                return files.Select(f => f[_rootPath.Length..]).ToArray();
            }
        }
        catch (Exception)
        {
            // Ignore directory access errors
        }
        
        return Array.Empty<string>();
    }

    public byte[] ReadAllBytes(string path)
    {
        try
        {
            return File.ReadAllBytes(Path.Join(_rootPath, path));
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException(path);
        }
        catch (DirectoryNotFoundException)
        {
            throw new FileNotFoundException(path);
        }
        catch (IOException ioe) when (ioe.HResult == unchecked((int) 0x8007007B)) // Path name is invalid
        {
            throw new FileNotFoundException(path);
        }
    }
}
