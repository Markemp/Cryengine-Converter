using System.Collections.Generic;

namespace CgfConverter.CryEngineCore.Chunks;

abstract class ChunkNodeDetails : Chunk
{
    public int NumberOfNodes { get; internal set; }
    public int StringTableSize { get; internal set; }
    public int Flags1 { get; internal set; }
    public int Flags2 { get; internal set; }
    required public List<NodeDetail> NodeDetails { get; internal set; }
    required public List<string> NodeNames { get; internal set; }
}

public class NodeDetail
{
    public uint ControllerId { get; set; }
    public short Unknown { get; set; }
    public short ParentIndex { get; set; } // 0xffff if root
    public short NumberOfChildren { get; set; }
    public short ObjectNodeId { get; set; } // points to mesh info identifier (like chunk id)
    public short Unknown2 { get; set; } // always 0xffff
    public short Unknown3 { get; set; } // always 0xffff
}
