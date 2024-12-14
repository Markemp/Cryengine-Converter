using System.Collections.Generic;

namespace CgfConverter.CryEngineCore.Chunks;

abstract class ChunkNodeMeshCombo : Chunk
{
    // #ivo format for .cga/cgf files.
    public int Flags1 { get; internal set; }
    public int NumberOfNodes { get; internal set; }
    public int NumberOfMeshSubsets { get; internal set; } 
    public List<string> NodeNames { get; internal set; } = new();
    public int NumberOfBoneIndices { get; internal set; } // ?



}
