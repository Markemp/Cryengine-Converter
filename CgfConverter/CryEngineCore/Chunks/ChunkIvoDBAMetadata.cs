using CgfConverter.Models.Structs;
using System.Collections.Generic;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// DBA metadata chunk for Star Citizen #ivo DBA files.
/// Chunk ID: 0xF7351608 (IvoDBAMetadata)
/// Contains animation metadata entries (44 bytes each) and path string table.
/// </summary>
public class ChunkIvoDBAMetadata : Chunk
{
    /// <summary>Number of animations in the library.</summary>
    public uint AnimCount { get; set; }

    /// <summary>Metadata entries for each animation (44 bytes each).</summary>
    public List<IvoDBAMetaEntry> Entries { get; set; } = [];

    /// <summary>Animation path strings (null-terminated).</summary>
    public List<string> AnimPaths { get; set; } = [];
}
