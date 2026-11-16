using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CgfConverter.CryEngineCore;

namespace CgfConverter.Diagnostics;

internal enum ChunkScanInputKind
{
    Unknown,
    File,
    Directory,
}

internal enum ChunkScanOutcome
{
    Parsed,
    IntentionallyIgnored,
    EnumDefinedUnimplemented,
    RawUnknown,
    UnsupportedFileSignature,
    UnsupportedVersion,
    ParseFailure,
    IoFailure,
    ResourceExhaustion,
    UnexpectedFailure,
}

internal enum ChunkScanFindingScope
{
    File,
    Chunk,
    DataStream,
}

internal enum ChunkScanOperationalStage
{
    Discovery,
    FileOpen,
    FileIo,
    ResourceExhaustion,
    Unexpected,
}

internal sealed class ChunkScanInput
{
    public int Index { get; init; }
    public required string Alias { get; init; }
    public ChunkScanInputKind Kind { get; internal set; }
}

internal sealed class ChunkScanReport
{
    public int SchemaVersion { get; init; } = 4;
    public string ConverterVersion { get; init; } = GetConverterVersion();
    public required List<ChunkScanInput> Inputs { get; init; }
    public bool Recursive { get; init; }
    public int ConsecutiveResourceExhaustionAbortThreshold { get; init; }
    public bool Aborted { get; internal set; }
    public string? AbortReason { get; internal set; }
    public ChunkScanSummary Summary { get; private set; } = new();
    public List<ChunkScanFileResult> Files { get; } = [];
    public List<ChunkFindingGroup> FindingGroups { get; private set; } = [];
    public List<ChunkScanOperationalError> OperationalErrors { get; } = [];
    internal int TotalDiscoveredFiles { get; set; }

    internal void FinalizeReport()
    {
        Files.Sort(ChunkScanOrdering.CompareFiles);
        for (var index = 0; index < Files.Count; index++)
            Files[index].FileIndex = index + 1;
        OperationalErrors.Sort(ChunkScanOrdering.CompareOperationalErrors);

        var chunkTotals = new ChunkOutcomeCounts();
        var fileTotals = new FileOutcomeCounts();
        foreach (var file in Files)
        {
            chunkTotals.Add(file.ChunkOutcomes);
            fileTotals.Add(file.FileOutcomes);
        }

        Summary = new ChunkScanSummary
        {
            FilesDiscovered = TotalDiscoveredFiles,
            FilesProcessed = Files.Count,
            FilesComplete = Files.Count(file => file.Complete),
            FilesIncomplete = Files.Count(file => !file.Complete),
            OperationalErrors = OperationalErrors.Count,
            Aborted = Aborted,
            AbortReason = AbortReason,
            ConsecutiveResourceExhaustionAbortThreshold = ConsecutiveResourceExhaustionAbortThreshold,
            ChunkOutcomes = chunkTotals,
            FileOutcomes = fileTotals,
            NestedDataStreamFindings = Files.Sum(file =>
                file.Findings.Count(finding => finding.Scope == ChunkScanFindingScope.DataStream)),
        };

        FindingGroups = Files
            .SelectMany(file => file.Findings.Select(finding => (file, finding)))
            .GroupBy(item => new
            {
                item.finding.Scope,
                item.finding.Outcome,
                item.finding.TypeCode,
                item.finding.TypeName,
                item.finding.VersionRaw,
                item.finding.Stage,
            })
            .Select(group => new ChunkFindingGroup
            {
                Scope = group.Key.Scope,
                Outcome = group.Key.Outcome,
                TypeCode = group.Key.TypeCode,
                TypeName = group.Key.TypeName,
                VersionRaw = group.Key.VersionRaw,
                Stage = group.Key.Stage,
                OccurrenceCount = group.Count(),
                AffectedExtensions = group
                    .Select(item => item.file.Extension)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, ChunkScanOrdering.PathComparer)
                    .ToList(),
                AffectedFileIndices = group
                    .Select(item => item.file.FileIndex)
                    .Distinct()
                    .OrderBy(value => value)
                    .ToList(),
                AffectedFileCount = group.Select(item => item.file.FileIndex).Distinct().Count(),
            })
            .OrderBy(group => group.Scope)
            .ThenBy(group => group.Outcome)
            .ThenBy(group => group.TypeCode, StringComparer.Ordinal)
            .ThenBy(group => group.VersionRaw, StringComparer.Ordinal)
            .ThenBy(group => group.Stage)
            .ToList();
    }

    private static string GetConverterVersion()
    {
        var assembly = typeof(ChunkScanReport).Assembly;
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "unknown";
    }
}

internal sealed class ChunkScanSummary
{
    public int FilesDiscovered { get; init; }
    public int FilesProcessed { get; init; }
    public int FilesComplete { get; init; }
    public int FilesIncomplete { get; init; }
    public int OperationalErrors { get; init; }
    public bool Aborted { get; init; }
    public string? AbortReason { get; init; }
    public int ConsecutiveResourceExhaustionAbortThreshold { get; init; }
    public ChunkOutcomeCounts ChunkOutcomes { get; init; } = new();
    public FileOutcomeCounts FileOutcomes { get; init; } = new();
    public int NestedDataStreamFindings { get; init; }
    internal int FindingCount => ChunkOutcomes.FindingCount + FileOutcomes.FindingCount + NestedDataStreamFindings;
}

internal sealed class ChunkOutcomeCounts
{
    public int Parsed { get; private set; }
    public int IntentionallyIgnored { get; private set; }
    public int EnumDefinedUnimplemented { get; private set; }
    public int RawUnknown { get; private set; }
    public int UnsupportedVersion { get; private set; }
    public int ParseFailure { get; private set; }
    public int IoFailure { get; private set; }
    public int ResourceExhaustion { get; private set; }
    public int Total => Parsed + IntentionallyIgnored + EnumDefinedUnimplemented + RawUnknown
        + UnsupportedVersion + ParseFailure + IoFailure + ResourceExhaustion;

    internal int FindingCount =>
        EnumDefinedUnimplemented + RawUnknown + UnsupportedVersion + ParseFailure
        + IoFailure + ResourceExhaustion;

    internal void Increment(ChunkScanOutcome outcome)
    {
        switch (outcome)
        {
            case ChunkScanOutcome.Parsed:
                Parsed++;
                break;
            case ChunkScanOutcome.IntentionallyIgnored:
                IntentionallyIgnored++;
                break;
            case ChunkScanOutcome.EnumDefinedUnimplemented:
                EnumDefinedUnimplemented++;
                break;
            case ChunkScanOutcome.RawUnknown:
                RawUnknown++;
                break;
            case ChunkScanOutcome.UnsupportedVersion:
                UnsupportedVersion++;
                break;
            case ChunkScanOutcome.ParseFailure:
                ParseFailure++;
                break;
            case ChunkScanOutcome.IoFailure:
                IoFailure++;
                break;
            case ChunkScanOutcome.ResourceExhaustion:
                ResourceExhaustion++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
        }
    }

    internal void Add(ChunkOutcomeCounts other)
    {
        Parsed += other.Parsed;
        IntentionallyIgnored += other.IntentionallyIgnored;
        EnumDefinedUnimplemented += other.EnumDefinedUnimplemented;
        RawUnknown += other.RawUnknown;
        UnsupportedVersion += other.UnsupportedVersion;
        ParseFailure += other.ParseFailure;
        IoFailure += other.IoFailure;
        ResourceExhaustion += other.ResourceExhaustion;
    }
}

internal sealed class FileOutcomeCounts
{
    public int UnsupportedFileSignature { get; private set; }
    public int ParseFailure { get; private set; }
    public int IoFailure { get; private set; }
    public int ResourceExhaustion { get; private set; }
    public int UnexpectedFailure { get; private set; }
    public int Total => UnsupportedFileSignature + ParseFailure + IoFailure
        + ResourceExhaustion + UnexpectedFailure;
    internal int FindingCount => Total;

    internal void Increment(ChunkScanOutcome outcome)
    {
        switch (outcome)
        {
            case ChunkScanOutcome.UnsupportedFileSignature:
                UnsupportedFileSignature++;
                break;
            case ChunkScanOutcome.ParseFailure:
                ParseFailure++;
                break;
            case ChunkScanOutcome.IoFailure:
                IoFailure++;
                break;
            case ChunkScanOutcome.ResourceExhaustion:
                ResourceExhaustion++;
                break;
            case ChunkScanOutcome.UnexpectedFailure:
                UnexpectedFailure++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
        }
    }

    internal void Add(FileOutcomeCounts other)
    {
        UnsupportedFileSignature += other.UnsupportedFileSignature;
        ParseFailure += other.ParseFailure;
        IoFailure += other.IoFailure;
        ResourceExhaustion += other.ResourceExhaustion;
        UnexpectedFailure += other.UnexpectedFailure;
    }
}

internal sealed class ChunkScanFileResult
{
    public int FileIndex { get; internal set; }
    public int InputIndex { get; init; }
    public required string Path { get; init; }
    public required string Extension { get; init; }
    public bool Complete { get; internal set; } = true;
    public string? FileSignature { get; internal set; }
    public string? FileType { get; internal set; }
    public string? FileVersion { get; internal set; }
    public uint? DeclaredChunkCount { get; internal set; }
    public int? ChunkTableOffset { get; internal set; }
    public int ObservedChunkCount { get; internal set; }
    public ChunkOutcomeCounts ChunkOutcomes { get; } = new();
    public FileOutcomeCounts FileOutcomes { get; } = new();
    public List<ChunkScanFinding> Findings { get; } = [];

    internal string Identity => $"input-{InputIndex}/{Path}";
}

internal sealed class ChunkScanFinding
{
    public ChunkScanFindingScope Scope { get; init; }
    public int? TableIndex { get; init; }
    public int? ChunkId { get; init; }
    public ChunkScanOutcome Outcome { get; init; }
    public string? TypeCode { get; init; }
    public string? TypeName { get; init; }
    public string? VersionRaw { get; init; }
    public string? Version { get; init; }
    public string? Offset { get; init; }
    public uint? Size { get; init; }
    public ModelReadStage Stage { get; init; }
    public string? ExceptionType { get; init; }
}

internal sealed class ChunkFindingGroup
{
    public ChunkScanFindingScope Scope { get; init; }
    public ChunkScanOutcome Outcome { get; init; }
    public string? TypeCode { get; init; }
    public string? TypeName { get; init; }
    public string? VersionRaw { get; init; }
    public ModelReadStage Stage { get; init; }
    public int OccurrenceCount { get; init; }
    public required List<string> AffectedExtensions { get; init; }
    public required List<int> AffectedFileIndices { get; init; }
    public int AffectedFileCount { get; init; }
}

internal sealed class ChunkScanOperationalError
{
    public ChunkScanOperationalStage Stage { get; init; }
    public int InputIndex { get; init; }
    public required string Path { get; init; }
    public string? ExceptionType { get; init; }
    public required string Message { get; init; }

    internal string Identity => $"input-{InputIndex}/{Path}";
}

internal interface IChunkScanReportDestination : IDisposable
{
    string OutputPath { get; }
    void Commit(ChunkScanReport report);
}

internal static class ChunkScanJsonWriter
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    internal static IChunkScanReportDestination Reserve(string outputPath) =>
        new ReservedJsonDestination(outputPath);

    internal static void Write(string outputPath, ChunkScanReport report)
    {
        using var destination = Reserve(outputPath);
        destination.Commit(report);
    }

    internal static byte[] Serialize(ChunkScanReport report)
    {
        var json = JsonSerializer.Serialize(report, JsonOptions)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(json + "\n");
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private sealed class ReservedJsonDestination : IChunkScanReportDestination
    {
        private readonly string temporaryPath;
        private bool committed;
        private bool disposed;

        internal ReservedJsonDestination(string outputPath)
        {
            OutputPath = Path.GetFullPath(outputPath);
            var directory = Path.GetDirectoryName(OutputPath);
            if (string.IsNullOrEmpty(directory))
                throw new InvalidOperationException("The report path does not have a parent directory.");

            Directory.CreateDirectory(directory);

            if (File.Exists(OutputPath))
            {
                using var existing = new FileStream(
                    OutputPath,
                    FileMode.Open,
                    FileAccess.Write,
                    FileShare.Read);
            }

            temporaryPath = Path.Combine(
                directory,
                $".{Path.GetFileName(OutputPath)}.{Guid.NewGuid():N}.tmp");

            using var reservation = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
        }

        public string OutputPath { get; }

        public void Commit(ChunkScanReport report)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (committed)
                throw new InvalidOperationException("The report destination has already been committed.");

            File.WriteAllBytes(temporaryPath, Serialize(report));
            File.Move(temporaryPath, OutputPath, overwrite: true);
            committed = true;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            if (committed)
                return;

            try
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
            catch
            {
                // Cleanup must never mask the scan or report-write failure.
            }
        }
    }
}

internal static class ChunkScanOrdering
{
    internal static IComparer<string> PathComparer { get; } =
        Comparer<string>.Create(ComparePaths);

    internal static int ComparePaths(string? left, string? right)
    {
        var caseInsensitive = StringComparer.OrdinalIgnoreCase.Compare(left, right);
        return caseInsensitive != 0
            ? caseInsensitive
            : StringComparer.Ordinal.Compare(left, right);
    }

    internal static int CompareFiles(ChunkScanFileResult left, ChunkScanFileResult right)
    {
        var input = left.InputIndex.CompareTo(right.InputIndex);
        return input != 0 ? input : ComparePaths(left.Path, right.Path);
    }

    internal static int CompareOperationalErrors(
        ChunkScanOperationalError left,
        ChunkScanOperationalError right)
    {
        var input = left.InputIndex.CompareTo(right.InputIndex);
        if (input != 0)
            return input;

        var path = ComparePaths(left.Path, right.Path);
        if (path != 0)
            return path;

        var stage = left.Stage.CompareTo(right.Stage);
        return stage != 0
            ? stage
            : StringComparer.Ordinal.Compare(left.Message, right.Message);
    }
}
