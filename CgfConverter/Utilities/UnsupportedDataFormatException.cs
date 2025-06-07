using System;

namespace CgfConverter.Utilities;

public class UnsupportedDataFormatException : Exception
{
    public DatastreamType DataStreamType { get; }
    public uint BytesPerElement { get; }

    public UnsupportedDataFormatException(DatastreamType type, uint bytesPerElement, string message)
        : base(message)
    {
        DataStreamType = type;
        BytesPerElement = bytesPerElement;
    }
}
