using CgfConverter.Structs;
using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

abstract class ChunkNodeMeshCombo : Chunk
{
    // #ivo format for .cga/cgf files.
    public int ZeroPad { get; internal set; }
    public int NumberOfNodes { get; internal set; }
    // Not all nodes will have a mesh chunk.  Examples:  Helper nodes
    public int NumberOfMeshes { get; internal set; }
    public int NumberOfMeshSubsets { get; internal set; }
    public int GeometryTypeFlag { get; internal set; } // 0x0 if has geometry, otherwise 0x2 or 0x3
    public int Unknown1 { get; internal set; }
    public int Unknown2 { get; internal set; }
    public int Unknown3 { get; internal set; }
    public int StringTableSize { get; internal set; }
    public uint MeshChunkId { get; set; } // If the node has a geometry mesh, this is the chunk id for it.
    public required List<NodeMeshCombo> NodeMeshCombos { get; internal set; }

    public required List<ushort> MaterialIndices { get; internal set; }
    public required List<ushort> UnknownIndices { get; internal set; }

    public required List<string> NodeNames { get; internal set; }
}

public class NodeMeshCombo
{
    public Matrix3x4 WorldToBone { get; set; }
    public Matrix3x4 BoneToWorld { get; set; }
    public Vector3 ScaleComponent { get; set; }
    public uint Id { get; set; }
    public uint Unknown2 { get; set; }
    public ushort ParentIndex { get; set; }
    public IvoGeometryType GeometryType { get; set; }
    public Vector3 BoundingBoxMin { get; set; }
    public Vector3 BoundingBoxMax { get; set; }
    public uint[] Unknown3 { get; set; } = new uint[4];
    public uint NumberOfVertices { get; set; }
    public ushort NumberOfChildren { get; set; }
    // Chunk ID of the mesh chunk for this node.  If no geometry, it's 0 (helper)
    public ushort MeshChunkId { get; set; }
    // Everything past this point is currently unknown
    //struct Unknown
    //{
    //    ushort unkShort;
    //    ushort unkShort; // Also seems to always be 256
    //    VECTOR3 unkVec1<comment=PrintVector3>; // Both of the following have values which are conveniently floats that are either 1.0 or -1.0, 
    //    VECTOR3 unkVec1<comment=PrintVector3>; // with first value of the second vector being 0.
    //    uint unkInt;
    //    uint unkInt_256; // Seems to always be 256?
    //    uint unkInt_0;
    //}
}
