using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CgfConverter.Services;
using Extensions;

namespace CgfConverter.PackFileSystem;

public class WiiuStreamPackFileSystem : IPackFileSystem, IDisposable
{
    public const string PackFileNameSuffix = ".wiiu.stream";

    private readonly Stream _stream;
    private readonly Mutex _streamMutex = new();
    private readonly List<FileEntry> _entries = new();

    public WiiuStreamPackFileSystem(Stream stream)
    {
        _stream = stream;

        var reader = new EndiannessChangeableBinaryReader(_stream, Encoding.UTF8, true);
        reader.IsBigEndian = true;

        if (reader.ReadUInt32() != 0x7374726d) // 'strm'
            throw new InvalidDataException();

        while (reader.PeekChar() != -1)
        {
            var entry = new FileEntry(reader);
            var i = _entries.BinarySearch(entry);
            if (i >= 0)
                _entries[i] = entry;
            else
                _entries.Insert(~i, entry);
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

    public Stream GetStream(string path)
    {
        return new MemoryStream(ReadAllBytes(path));
    }

    public bool Exists(string path) => _entries.BinarySearch(new FileEntry(path)) >= 0;

    public string[] Glob(string pattern)
    {
        var regexPattern = new Regex(
            "^" +
            string.Join("[\\s\\S]*", Regex.Split(pattern, "\\*{2,}").Select(x =>
                string.Join("[^/\\\\]*", x.Split("*").Select(y =>
                    string.Join("[^/\\\\]", y.Split('?').Select(
                        Regex.Escape)))))) +
            "$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        return _entries.Where(x => regexPattern.IsMatch(x.InnerPath)).Select(x => x.InnerPath).ToArray();
    }

    public byte[] ReadAllBytes(string path)
    {
        var entry = new FileEntry(path);
        var i = _entries.BinarySearch(entry);
        if (i < 0)
            throw new FileNotFoundException();

        entry = _entries[i];
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
        public readonly int DecompressedSize;
        public readonly int Hash;
        public readonly int CompressedSize;
        public readonly int UnknownValue1;
        public readonly string InnerPath;
        public readonly long Offset;

        /// <summary>
        /// Create a dummy FileEntry for bisecting purposes.
        /// </summary>
        /// <param name="innerPath">Inner path.</param>
        public FileEntry(string innerPath)
        {
            InnerPath = FileHandlingExtensions.CombineAndNormalizePath(innerPath);
            DecompressedSize = Hash = CompressedSize = UnknownValue1 = 0;
            Offset = 0;
        }

        public FileEntry(BinaryReader reader)
        {
            CompressedSize = reader.ReadInt32();
            DecompressedSize = reader.ReadInt32();
            Hash = reader.ReadInt32();
            UnknownValue1 = reader.ReadInt32();
            InnerPath = FileHandlingExtensions.CombineAndNormalizePath(reader.ReadCString());
            Offset = reader.BaseStream.Position;
        }

        public int CompareTo(FileEntry other)
        {
            return string.Compare(InnerPath, other.InnerPath, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}