using System.IO;

namespace CgfConverter.Diagnostics.ChunkScanning;

public interface IChunkScanReportWriter
{
    void Write(ChunkScanReport report, TextWriter writer);
}
