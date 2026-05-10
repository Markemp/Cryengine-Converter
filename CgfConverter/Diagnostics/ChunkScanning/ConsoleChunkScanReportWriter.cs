using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter.Diagnostics.ChunkScanning;

public sealed class ConsoleChunkScanReportWriter : IChunkScanReportWriter
{
    public void Write(ChunkScanReport report, TextWriter writer)
    {
        writer.WriteLine("Scanned {0} of {1} discovered file(s).", report.Summary.FilesScanned, report.Summary.FilesDiscovered);
        writer.WriteLine("Files with issues: {0}", report.Summary.FilesWithIssues);
        writer.WriteLine("Unknown chunk occurrences: {0}", report.Summary.UnknownChunkOccurrences);
        writer.WriteLine("Unsupported chunk version occurrences: {0}", report.Summary.UnsupportedVersionOccurrences);
        writer.WriteLine("Parse failure occurrences: {0}", report.Summary.ParseFailureOccurrences);
        writer.WriteLine("Files with parse failures: {0}", report.Summary.ErrorFiles);

        WriteGroupedSection(writer, "Top unknown chunk patterns:", report.Records
            .Where(r => r.IssueKind == ChunkIssueKind.UnknownChunkType));
        WriteGroupedSection(writer, "Top unsupported chunk version patterns:", report.Records
            .Where(r => r.IssueKind == ChunkIssueKind.UnsupportedChunkVersion));

        var failures = report.Records
            .Where(r => r.IssueKind == ChunkIssueKind.ParseFailure)
            .OrderBy(r => r.InputPath, StringComparer.InvariantCultureIgnoreCase)
            .ThenBy(r => r.OccurrenceIndex ?? -1)
            .Take(10)
            .ToList();

        if (failures.Count == 0)
            return;

        writer.WriteLine();
        writer.WriteLine("Parse failure examples:");
        foreach (var failure in failures)
        {
            writer.WriteLine(
                "  {0} [{1}] {2}: {3}",
                failure.InputPath,
                failure.ParserStage,
                failure.ExceptionType,
                failure.ExceptionMessage);
        }
    }

    private static void WriteGroupedSection(TextWriter writer, string title, IEnumerable<ChunkIssueRecord> records)
    {
        var groups = records
            .GroupBy(r => new { r.ChunkTypeHex, r.ChunkTypeName, r.ChunkVersionHex })
            .Select(g => new
            {
                g.Key.ChunkTypeHex,
                g.Key.ChunkTypeName,
                g.Key.ChunkVersionHex,
                Count = g.Count(),
                Files = g.Select(r => r.InputPath).Distinct(StringComparer.InvariantCultureIgnoreCase).Count(),
                Example = g.Select(r => r.InputPath).OrderBy(path => path, StringComparer.InvariantCultureIgnoreCase).First()
            })
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.ChunkTypeHex, StringComparer.InvariantCultureIgnoreCase)
            .ThenBy(g => g.ChunkVersionHex, StringComparer.InvariantCultureIgnoreCase)
            .Take(10)
            .ToList();

        if (groups.Count == 0)
            return;

        writer.WriteLine();
        writer.WriteLine(title);
        foreach (var group in groups)
        {
            writer.WriteLine(
                "  {0} {1} version {2}: {3} occurrence(s) in {4} file(s), example {5}",
                group.ChunkTypeHex,
                group.ChunkTypeName is null ? string.Empty : $"({group.ChunkTypeName})",
                group.ChunkVersionHex,
                group.Count,
                group.Files,
                group.Example);
        }
    }
}
