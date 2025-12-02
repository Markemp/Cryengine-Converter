using CgfConverter.Models.Structs;
using System.Collections.Generic;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// DBA metadata chunk for Star Citizen #ivo DBA files.
/// Chunk ID: 0xF7351608 (IvoDBAMetadata)
/// Contains animation metadata entries and path string table.
/// </summary>
public class ChunkIvoDBAMetadata : Chunk
{
    /// <summary>Number of animations in the library.</summary>
    public uint AnimCount { get; set; }

    /// <summary>Reserved field.</summary>
    public uint Reserved { get; set; }

    /// <summary>Metadata entries for each animation.</summary>
    public List<IvoDBAMetaEntry> Entries { get; set; } = [];

    /// <summary>Animation path strings.</summary>
    public List<string> AnimPaths { get; set; } = [];
}
