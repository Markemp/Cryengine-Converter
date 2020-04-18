using System;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkDataStream : Chunk // cccc0016:  Contains data such as vertices, normals, etc.
    {
        public UInt32 Flags; // not used, but looks like the start of the Data Stream chunk
        public UInt32 Flags1; // not used.  UInt32 after Flags that looks like offsets
        public UInt32 Flags2; // not used, looks almost like a filler.
        public DataStreamTypeEnum DataStreamType { get; set; } // type of data (vertices, normals, uv, etc)
        public UInt32 NumElements; // Number of data entries
        public UInt32 BytesPerElement; // Bytes per data entry
        public UInt32 Reserved1;
        public UInt32 Reserved2;
        // Need to be careful with using float for Vertices and normals.  technically it's a floating point of length BytesPerElement.  May need to fix this.
        public Vector3[] Vertices;  // For dataStreamType of 0, length is NumElements. 
        public Vector3[] Normals;   // For dataStreamType of 1, length is NumElements.

        public UV[] UVs;            // for datastreamType of 2, length is NumElements.
        public IRGB[] RGBColors;    // for dataStreamType of 3, length is NumElements.  Bytes per element of 3
        public IRGBA[] RGBAColors;  // for dataStreamType of 4, length is NumElements.  Bytes per element of 4
        public UInt32[] Indices;    // for dataStreamType of 5, length is NumElements.
        // For Tangents on down, this may be a 2 element array.  See line 846+ in cgf.xml
        public Tangent[,] Tangents;  // for dataStreamType of 6, length is NumElements, 2.  
        public Byte[,] ShCoeffs;     // for dataStreamType of 7, length is NumElement,BytesPerElements.
        public Byte[,] ShapeDeformation; // for dataStreamType of 8, length is NumElements,BytesPerElement.
        public Byte[,] BoneMap;      // for dataStreamType of 9, length is NumElements,BytesPerElement, 2.
        //public MeshBoneMapping[] BoneMap;      // for dataStreamType of 9, length is NumElements,BytesPerElement.
        public Byte[,] FaceMap;      // for dataStreamType of 10, length is NumElements,BytesPerElement.
        public Byte[,] VertMats;     // for dataStreamType of 11, length is NumElements,BytesPerElement.

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Ver: {Version}, Datastream Type: {DataStreamType}, Number of Elements: {NumElements}, Bytes per Element: {BytesPerElement}";
        }
    }
}
