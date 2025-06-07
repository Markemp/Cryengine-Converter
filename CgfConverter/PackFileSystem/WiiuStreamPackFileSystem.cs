using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CgfConverter.Services;
using CgfConverter.Utils;
using Extensions;

namespace CgfConverter.PackFileSystem;

public class WiiuStreamPackFileSystem : IPackFileSystem, IDisposable
{
    private readonly TaggedLogger Log = new("WiiuStreamPackFileSystem");
    public const string PackFileNameSuffix = ".wiiu.stream";

    private readonly Stream _stream;
    private readonly Mutex _streamMutex = new();
    private readonly Dictionary<string, FileEntry> _entries = [];

    public WiiuStreamPackFileSystem(Stream stream, Dictionary<string, string> options)
    {
        _stream = stream;

        var reader = new EndiannessChangeableBinaryReader(_stream, Encoding.UTF8, true);
        reader.IsBigEndian = true;

        if (reader.ReadUInt32() != 0x7374726d) // 'strm'
            throw new InvalidDataException();

        options = options.ToDictionary(x => x.Key, x => x.Value);

        var altCostume = false;
        if (options.Remove("alt", out var altCostumeString))
            altCostume = altCostumeString.IsTrueyString(true);

        foreach (var (k, v) in options)
            Log.W("Ignoring unknown parameter \"{0}\" with value \"{1}\".", k, v);

        while (reader.PeekChar() != -1)
        {
            var entry = new FileEntry(reader);
            if ((entry.Variant >= 0 && altCostume) || (entry.Variant <= 0 && !altCostume))
                _entries[entry.InnerPath.ToLowerInvariant()] = entry;

            reader.BaseStream.Seek(
                entry.CompressedSize == 0 ? entry.DecompressedSize : entry.CompressedSize,
                SeekOrigin.Current);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _stream.Dispose();
    }

    public Stream GetStream(string path) => new MemoryStream(ReadAllBytes(path));

    public bool Exists(string path) => _entries.ContainsKey(path.ToLowerInvariant());

    // Simple cache for glob results to avoid regex recompilation
    private readonly Dictionary<string, string[]> _globCache = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase);
    
    public string[] Glob(string pattern)
    {
        // Check cache first
        if (_globCache.TryGetValue(pattern, out var cachedResult))
            return cachedResult;
            
        // For exact file references, use direct lookup which is much faster
        if (!pattern.Contains('*') && !pattern.Contains('?'))
        {
            var result = Exists(pattern) ? new[] { pattern } : Array.Empty<string>();
            _globCache[pattern] = result;
            return result;
        }
        
        // For simple filename patterns, avoid complex regex
        if (pattern.StartsWith("**/") && pattern.LastIndexOf('/') == 2)
        {
            // Pattern like "**/*.dds" - match by extension or filename
            var filePattern = pattern.Substring(3);
            if (filePattern.StartsWith("*."))
            {
                // Extension matching - faster than regex for common cases
                var extension = filePattern.Substring(1);
                var result = _entries.Values
                    .Where(x => x.InnerPath.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.InnerPath)
                    .ToArray();
                    
                _globCache[pattern] = result;
                return result;
            }
        }
        
        // Last resort: use regex for complex patterns
        try
        {
            var regexPattern = new Regex(
                "^" +
                string.Join("[\\s\\S]*", Regex.Split(pattern, "\\*{2,}").Select(x =>
                    string.Join("[^/\\\\]*", x.Split("*").Select(y =>
                        string.Join("[^/\\\\]", y.Split('?').Select(
                            Regex.Escape)))))) +
                "$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            
            var result = _entries.Values
                .Where(x => regexPattern.IsMatch(x.InnerPath))
                .Select(x => x.InnerPath)
                .ToArray();
                
            // Only cache if the pattern isn't too complex (to avoid memory issues)
            if (pattern.Length < 100)
                _globCache[pattern] = result;
                
            return result;
        }
        catch (RegexParseException)
        {
            // Fallback for invalid regex patterns
            Log.W("Invalid glob pattern: {0}", pattern);
            return Array.Empty<string>();
        }
    }

    public byte[] ReadAllBytes(string path)
    {
        if (!_entries.TryGetValue(path.ToLowerInvariant(), out FileEntry entry))
            throw new FileNotFoundException();

        var buffer = new byte[entry.DecompressedSize];

        Stream stream;
        var requireDispose = true;
        var requireMutex = false;

        switch (_stream)
        {
            case FileStream fs:
                stream = new FileStream(fs.Name, FileMode.Open, FileAccess.Read);
                break;
            case MemoryStream ms:
                stream = new MemoryStream(ms.GetBuffer(), 0, ms.Capacity, false);
                break;
            default:
                stream = _stream;
                requireMutex = true;
                requireDispose = false;
                _streamMutex.WaitOne();
                break;
        }

        try
        {
            stream.Seek(entry.Offset, SeekOrigin.Begin);
            if (entry.CompressedSize == 0)
            {
                if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                    throw new EndOfStreamException();
            }
            else
            {
                using var ms = new MemoryStream(buffer, true);
                while (ms.Position < buffer.Length && stream.Position < entry.Offset + entry.CompressedSize)
                {
                    var size = stream.ReadCryIntWithFlag(out var backFlag);

                    if (backFlag)
                    {
                        var copyLength = stream.ReadCryInt();
                        var copyBaseOffset = (int) ms.Position - copyLength;

                        for (var remaining = size + 3; remaining > 0; remaining -= copyLength)
                            ms.Write(buffer, copyBaseOffset, Math.Min(copyLength, remaining));
                    }
                    else
                    {
                        if (stream.Read(buffer, (int) ms.Position, size) != size)
                            throw new EndOfStreamException();
                        ms.Position += size;
                    }
                }

                if (ms.Position != buffer.Length)
                    throw new EndOfStreamException();
            }
        } finally{
            if (requireDispose)
                stream.Dispose();
            if (requireMutex)
                _streamMutex.ReleaseMutex();
        }

        return buffer;
    }

    private readonly struct FileEntry : IComparable<FileEntry>
    {
        public readonly int CompressedSize;
        public readonly int DecompressedSize;
        public readonly int Hash;
        public readonly ushort UnknownValue1;
        public readonly short Variant;
        public readonly string InnerPath;
        public readonly long Offset;

        /// <summary>
        /// Create a dummy FileEntry for bisecting purposes.
        /// </summary>
        /// <param name="innerPath">Inner path.</param>
        public FileEntry(string innerPath)
        {
            InnerPath = FileHandlingExtensions.CombineAndNormalizePath(innerPath);
            Offset = CompressedSize = DecompressedSize = Hash = UnknownValue1 = 0;
            Variant = 0;
        }

        public FileEntry(BinaryReader reader)
        {
            reader.ReadInto(out CompressedSize);
            reader.ReadInto(out DecompressedSize);
            reader.ReadInto(out Hash);
            reader.ReadInto(out UnknownValue1);
            reader.ReadInto(out Variant);
            InnerPath = FileHandlingExtensions.CombineAndNormalizePath(reader.ReadCString());
            Offset = reader.BaseStream.Position;
        }

        public int CompareTo(FileEntry other) =>
            string.Compare(InnerPath, other.InnerPath, StringComparison.InvariantCultureIgnoreCase);
    }
}
