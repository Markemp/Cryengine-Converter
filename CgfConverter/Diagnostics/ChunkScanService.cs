using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CgfConverter.CryEngineCore;

namespace CgfConverter.Diagnostics;

internal interface IChunkScanFileSystem
{
    string GetFullPath(string path);
    bool FileExists(string path);
    bool DirectoryExists(string path);
    FileAttributes GetAttributes(string path);
    IEnumerable<string> EnumerateFiles(string directory);
    IEnumerable<string> EnumerateDirectories(string directory);
    Stream OpenRead(string path);
}

internal sealed class SystemChunkScanFileSystem : IChunkScanFileSystem
{
    public string GetFullPath(string path) => Path.GetFullPath(path);
    public bool FileExists(string path) => File.Exists(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public FileAttributes GetAttributes(string path) => File.GetAttributes(path);
    public IEnumerable<string> EnumerateFiles(string directory) => Directory.EnumerateFiles(directory);
    public IEnumerable<string> EnumerateDirectories(string directory) => Directory.EnumerateDirectories(directory);
    public Stream OpenRead(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
}

internal sealed class ChunkScanService(IChunkScanFileSystem fileSystem)
{
    internal const int MaxConsecutiveResourceExhaustions = 3;
    private static readonly HashSet<string> supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cgf",
        ".cgfm",
        ".cga",
        ".cgam",
        ".chr",
        ".chrm",
        ".skin",
        ".skinm",
        ".anim",
        ".soc",
        ".caf",
        ".dba",
    };

    internal static bool SupportsFile(string path) => supportedExtensions.Contains(Path.GetExtension(path));

    internal ChunkScanReport Scan(IReadOnlyList<string> inputs, bool recursive)
    {
        var errors = new List<ChunkScanOperationalError>();
        var roots = ResolveInputs(inputs, errors);
        var processedRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var discoveredPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var root in roots.Where(root => root.FullPath is not null))
        {
            if (processedRoots.Add(root.FullPath!))
                DiscoverInput(root, recursive, discoveredPaths, errors);
        }

        var report = new ChunkScanReport
        {
            Inputs = roots.Select(root => root.ReportInput).ToList(),
            Recursive = recursive,
            ConsecutiveResourceExhaustionAbortThreshold = MaxConsecutiveResourceExhaustions,
        };
        report.OperationalErrors.AddRange(errors);

        var files = discoveredPaths
            .Select(path => CreateDiscoveredFile(path, roots))
            .OrderBy(file => file.Input.ReportInput.Index)
            .ThenBy(file => file.RelativePath, ChunkScanOrdering.PathComparer)
            .ToList();
        report.TotalDiscoveredFiles = files.Count;

        var consecutiveResourceExhaustions = 0;
        for (var index = 0; index < files.Count; index++)
        {
            var disposition = ScanFile(files[index], report);
            if (disposition == ScanFileDisposition.ResourceExhaustion)
            {
                consecutiveResourceExhaustions++;
                if (consecutiveResourceExhaustions >= MaxConsecutiveResourceExhaustions
                    && index < files.Count - 1)
                {
                    report.Aborted = true;
                    report.AbortReason = "consecutiveResourceExhaustionLimit";
                    break;
                }
            }
            else
            {
                consecutiveResourceExhaustions = 0;
            }
        }

        report.FinalizeReport();
        return report;
    }

    private List<ScanInputRoot> ResolveInputs(IReadOnlyList<string> inputs, ICollection<ChunkScanOperationalError> errors)
    {
        var roots = new List<ScanInputRoot>(inputs.Count);
        for (var index = 0; index < inputs.Count; index++)
        {
            var input = new ChunkScanInput { Index = index + 1, Alias = $"input-{index + 1}" };
            var root = new ScanInputRoot(input);
            roots.Add(root);
            try
            {
                root.FullPath = fileSystem.GetFullPath(inputs[index]);
            }
            catch (Exception exception) when (IsRecoverable(exception))
            {
                AddError(errors, root, ".", ChunkScanOperationalStage.Discovery, "The input path could not be resolved.", exception);
                root.IsUsable = false;
                continue;
            }

            if (fileSystem.FileExists(root.FullPath))
            {
                input.Kind = ChunkScanInputKind.File;
                if (!SupportsFile(root.FullPath))
                {
                    AddError(errors, root, Path.GetFileName(root.FullPath), ChunkScanOperationalStage.Discovery, "The explicit input file has an unsupported extension.");
                    root.IsUsable = false;
                }
            }
            else if (fileSystem.DirectoryExists(root.FullPath))
            {
                input.Kind = ChunkScanInputKind.Directory;
            }
            else
            {
                AddError(errors, root, ".", ChunkScanOperationalStage.Discovery, "The input file or directory does not exist.");
                root.IsUsable = false;
            }
        }
        return roots;
    }

    private void DiscoverInput(ScanInputRoot root, bool recursive, HashSet<string> files, List<ChunkScanOperationalError> errors)
    {
        if (!root.IsUsable || root.FullPath is null)
            return;
        if (root.ReportInput.Kind == ChunkScanInputKind.File)
        {
            files.Add(root.FullPath);
            return;
        }

        var errorCount = errors.Count;
        var found = false;
        var pending = new Stack<string>();
        pending.Push(root.FullPath);
        while (pending.Count > 0)
        {
            var directory = pending.Pop();
            try
            {
                foreach (var file in fileSystem.EnumerateFiles(directory))
                {
                    if (!SupportsFile(file))
                        continue;
                    found = true;
                    files.Add(fileSystem.GetFullPath(file));
                }
            }
            catch (Exception exception) when (IsRecoverable(exception))
            {
                AddError(errors, root, ToRelativePath(root, directory), ChunkScanOperationalStage.Discovery, "Files in the directory could not be enumerated.", exception);
            }

            if (!recursive)
                continue;
            List<string> subdirectories;
            try
            {
                subdirectories = fileSystem.EnumerateDirectories(directory).ToList();
            }
            catch (Exception exception) when (IsRecoverable(exception))
            {
                AddError(errors, root, ToRelativePath(root, directory), ChunkScanOperationalStage.Discovery, "Subdirectories could not be enumerated.", exception);
                continue;
            }
            foreach (var subdirectory in subdirectories)
            {
                try
                {
                    if ((fileSystem.GetAttributes(subdirectory) & FileAttributes.ReparsePoint) == 0)
                        pending.Push(subdirectory);
                }
                catch (Exception exception) when (IsRecoverable(exception))
                {
                    AddError(errors, root, ToRelativePath(root, subdirectory), ChunkScanOperationalStage.Discovery, "The subdirectory attributes could not be read.", exception);
                }
            }
        }
        if (!found && errors.Count == errorCount)
            AddError(errors, root, ".", ChunkScanOperationalStage.Discovery, "The input directory contains no supported CryEngine files.");
    }

    private ScanFileDisposition ScanFile(DiscoveredFile file, ChunkScanReport report)
    {
        var observer = new ChunkScanObserver();
        Model? model = null;
        Stream stream;
        try
        {
            stream = fileSystem.OpenRead(file.FullPath);
        }
        catch (OutOfMemoryException exception)
        {
            AddOpenFailure(file, report, ChunkScanOperationalStage.ResourceExhaustion, exception);
            return ScanFileDisposition.ResourceExhaustion;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            AddOpenFailure(file, report, ChunkScanOperationalStage.FileOpen, exception);
            return ScanFileDisposition.OperationalFailure;
        }

        Exception? failure = null;
        try
        {
            using (stream)
                model = Model.FromStream(file.FullPath, stream, observer);
        }
        catch (OutOfMemoryException exception)
        {
            failure = exception;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            failure = exception;
        }

        if (failure is not null && !observer.HasFatalFailure)
        {
            observer.RecordUnexpectedFailure(
                failure is IOException && failure is not EndOfStreamException
                    ? ReadFailureClassification.IoFailure
                    : failure is OutOfMemoryException
                        ? ReadFailureClassification.ResourceExhaustion
                        : ReadFailureClassification.UnexpectedFailure,
                failure);
        }

        var result = observer.CreateFileResult(
            file.Input.ReportInput.Index,
            file.RelativePath,
            model);
        report.Files.Add(result);
        if (observer.TerminalFailureClassification is ReadFailureClassification.IoFailure)
        {
            AddTerminalError(file, report, ChunkScanOperationalStage.FileIo, result);
            return ScanFileDisposition.OperationalFailure;
        }
        if (observer.TerminalFailureClassification is ReadFailureClassification.ResourceExhaustion)
        {
            AddTerminalError(file, report, ChunkScanOperationalStage.ResourceExhaustion, result);
            return ScanFileDisposition.ResourceExhaustion;
        }
        if (observer.TerminalFailureClassification is ReadFailureClassification.UnexpectedFailure)
        {
            AddTerminalError(file, report, ChunkScanOperationalStage.Unexpected, result);
            return ScanFileDisposition.OperationalFailure;
        }
        return ScanFileDisposition.Completed;
    }

    private static void AddTerminalError(DiscoveredFile file, ChunkScanReport report, ChunkScanOperationalStage stage, ChunkScanFileResult result)
    {
        report.OperationalErrors.Add(new ChunkScanOperationalError
        {
            Stage = stage,
            InputIndex = file.Input.ReportInput.Index,
            Path = file.RelativePath,
            ExceptionType = result.Findings.LastOrDefault()?.ExceptionType,
            Message = stage == ChunkScanOperationalStage.ResourceExhaustion
                ? "The file was skipped after memory exhaustion while reading it."
                : stage == ChunkScanOperationalStage.Unexpected
                    ? "The file was skipped after an unexpected read failure."
                    : "The file could not be read completely because of an I/O failure.",
        });
    }

    private static void AddOpenFailure(DiscoveredFile file, ChunkScanReport report, ChunkScanOperationalStage stage, Exception exception)
    {
        var result = new ChunkScanFileResult
        {
            InputIndex = file.Input.ReportInput.Index,
            Path = file.RelativePath,
            Extension = Path.GetExtension(file.RelativePath).ToLowerInvariant(),
            Complete = false,
        };
        var outcome = stage == ChunkScanOperationalStage.ResourceExhaustion
            ? ChunkScanOutcome.ResourceExhaustion
            : ChunkScanOutcome.IoFailure;
        result.FileOutcomes.Increment(outcome);
        result.Findings.Add(new ChunkScanFinding
        {
            Scope = ChunkScanFindingScope.File,
            Outcome = outcome,
            Stage = ModelReadStage.FileOpen,
            ExceptionType = exception.GetType().FullName,
        });
        report.Files.Add(result);
        report.OperationalErrors.Add(new ChunkScanOperationalError
        {
            Stage = stage,
            InputIndex = file.Input.ReportInput.Index,
            Path = file.RelativePath,
            ExceptionType = exception.GetType().FullName,
            Message = stage == ChunkScanOperationalStage.ResourceExhaustion
                ? "The file was skipped because memory was exhausted while opening it."
                : "The file could not be opened.",
        });
    }

    private static DiscoveredFile CreateDiscoveredFile(string path, IReadOnlyList<ScanInputRoot> roots)
    {
        var owner = roots.Where(root => root.IsUsable && root.FullPath is not null && root.ReportInput.Kind == ChunkScanInputKind.File && string.Equals(root.FullPath, path, StringComparison.OrdinalIgnoreCase)).OrderBy(root => root.ReportInput.Index).FirstOrDefault();
        if (owner is not null)
            return new(path, owner, Path.GetFileName(path));

        owner = roots.Where(root => root.IsUsable && root.FullPath is not null && root.ReportInput.Kind == ChunkScanInputKind.Directory && IsWithinDirectory(root.FullPath, path)).OrderByDescending(root => root.FullPath!.Length).ThenBy(root => root.ReportInput.Index).First();
        return new(path, owner, ToRelativePath(owner, path));
    }

    private static bool IsWithinDirectory(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        return !Path.IsPathRooted(relative) && relative != ".." && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) && !relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private static string ToRelativePath(ScanInputRoot root, string path)
    {
        var relative = root.ReportInput.Kind == ChunkScanInputKind.File ? Path.GetFileName(path) : Path.GetRelativePath(root.FullPath!, path);
        return (string.IsNullOrEmpty(relative) ? "." : relative).Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
    }

    private static void AddError(ICollection<ChunkScanOperationalError> errors, ScanInputRoot root, string path, ChunkScanOperationalStage stage, string message, Exception? exception = null) => errors.Add(new ChunkScanOperationalError
    {
        Stage = stage,
        InputIndex = root.ReportInput.Index,
        Path = path,
        ExceptionType = exception?.GetType().FullName,
        Message = message,
    });

    private static bool IsRecoverable(Exception exception) => exception is not OperationCanceledException and not OutOfMemoryException and not AccessViolationException and not StackOverflowException;

    private sealed class ScanInputRoot(ChunkScanInput reportInput)
    {
        internal ChunkScanInput ReportInput { get; } = reportInput;
        internal string? FullPath { get; set; }
        internal bool IsUsable { get; set; } = true;
    }

    private sealed record DiscoveredFile(string FullPath, ScanInputRoot Input, string RelativePath);

    private enum ScanFileDisposition
    {
        Completed,
        OperationalFailure,
        ResourceExhaustion,
    }
}

internal sealed class ChunkScanObserver : IModelReadObserver
{
    private readonly Dictionary<int, ObservedChunk> chunks = [];
    private readonly List<ObservedDataStream> dataStreams = [];
    private FileHeaderObservation? header;
    private ModelReadStage? fileFailureStage;
    private ReadFailureClassification? fileFailureClassification;
    private Exception? fileFailureException;

    internal bool HasFatalFailure => TerminalFailureClassification is not null;
    internal ReadFailureClassification? TerminalFailureClassification { get; private set; }

    public void OnFileHeader(in FileHeaderObservation observedHeader) => header = observedHeader;

    public void OnFileFailure(ModelReadStage stage, ReadFailureClassification classification, Exception exception)
    {
        fileFailureStage = stage;
        fileFailureClassification = classification;
        fileFailureException = exception;
        TerminalFailureClassification = classification;
    }

    public void OnChunkClassified(in ChunkOccurrence occurrence, ChunkFactoryClassification classification) => chunks[occurrence.TableIndex] = new ObservedChunk(
        occurrence,
        classification switch
        {
            ChunkFactoryClassification.ParserAvailable => ChunkScanOutcome.Parsed,
            ChunkFactoryClassification.IntentionallyIgnored => ChunkScanOutcome.IntentionallyIgnored,
            ChunkFactoryClassification.EnumDefinedUnimplemented => ChunkScanOutcome.EnumDefinedUnimplemented,
            ChunkFactoryClassification.RawUnknown => ChunkScanOutcome.RawUnknown,
            _ => throw new ArgumentOutOfRangeException(nameof(classification), classification, null),
        },
        ModelReadStage.ChunkFactory,
        null);

    public void OnChunkFailure(in ChunkOccurrence occurrence, ModelReadStage stage, ReadFailureClassification classification, Exception exception)
    {
        chunks[occurrence.TableIndex] = new ObservedChunk(occurrence, ToOutcome(classification), stage, exception);
        if (classification is ReadFailureClassification.IoFailure or ReadFailureClassification.ResourceExhaustion)
            TerminalFailureClassification = classification;
    }

    public void OnUnknownDataStream(in ChunkOccurrence occurrence, uint typeCode, uint offset) =>
        dataStreams.Add(new ObservedDataStream(occurrence, typeCode, offset));

    internal void RecordUnexpectedFailure(ReadFailureClassification classification, Exception exception)
    {
        fileFailureStage = ModelReadStage.FileRead;
        fileFailureClassification = classification;
        fileFailureException = exception;
        TerminalFailureClassification = classification;
    }

    internal ChunkScanFileResult CreateFileResult(
        int inputIndex,
        string relativePath,
        Model? model)
    {
        var result = new ChunkScanFileResult
        {
            InputIndex = inputIndex,
            Path = relativePath,
            Extension = Path.GetExtension(relativePath).ToLowerInvariant(),
            Complete = TerminalFailureClassification is null,
            FileSignature = header?.Signature ?? model?.FileSignature,
            FileType = header?.FileTypeRaw is uint fileType ? FormatHex(fileType) : null,
            FileVersion = header?.FileVersionRaw is uint fileVersion
                ? FormatHex(fileVersion)
                : model is null ? null : FormatHex((uint)model.FileVersion),
            DeclaredChunkCount = header?.DeclaredChunkCount ?? model?.NumChunks,
            ChunkTableOffset = header?.ChunkTableOffset ?? model?.ChunkTableOffset,
        };

        foreach (var observed in chunks.Values.OrderBy(value => value.Occurrence.TableIndex))
        {
            result.ObservedChunkCount++;
            result.ChunkOutcomes.Increment(observed.Outcome);
            if (observed.Outcome is ChunkScanOutcome.Parsed or ChunkScanOutcome.IntentionallyIgnored)
                continue;
            var occurrence = observed.Occurrence;
            result.Findings.Add(new ChunkScanFinding
            {
                TableIndex = occurrence.TableIndex,
                Scope = ChunkScanFindingScope.Chunk,
                ChunkId = occurrence.StoredId,
                Outcome = observed.Outcome,
                TypeCode = FormatHex(occurrence.TypeCode),
                TypeName = Enum.GetName(typeof(ChunkType), (ChunkType)occurrence.TypeCode),
                VersionRaw = FormatHex(occurrence.VersionRaw),
                Version = FormatHex(occurrence.Version),
                Offset = FormatHex(occurrence.Offset),
                Size = occurrence.Size,
                Stage = observed.Stage,
                ExceptionType = observed.Exception?.GetType().FullName,
            });
        }

        foreach (var observed in dataStreams
                     .OrderBy(value => value.Occurrence.TableIndex)
                     .ThenBy(value => value.Offset)
                     .ThenBy(value => value.TypeCode))
        {
            var occurrence = observed.Occurrence;
            result.Findings.Add(new ChunkScanFinding
            {
                TableIndex = occurrence.TableIndex,
                Scope = ChunkScanFindingScope.DataStream,
                ChunkId = occurrence.StoredId,
                Outcome = ChunkScanOutcome.RawUnknown,
                TypeCode = FormatHex(observed.TypeCode),
                TypeName = Enum.GetName(typeof(DatastreamType), (DatastreamType)observed.TypeCode),
                VersionRaw = FormatHex(occurrence.VersionRaw),
                Version = FormatHex(occurrence.Version),
                Offset = FormatHex(observed.Offset),
                Stage = ModelReadStage.DataStreamRead,
            });
        }

        if (fileFailureClassification is not null && fileFailureStage is not null && fileFailureException is not null)
        {
            var outcome = ToOutcome(fileFailureClassification.Value);
            result.FileOutcomes.Increment(outcome);
            result.Findings.Add(new ChunkScanFinding
            {
                Scope = ChunkScanFindingScope.File,
                Outcome = outcome,
                Stage = fileFailureStage.Value,
                ExceptionType = fileFailureException.GetType().FullName,
            });
        }
        return result;
    }

    private static ChunkScanOutcome ToOutcome(ReadFailureClassification classification) => classification switch
    {
        ReadFailureClassification.UnsupportedFileSignature => ChunkScanOutcome.UnsupportedFileSignature,
        ReadFailureClassification.UnsupportedVersion => ChunkScanOutcome.UnsupportedVersion,
        ReadFailureClassification.ParseFailure => ChunkScanOutcome.ParseFailure,
        ReadFailureClassification.IoFailure => ChunkScanOutcome.IoFailure,
        ReadFailureClassification.ResourceExhaustion => ChunkScanOutcome.ResourceExhaustion,
        ReadFailureClassification.UnexpectedFailure => ChunkScanOutcome.UnexpectedFailure,
        _ => throw new ArgumentOutOfRangeException(nameof(classification), classification, null),
    };

    private static string FormatHex(uint value) => $"0x{value:X8}";

    private sealed record ObservedChunk(ChunkOccurrence Occurrence, ChunkScanOutcome Outcome, ModelReadStage Stage, Exception? Exception);
    private sealed record ObservedDataStream(ChunkOccurrence Occurrence, uint TypeCode, uint Offset);
}
