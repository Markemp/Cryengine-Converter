namespace CgfConverter.Diagnostics.ChunkScanning;

public sealed record ChunkIssueRecord
{
    public required ChunkIssueKind IssueKind { get; init; }
    public required string InputPath { get; init; }
    public string? RelativePath { get; init; }
    public string? Extension { get; init; }
    public string? ModelFileName { get; init; }
    public string? FileSignature { get; init; }
    public uint? FileVersion { get; init; }
    public uint? ChunkTypeValue { get; init; }
    public string? ChunkTypeHex { get; init; }
    public string? ChunkTypeName { get; init; }
    public string? ChunkVersionRawHex { get; init; }
    public string? ChunkVersionHex { get; init; }
    public int? ChunkId { get; init; }
    public bool ChunkIdIsGenerated { get; init; }
    public uint? ChunkOffset { get; init; }
    public string? ChunkOffsetHex { get; init; }
    public uint? ChunkSize { get; init; }
    public uint? ChunkDataSize { get; init; }
    public int? OccurrenceIndex { get; init; }
    public ChunkParserStage ParserStage { get; init; }
    public string? ExceptionType { get; init; }
    public string? ExceptionMessage { get; init; }
}
