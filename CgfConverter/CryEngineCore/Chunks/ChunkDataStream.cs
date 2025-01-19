using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

public abstract class ChunkDataStream : Chunk // Contains data such as vertices, normals, etc.
{
    public uint Flags { get; set; } // not used, but looks like the start of the Data Stream chunk
    public uint Flags1 { get; set; } // not used.  UInt32 after Flags that looks like offsets
    public uint Flags2 { get; set; } // not used, looks almost like a filler.
    public DatastreamType DataStreamType { get; set; } // type of data (vertices, normals, uv, etc)
    public uint NumElements { get; set; } // Number of data entries
    public uint BytesPerElement { get; set; } // Bytes per data entry
    public uint Reserved1 { get; set; }
    public uint Reserved2 { get; set; }

    public Vector3[] Vertices { get; set; }
    public Vector3[] Normals { get; set; }
    public UV[] UVs { get; set; }
    public uint[] Indices { get; set; }
    public IRGBA[] Colors { get; set; }
    public IRGBA[] Colors2 { get; set; }

    public List<Quaternion> Tangents = []; // datastreamType of 6
    public List<Quaternion> BiTangents = [];
    public List<Quaternion> QTangents = [];
    public byte[,] ShCoeffs;     // for dataStreamType of 7, length is NumElement,BytesPerElements.
    public byte[,] ShapeDeformation; // for dataStreamType of 8, length is NumElements,BytesPerElement.

    //public byte[,] BoneMap { get; set; }      // for dataStreamType of 9, length is NumElements,BytesPerElement, 2.
    //public MeshBoneMapping[] BoneMap;      // for dataStreamType of 9, length is NumElements,BytesPerElement.
    //public byte[,] FaceMap { get; set; }      // for dataStreamType of 10, length is NumElements,BytesPerElement.
    //public byte[,] VertMats { get; set; }     // for dataStreamType of 11, length is NumElements,BytesPerElement.

    // Generic property to hold the current datastream
    public object? DataStream { get; protected set; }

    // Helper method to get strongly typed data
    public Datastream<T>? GetData<T>() => DataStream as Datastream<T>;

    public override string ToString() => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Ver: {Version}, Datastream Type: {DataStreamType}, Number of Elements: {NumElements}, Bytes per Element: {BytesPerElement}";
}
