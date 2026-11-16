using System;
using System.Collections.Generic;
using System.IO;

namespace CgfConverter.Diagnostics;

public static class ChunkScanCommand
{
    public const string Name = "scan-unknown-chunks";

    public static bool TryRun(string[] args, out int exitCode) =>
        TryDispatch(args, RunWithConsole, out exitCode);

    internal static bool TryDispatch(
        string[] args,
        Func<string[], int> runner,
        out int exitCode)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(runner);

        if (args.Length == 0 || !string.Equals(args[0], Name, StringComparison.OrdinalIgnoreCase))
        {
            exitCode = 0;
            return false;
        }

        exitCode = runner(args[1..]);
        return true;
    }

    internal static int Run(
        string[] args,
        TextWriter output,
        TextWriter error,
        IChunkScanFileSystem fileSystem,
        Func<string, IChunkScanReportDestination> destinationFactory)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(destinationFactory);

        if (args.Length == 1 && IsHelp(args[0]))
        {
            PrintUsage(output);
            return 0;
        }

        var inputs = new List<string>();
        string? reportPath = null;
        var recursive = false;

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument.ToLowerInvariant())
            {
                case "-recursive":
                case "--recursive":
                case "-r":
                    if (recursive)
                        return InvalidInvocation(error, "The recursive option was supplied more than once.");
                    recursive = true;
                    break;

                case "-out":
                case "--out":
                    if (reportPath is not null)
                        return InvalidInvocation(error, "The report output option was supplied more than once.");
                    if (++index >= args.Length
                        || string.IsNullOrWhiteSpace(args[index])
                        || args[index].StartsWith("-", StringComparison.Ordinal))
                        return InvalidInvocation(error, "The report output option requires a JSON file path.");
                    reportPath = args[index];
                    break;

                default:
                    if (argument.StartsWith("-", StringComparison.Ordinal))
                        return InvalidInvocation(error, $"Unknown scan option: {argument}");
                    if (string.IsNullOrWhiteSpace(argument))
                        return InvalidInvocation(error, "Input paths cannot be empty.");
                    inputs.Add(argument);
                    break;
            }
        }

        if (inputs.Count == 0)
            return InvalidInvocation(error, "At least one input file or directory is required.");
        if (reportPath is null)
            return InvalidInvocation(error, "A JSON report path is required with -out.");
        if (!string.Equals(Path.GetExtension(reportPath), ".json", StringComparison.OrdinalIgnoreCase))
            return InvalidInvocation(error, "The report output path must use the .json extension.");

        string normalizedReportPath;
        try
        {
            normalizedReportPath = fileSystem.GetFullPath(reportPath);
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            error.WriteLine($"The report output path could not be resolved: {exception.Message}");
            return 2;
        }

        IChunkScanReportDestination destination;
        try
        {
            destination = destinationFactory(normalizedReportPath);
        }
        catch (OutOfMemoryException)
        {
            error.WriteLine("Failed to reserve the scan report because memory was exhausted.");
            return 1;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            error.WriteLine($"Failed to reserve scan report '{normalizedReportPath}': {exception.Message}");
            return 1;
        }

        using (destination)
        {
            ChunkScanReport report;
            try
            {
                report = new ChunkScanService(fileSystem).Scan(inputs, recursive);
            }
            catch (OutOfMemoryException)
            {
                error.WriteLine("The scan stopped because memory was exhausted before a report could be finalized.");
                return 1;
            }
            catch (Exception exception) when (IsRecoverable(exception))
            {
                error.WriteLine($"The scan stopped after an unexpected {exception.GetType().Name}.");
                return 1;
            }

            foreach (var operationalError in report.OperationalErrors)
            {
                error.WriteLine(
                    $"{operationalError.Stage}: {operationalError.Identity}: {operationalError.Message}");
            }

            output.WriteLine(
                $"Discovered {report.Summary.FilesDiscovered} file(s); " +
                $"processed {report.Summary.FilesProcessed}; " +
                $"found {report.Summary.FindingCount} diagnostic issue(s); " +
                $"recorded {report.Summary.OperationalErrors} operational error(s); " +
                $"aborted={report.Summary.Aborted.ToString().ToLowerInvariant()}.");

            try
            {
                destination.Commit(report);
                output.WriteLine($"JSON report: {destination.OutputPath}");
            }
            catch (OutOfMemoryException)
            {
                error.WriteLine("Failed to write the scan report because memory was exhausted.");
                return 1;
            }
            catch (Exception exception) when (IsRecoverable(exception))
            {
                error.WriteLine($"Failed to write scan report '{destination.OutputPath}': {exception.Message}");
                return 1;
            }

            return report.Summary.Aborted || report.Summary.OperationalErrors > 0
                ? 1
                : 0;
        }
    }

    private static int RunWithConsole(string[] args) =>
        Run(
            args,
            Console.Out,
            Console.Error,
            new SystemChunkScanFileSystem(),
            ChunkScanJsonWriter.Reserve);

    private static int InvalidInvocation(TextWriter error, string message)
    {
        error.WriteLine(message);
        error.WriteLine($"Run '{Name} -usage' for command help.");
        return 2;
    }

    private static bool IsHelp(string argument) =>
        argument.Equals("-usage", StringComparison.OrdinalIgnoreCase)
        || argument.Equals("--help", StringComparison.OrdinalIgnoreCase)
        || argument.Equals("-h", StringComparison.OrdinalIgnoreCase);

    private static void PrintUsage(TextWriter output)
    {
        output.WriteLine($"cgf-converter {Name} <file-or-directory> [<file-or-directory> ...] -out <report.json> [-recursive]");
        output.WriteLine();
        output.WriteLine("Scans supported CryEngine files through the canonical parser without rendering.");
        output.WriteLine("Unknown chunk and nested #ivo datastream types, unimplemented types, unsupported versions, and parse failures are written to JSON.");
        output.WriteLine("Directories are scanned at the top level unless -recursive is supplied; recursive scans do not follow reparse points.");
        output.WriteLine("Reports use stable input aliases and relative file paths; the output path must end in .json.");
        output.WriteLine($"The corpus stops after {ChunkScanService.MaxConsecutiveResourceExhaustions} consecutive memory-exhausted files.");
        output.WriteLine("Exit codes: 0 = completed diagnostic, 1 = operational/aborted/report failure, 2 = invalid invocation.");
    }

    private static bool IsRecoverable(Exception exception) =>
        exception is not OperationCanceledException
            and not OutOfMemoryException
            and not AccessViolationException
            and not StackOverflowException;
}
