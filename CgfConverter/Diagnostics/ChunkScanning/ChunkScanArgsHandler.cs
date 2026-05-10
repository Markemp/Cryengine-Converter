using System;
using System.IO;
using CgfConverter.Utilities;

namespace CgfConverter.Diagnostics.ChunkScanning;

public sealed class ChunkScanArgsHandler
{
    public ChunkScanOptions Options { get; } = new();

    public int ProcessArgs(string[] args, TextWriter? errorWriter = null)
    {
        errorWriter ??= Console.Error;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "-recursive":
                    Options.Recursive = true;
                    break;
                case "-format":
                    if (!TryReadValue(args, ref i, errorWriter, "-format", out var format))
                        return 1;
                    if (!Enum.TryParse(format, true, out ChunkScanReportFormat parsedFormat))
                    {
                        errorWriter.WriteLine("Invalid scan report format '{0}'. Expected console, csv, or json.", format);
                        return 1;
                    }
                    Options.Format = parsedFormat;
                    break;
                case "-out":
                    if (!TryReadValue(args, ref i, errorWriter, "-out", out var outputPath))
                        return 1;
                    Options.OutputPath = outputPath;
                    break;
                case "-mt":
                case "-maxthreads":
                    if (!TryReadValue(args, ref i, errorWriter, args[i], out var maxThreadsText))
                        return 1;
                    if (!int.TryParse(maxThreadsText, out var maxThreads) || maxThreads < 0)
                    {
                        errorWriter.WriteLine("Invalid max thread count '{0}'. Expected a non-negative integer.", maxThreadsText);
                        return 1;
                    }
                    Options.MaxThreads = maxThreads == 0 ? Environment.ProcessorCount : maxThreads;
                    break;
                case "-loglevel":
                    if (!TryReadValue(args, ref i, errorWriter, "-loglevel", out var logLevel))
                        return 1;
                    if (!Enum.TryParse(logLevel, true, out LogLevelEnum parsedLevel))
                    {
                        errorWriter.WriteLine("Invalid log level '{0}'.", logLevel);
                        return 1;
                    }
                    Options.LogLevel = parsedLevel;
                    HelperMethods.LogLevel = parsedLevel;
                    break;
                case "-usage":
                case "-help":
                case "--help":
                    PrintUsage(Console.Out);
                    return 1;
                default:
                    if (args[i].StartsWith('-'))
                    {
                        errorWriter.WriteLine("Unknown scan option '{0}'.", args[i]);
                        return 1;
                    }
                    Options.Inputs.Add(args[i]);
                    break;
            }
        }

        if (Options.Inputs.Count == 0)
        {
            errorWriter.WriteLine("scan-unknown-chunks requires at least one input file or directory.");
            return 1;
        }

        if (Options.OutputPath is not null)
            Options.Format = InferFormat(Options.OutputPath, Options.Format);

        return 0;
    }

    public static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("cgf-converter scan-unknown-chunks <input> [options]");
        writer.WriteLine();
        writer.WriteLine("Options:");
        writer.WriteLine("  -recursive        Recursively scan local directories.");
        writer.WriteLine("  -format <fmt>     Output format: console, csv, or json.");
        writer.WriteLine("  -out <file>       Write report to a file. .json/.csv infer format when -format is omitted.");
        writer.WriteLine("  -mt <n>           Max threads. 0 = all cores.");
        writer.WriteLine("  -loglevel <lvl>   Optional log level.");
        writer.WriteLine();
        writer.WriteLine("Scanned extensions:");
        writer.WriteLine("  .cgf .cga .chr .skin .anim .soc .caf .dba");
    }

    private static bool TryReadValue(
        string[] args,
        ref int index,
        TextWriter errorWriter,
        string option,
        out string value)
    {
        if (++index >= args.Length)
        {
            errorWriter.WriteLine("{0} requires a value.", option);
            value = string.Empty;
            return false;
        }

        value = args[index];
        return true;
    }

    private static ChunkScanReportFormat InferFormat(string outputPath, ChunkScanReportFormat currentFormat)
    {
        return Path.GetExtension(outputPath).ToLowerInvariant() switch
        {
            ".json" => ChunkScanReportFormat.Json,
            ".csv" => ChunkScanReportFormat.Csv,
            _ => currentFormat
        };
    }
}
