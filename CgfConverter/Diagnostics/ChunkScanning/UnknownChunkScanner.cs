using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CgfConverter.CryEngineCore;

namespace CgfConverter.Diagnostics.ChunkScanning;

public sealed class UnknownChunkScanner
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.InvariantCultureIgnoreCase)
    {
        ".cgf",
        ".cga",
        ".chr",
        ".skin",
        ".anim",
        ".soc",
        ".caf",
        ".dba"
    };

    public ChunkScanReport Scan(ChunkScanOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var inputs = EnumerateInputs(options);
        var records = new List<ChunkIssueRecord>();
        var filesScanned = 0;

        // TODO: v2 - replace with Parallel.ForEach using options.MaxThreads
        foreach (var input in inputs)
        {
            filesScanned++;
            try
            {
                using var stream = File.OpenRead(input);
                records.AddRange(Model.ScanChunkIssues(input, stream).Records);
            }
            catch (Exception ex)
            {
                records.Add(new ChunkIssueRecord
                {
                    IssueKind = ChunkIssueKind.ParseFailure,
                    InputPath = input,
                    RelativePath = null,
                    Extension = Path.GetExtension(input).ToLowerInvariant(),
                    ParserStage = ChunkParserStage.FileOpen,
                    ExceptionType = ex.GetType().Name,
                    ExceptionMessage = ex.Message
                });
            }
        }

        var orderedRecords = records
            .OrderBy(r => r.InputPath, StringComparer.InvariantCultureIgnoreCase)
            .ThenBy(r => r.OccurrenceIndex ?? -1)
            .ThenBy(r => r.IssueKind)
            .ToList();

        var filesWithIssues = orderedRecords
            .Select(r => r.InputPath)
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .Count();

        return new ChunkScanReport
        {
            Inputs = options.Inputs.ToList(),
            Records = orderedRecords,
            Summary = new ChunkScanSummary
            {
                FilesDiscovered = inputs.Count,
                FilesScanned = filesScanned,
                FilesWithIssues = filesWithIssues,
                UnknownChunkOccurrences = orderedRecords.Count(r => r.IssueKind == ChunkIssueKind.UnknownChunkType),
                UnsupportedVersionOccurrences = orderedRecords.Count(r => r.IssueKind == ChunkIssueKind.UnsupportedChunkVersion),
                ParseFailureOccurrences = orderedRecords.Count(r => r.IssueKind == ChunkIssueKind.ParseFailure),
                ErrorFiles = orderedRecords
                    .Where(r => r.IssueKind == ChunkIssueKind.ParseFailure)
                    .Select(r => r.InputPath)
                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                    .Count()
            }
        };
    }

    public IReadOnlyList<string> EnumerateInputs(ChunkScanOptions options)
    {
        var found = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var input in options.Inputs)
        {
            if (File.Exists(input))
            {
                AddIfSupported(found, Path.GetFullPath(input));
                continue;
            }

            if (Directory.Exists(input))
            {
                var searchOption = options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var file in Directory.EnumerateFiles(input, "*.*", searchOption))
                    AddIfSupported(found, Path.GetFullPath(file));
                continue;
            }

            foreach (var file in ExpandLocalGlob(input))
                AddIfSupported(found, Path.GetFullPath(file));
        }

        return found
            .OrderBy(path => path, StringComparer.InvariantCultureIgnoreCase)
            .ToList();
    }

    public static bool IsSupportedModelExtension(string path)
    {
        return SupportedExtensions.Contains(Path.GetExtension(path));
    }

    private static void AddIfSupported(HashSet<string> paths, string path)
    {
        if (IsSupportedModelExtension(path))
            paths.Add(path);
    }

    private static IEnumerable<string> ExpandLocalGlob(string pattern)
    {
        if (!pattern.Contains('*') && !pattern.Contains('?'))
            return [];

        var directory = Path.GetDirectoryName(pattern);
        if (string.IsNullOrWhiteSpace(directory))
            directory = Directory.GetCurrentDirectory();

        var filePattern = Path.GetFileName(pattern);
        if (!Directory.Exists(directory))
            return [];

        return Directory.EnumerateFiles(directory, filePattern, SearchOption.TopDirectoryOnly);
    }
}
