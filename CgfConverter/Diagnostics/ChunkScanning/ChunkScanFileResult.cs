using System.Collections.Generic;

namespace CgfConverter.Diagnostics.ChunkScanning;

internal sealed record ChunkScanFileResult(
    string InputPath,
    string? FileSignature,
    uint? FileVersion,
    IReadOnlyList<ChunkIssueRecord> Records);
