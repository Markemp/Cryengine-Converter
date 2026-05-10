using System;
using System.Collections.Generic;

namespace CgfConverter.Diagnostics.ChunkScanning;

public sealed record ChunkScanReport
{
    public string SchemaVersion { get; init; } = "1.0";
    public DateTimeOffset GeneratedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<string> Inputs { get; init; } = [];
    public required ChunkScanSummary Summary { get; init; }
    public IReadOnlyList<ChunkIssueRecord> Records { get; init; } = [];
}
