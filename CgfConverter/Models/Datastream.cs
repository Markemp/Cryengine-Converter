namespace CgfConverter.Models;

public interface IDatastream
{
    DatastreamType Type { get; }
    uint NumElements { get; }
    uint BytesPerElement { get; }
}

public sealed record Datastream<T>(DatastreamType type, uint numElements, uint bytesPerElement, T[] data) : IDatastream
{
    public DatastreamType Type { get; set; } = type;
    public uint NumElements { get; set; } = numElements;
    public uint BytesPerElement { get; set; } = bytesPerElement;
    public T[] Data { get; set; } = data;
}
