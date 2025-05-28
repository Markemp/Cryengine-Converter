using CgfConverter.Models;

namespace CgfConverter.CryEngineCore;

public abstract class ChunkDataStream : Chunk // Contains data such as vertices, normals, etc.
{
    public uint Flags2 { get; set; } // not used, looks almost like a filler. Set to 257 for SC files
    public DatastreamType DataStreamType { get; set; } // type of data (vertices, normals, uv, etc)
    public uint NumElements { get; set; } // Number of data entries
    public uint BytesPerElement { get; set; } // Bytes per data entry

    public IDatastream? Data { get; protected set; }

    public override string ToString() => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Ver: {Version}, Datastream Type: {DataStreamType}, Number of Elements: {NumElements}, Bytes per Element: {BytesPerElement}";
}
