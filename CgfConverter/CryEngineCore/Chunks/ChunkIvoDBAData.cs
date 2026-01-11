using CgfConverter.Models.Structs;
using System.Collections.Generic;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// DBA animation data chunk for Star Citizen #ivo DBA files.
/// Chunk ID: 0x194FBC50 (IvoDBAData)
/// Contains multiple #dba animation blocks.
/// </summary>
public class ChunkIvoDBAData : Chunk
{
    /// <summary>Total size of all animation blocks.</summary>
    public uint TotalDataSize { get; set; }

    /// <summary>Parsed animation blocks.</summary>
    public List<IvoAnimationBlock> AnimationBlocks { get; set; } = [];
}
