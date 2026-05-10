using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CgfConverter.Diagnostics.ChunkScanning;

public sealed class JsonChunkScanReportWriter : IChunkScanReportWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public void Write(ChunkScanReport report, TextWriter writer)
    {
        writer.Write(JsonSerializer.Serialize(report, Options));
        writer.WriteLine();
    }
}
