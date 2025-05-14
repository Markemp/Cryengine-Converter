using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter.PackFileSystem;

public class CascadedPackFileSystem : IPackFileSystem, IDisposable
{
    private readonly List<IPackFileSystem> _underlying = [];
    
    // Add a cache for file existence checks to avoid repeatedly searching all file systems
    private readonly Dictionary<string, IPackFileSystem> _existsCache = new(StringComparer.InvariantCultureIgnoreCase);

    public void Add(IPackFileSystem system)
    {
        _underlying.Insert(0, system);
        // Clear cache when adding a new system
        _existsCache.Clear();
    }

    public Stream GetStream(string path)
    {
        // Check cache first to avoid searching through all systems
        if (_existsCache.TryGetValue(path, out var cachedSystem))
        {
            try
            {
                return cachedSystem.GetStream(path);
            }
            catch (IOException)
            {
                // Cache might be stale, remove and continue with normal search
                _existsCache.Remove(path);
            }
        }

        foreach (var x in _underlying)
        {
            try
            {
                var stream = x.GetStream(path);
                // Cache the successful lookup
                _existsCache[path] = x;
                return stream;
            }
            catch (IOException)
            {
                // pass
            }
        }

        if (File.Exists(path))
            return new FileStream(path, FileMode.Open, FileAccess.Read);

        throw new FileNotFoundException();
    }

    public bool Exists(string path)
    {
        // Check cache first
        if (_existsCache.TryGetValue(path, out _))
            return true;

        foreach (var x in _underlying)
        {
            if (x.Exists(path))
            {
                // Cache the successful lookup
                _existsCache[path] = x;
                return true;
            }
        }

        if (File.Exists(path))
            return true;

        return false;
    }

    public string[] Glob(string pattern)
    {
        // Only handle exact file patterns or simple wildcards in the filename
        var directory = Path.GetDirectoryName(pattern) ?? "";
        var filePattern = Path.GetFileName(pattern);
        
        // For direct file reference with no wildcards, just check if it exists
        if (!filePattern.Contains("*") && !filePattern.Contains("?"))
        {
            return Exists(pattern) ? new[] { pattern } : Array.Empty<string>();
        }
        
        // For wildcards, collect results from all file systems but don't do recursive searching
        var results = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        
        foreach (var fs in _underlying)
        {
            var matches = fs.Glob(pattern);
            foreach (var match in matches)
            {
                results.Add(match);
            }
        }
        
        return results.ToArray();
    }

    public byte[] ReadAllBytes(string path)
    {
        foreach (var x in _underlying)
        {
            try
            {
                return x.ReadAllBytes(path);
            }
            catch (FileNotFoundException)
            {
                // pass
            }
        }

        if (File.Exists(path))
            return File.ReadAllBytes(path);

        throw new FileNotFoundException();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var x in _underlying)
            if (x is IDisposable d)
                d.Dispose();
        _underlying.Clear();
    }
}
