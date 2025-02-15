using CgfConverter.Models;
using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

public abstract class ChunkDataStream : Chunk // Contains data such as vertices, normals, etc.
{
    public uint Flags2 { get; set; } // not used, looks almost like a filler.
    public DatastreamType DataStreamType { get; set; } // type of data (vertices, normals, uv, etc)
    public uint NumElements { get; set; } // Number of data entries
    public uint BytesPerElement { get; set; } // Bytes per data entry

    public IDatastream? DataStream { get; protected set; }

    // Remove these properties
    //public Vector3[] Vertices { get; set; }
    //public Vector3[] Normals { get; set; }
    //public UV[] UVs { get; set; }
    //public uint[] Indices { get; set; }
    //public IRGBA[] Colors { get; set; }
    //public IRGBA[] Colors2 { get; set; }
    //public List<Quaternion> Tangents { get; set; } = []; // datastreamType of 6
    //public List<Quaternion> BiTangents { get; set; } = [];
    //public List<Quaternion> QTangents { get; set; } = [];

    public override string ToString() => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Ver: {Version}, Datastream Type: {DataStreamType}, Number of Elements: {NumElements}, Bytes per Element: {BytesPerElement}";
}
