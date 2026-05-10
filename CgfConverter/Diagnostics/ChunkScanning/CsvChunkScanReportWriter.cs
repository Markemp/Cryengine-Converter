using System.IO;
using System.Linq;

namespace CgfConverter.Diagnostics.ChunkScanning;

public sealed class CsvChunkScanReportWriter : IChunkScanReportWriter
{
    private static readonly string[] Headers =
    [
        "issueKind",
        "inputPath",
        "relativePath",
        "extension",
        "modelFileName",
        "fileSignature",
        "fileVersion",
        "chunkTypeValue",
        "chunkTypeHex",
        "chunkTypeName",
        "chunkVersionRawHex",
        "chunkVersionHex",
        "chunkId",
        "chunkIdIsGenerated",
        "chunkOffset",
        "chunkOffsetHex",
        "chunkSize",
        "chunkDataSize",
        "occurrenceIndex",
        "parserStage",
        "exceptionType",
        "exceptionMessage"
    ];

    public void Write(ChunkScanReport report, TextWriter writer)
    {
        writer.WriteLine(string.Join(",", Headers.Select(Escape)));

        foreach (var record in report.Records)
        {
            var fields = new string?[]
            {
                record.IssueKind.ToString(),
                record.InputPath,
                record.RelativePath,
                record.Extension,
                record.ModelFileName,
                record.FileSignature,
                record.FileVersion?.ToString(),
                record.ChunkTypeValue?.ToString(),
                record.ChunkTypeHex,
                record.ChunkTypeName,
                record.ChunkVersionRawHex,
                record.ChunkVersionHex,
                record.ChunkId?.ToString(),
                record.ChunkIdIsGenerated.ToString(),
                record.ChunkOffset?.ToString(),
                record.ChunkOffsetHex,
                record.ChunkSize?.ToString(),
                record.ChunkDataSize?.ToString(),
                record.OccurrenceIndex?.ToString(),
                record.ParserStage.ToString(),
                record.ExceptionType,
                record.ExceptionMessage
            };
            writer.WriteLine(string.Join(",", fields.Select(Escape)));
        }
    }

    internal static string Escape(string? value)
    {
        if (value is null)
            return string.Empty;

        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\r') && !value.Contains('\n'))
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
