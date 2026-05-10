using System;
using System.Collections.Generic;
using CgfConverter.Utilities;

namespace CgfConverter.Diagnostics.ChunkScanning;

public sealed class ChunkScanOptions
{
    public List<string> Inputs { get; } = [];
    public bool Recursive { get; set; }
    public ChunkScanReportFormat Format { get; set; } = ChunkScanReportFormat.Console;
    public string? OutputPath { get; set; }
    public int MaxThreads { get; set; } = Environment.ProcessorCount;
    public LogLevelEnum? LogLevel { get; set; }
}
