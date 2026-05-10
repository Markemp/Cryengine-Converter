namespace CgfConverter.Diagnostics.ChunkScanning;

public sealed record ChunkScanSummary
{
    public int FilesDiscovered { get; init; }
    public int FilesScanned { get; init; }
    public int FilesWithIssues { get; init; }
    public int UnknownChunkOccurrences { get; init; }
    public int UnsupportedVersionOccurrences { get; init; }
    public int ParseFailureOccurrences { get; init; }
    public int ErrorFiles { get; init; }
}
