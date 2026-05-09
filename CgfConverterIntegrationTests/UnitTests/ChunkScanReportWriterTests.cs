using System.IO;
using System.Text.Json;
using CgfConverter.Diagnostics.ChunkScanning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class ChunkScanReportWriterTests
{
    [TestMethod]
    public void CsvWriter_WritesHeader()
    {
        using var writer = new StringWriter();

        new CsvChunkScanReportWriter().Write(CreateReport(), writer);

        StringAssert.StartsWith(writer.ToString(), "issueKind,inputPath,relativePath");
    }

    [TestMethod]
    public void CsvWriter_EscapesComma()
    {
        using var writer = new StringWriter();

        new CsvChunkScanReportWriter().Write(CreateReport(inputPath: "a,b.cgf"), writer);

        StringAssert.Contains(writer.ToString(), "\"a,b.cgf\"");
    }

    [TestMethod]
    public void CsvWriter_EscapesQuote()
    {
        using var writer = new StringWriter();

        new CsvChunkScanReportWriter().Write(CreateReport(inputPath: "a\"b.cgf"), writer);

        StringAssert.Contains(writer.ToString(), "\"a\"\"b.cgf\"");
    }

    [TestMethod]
    public void CsvWriter_EscapesNewline()
    {
        using var writer = new StringWriter();

        new CsvChunkScanReportWriter().Write(CreateReport(exceptionMessage: "line1\nline2"), writer);

        StringAssert.Contains(writer.ToString(), "\"line1\nline2\"");
    }

    [TestMethod]
    public void JsonWriter_ContainsSchemaVersionSummaryAndRecords()
    {
        using var writer = new StringWriter();

        new JsonChunkScanReportWriter().Write(CreateReport(), writer);
        using var document = JsonDocument.Parse(writer.ToString());

        Assert.IsTrue(document.RootElement.TryGetProperty("schemaVersion", out var schemaVersion));
        Assert.AreEqual("1.0", schemaVersion.GetString());
        Assert.IsTrue(document.RootElement.TryGetProperty("generatedAtUtc", out _));
        Assert.IsTrue(document.RootElement.TryGetProperty("inputs", out _));
        Assert.IsTrue(document.RootElement.TryGetProperty("summary", out _));
        Assert.IsTrue(document.RootElement.TryGetProperty("records", out _));
    }

    [TestMethod]
    public void ConsoleWriter_ContainsSummaryCountLines()
    {
        using var writer = new StringWriter();

        new ConsoleChunkScanReportWriter().Write(CreateReport(), writer);
        var output = writer.ToString();

        StringAssert.Contains(output, "Scanned 1 of 1 discovered file(s).");
        StringAssert.Contains(output, "Unknown chunk occurrences: 1");
        StringAssert.Contains(output, "Files with issues: 1");
    }

    private static ChunkScanReport CreateReport(
        string inputPath = "ship.cgf",
        string? exceptionMessage = null)
    {
        var record = new ChunkIssueRecord
        {
            IssueKind = ChunkIssueKind.UnknownChunkType,
            InputPath = inputPath,
            Extension = ".cgf",
            FileSignature = "CrCh",
            FileVersion = 0x746,
            ChunkTypeValue = 0xCCCC2333,
            ChunkTypeHex = "0xCCCC2333",
            ChunkVersionRawHex = "0x1",
            ChunkVersionHex = "0x1",
            ChunkOffset = 32,
            ChunkOffsetHex = "0x20",
            OccurrenceIndex = 0,
            ParserStage = ChunkParserStage.ChunkFactory,
            ExceptionMessage = exceptionMessage
        };

        return new ChunkScanReport
        {
            Inputs = [inputPath],
            Records = [record],
            Summary = new ChunkScanSummary
            {
                FilesDiscovered = 1,
                FilesScanned = 1,
                FilesWithIssues = 1,
                UnknownChunkOccurrences = 1
            }
        };
    }
}
