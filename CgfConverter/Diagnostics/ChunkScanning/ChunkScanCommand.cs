using System;
using System.IO;

namespace CgfConverter.Diagnostics.ChunkScanning;

public static class ChunkScanCommand
{
    public static int Run(string[] args)
    {
        var argsHandler = new ChunkScanArgsHandler();
        if (argsHandler.ProcessArgs(args) != 0)
        {
            ChunkScanArgsHandler.PrintUsage(Console.Error);
            return 1;
        }

        try
        {
            var report = new UnknownChunkScanner().Scan(argsHandler.Options);
            var writer = CreateWriter(argsHandler.Options.Format);

            if (argsHandler.Options.OutputPath is null)
            {
                writer.Write(report, Console.Out);
            }
            else
            {
                using var output = new StreamWriter(argsHandler.Options.OutputPath);
                writer.Write(report, output);
                Console.WriteLine("Wrote {0} report: {1}", argsHandler.Options.Format.ToString().ToLowerInvariant(), argsHandler.Options.OutputPath);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Failed to scan unknown chunks: {0}", ex.Message);
            return 1;
        }
    }

    private static IChunkScanReportWriter CreateWriter(ChunkScanReportFormat format)
    {
        return format switch
        {
            ChunkScanReportFormat.Csv => new CsvChunkScanReportWriter(),
            ChunkScanReportFormat.Json => new JsonChunkScanReportWriter(),
            _ => new ConsoleChunkScanReportWriter(),
        };
    }
}
