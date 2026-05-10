using System;
using System.IO;
using System.Linq;
using CgfConverter;
using CgfConverter.Diagnostics.ChunkScanning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class ModelChunkScanTests
{
    [TestMethod]
    public void Scan_SyntheticUnknownChunk_RecordsUnknownChunkType()
    {
        using var temp = TempModelFile.Create(BuildCrChFile([(0x3333, 0x0001)]));

        var report = new UnknownChunkScanner().Scan(new ChunkScanOptions
        {
            Inputs = { temp.Path }
        });

        Assert.AreEqual(1, report.Summary.FilesScanned);
        Assert.AreEqual(1, report.Summary.UnknownChunkOccurrences);
        Assert.AreEqual(ChunkIssueKind.UnknownChunkType, report.Records.Single().IssueKind);
        Assert.AreEqual(ChunkParserStage.ChunkFactory, report.Records.Single().ParserStage);
    }

    [TestMethod]
    public void Scan_SyntheticUnsupportedKnownChunkVersion_RecordsUnsupportedChunkVersion()
    {
        using var temp = TempModelFile.Create(BuildCrChFile([(0x1000, 0xFFFF)]));

        var report = new UnknownChunkScanner().Scan(new ChunkScanOptions
        {
            Inputs = { temp.Path }
        });

        Assert.AreEqual(1, report.Summary.UnsupportedVersionOccurrences);
        Assert.AreEqual(ChunkIssueKind.UnsupportedChunkVersion, report.Records.Single().IssueKind);
        Assert.AreEqual(ChunkParserStage.ChunkFactory, report.Records.Single().ParserStage);
        Assert.AreEqual(((uint)ChunkType.Mesh).ToString(), report.Records.Single().ChunkTypeValue?.ToString());
    }

    [TestMethod]
    public void Scan_InvalidHeader_RecordsParseFailureAtFileHeader()
    {
        using var temp = TempModelFile.Create([0x01, 0x02, 0x03]);

        var report = new UnknownChunkScanner().Scan(new ChunkScanOptions
        {
            Inputs = { temp.Path }
        });

        Assert.AreEqual(1, report.Summary.ParseFailureOccurrences);
        Assert.AreEqual(ChunkParserStage.FileHeader, report.Records.Single().ParserStage);
    }

    [TestMethod]
    public void Scan_MalformedChunkTable_RecordsParseFailureAtChunkTable()
    {
        using var temp = TempModelFile.Create(BuildCrChHeaderOnly(chunkTableOffset: 128, numChunks: 1));

        var report = new UnknownChunkScanner().Scan(new ChunkScanOptions
        {
            Inputs = { temp.Path }
        });

        Assert.AreEqual(1, report.Summary.ParseFailureOccurrences);
        Assert.AreEqual(ChunkParserStage.ChunkTable, report.Records.Single().ParserStage);
    }

    private static byte[] BuildCrChFile((ushort TypeLow, ushort Version)[] chunks)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var chunkTableOffset = 16u;
        var bodyOffset = chunkTableOffset + (uint)(chunks.Length * 16);

        writer.Write("CrCh"u8.ToArray());
        writer.Write((uint)FileVersion.x0746);
        writer.Write((uint)chunks.Length);
        writer.Write((int)chunkTableOffset);

        for (var i = 0; i < chunks.Length; i++)
        {
            writer.Write(chunks[i].TypeLow);
            writer.Write(chunks[i].Version);
            writer.Write(i + 1);
            writer.Write(0u);
            writer.Write(bodyOffset);
        }

        while (stream.Length < bodyOffset)
            writer.Write((byte)0);

        return stream.ToArray();
    }

    private static byte[] BuildCrChHeaderOnly(uint chunkTableOffset, uint numChunks)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write("CrCh"u8.ToArray());
        writer.Write((uint)FileVersion.x0746);
        writer.Write(numChunks);
        writer.Write((int)chunkTableOffset);

        return stream.ToArray();
    }

    private sealed class TempModelFile : IDisposable
    {
        public string Path { get; }

        private TempModelFile(byte[] contents)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.cgf");
            File.WriteAllBytes(Path, contents);
        }

        public static TempModelFile Create(byte[] contents) => new(contents);

        public void Dispose()
        {
            if (File.Exists(Path))
                File.Delete(Path);
        }
    }
}
