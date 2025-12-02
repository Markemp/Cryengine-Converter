using CgfConverter.Models.Structs;
using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// CAF animation data chunk for Star Citizen #ivo CAF files.
/// Chunk ID: 0xA9496CB5 (IvoCAFData)
/// Contains a single #caf animation block with bone controllers and keyframe data.
/// </summary>
public class ChunkIvoCAF : Chunk
{
    /// <summary>The animation block header.</summary>
    public IvoAnimBlockHeader Header { get; set; }

    /// <summary>Bone hash array (CRC32 of bone names).</summary>
    public uint[] BoneHashes { get; set; } = [];

    /// <summary>Controller entries for each bone.</summary>
    public IvoAnimControllerEntry[] Controllers { get; set; } = [];

    /// <summary>Raw keyframe data.</summary>
    public byte[] KeyframeData { get; set; } = [];

    /// <summary>Parsed rotation keyframes per bone (controller ID -> rotations).</summary>
    public Dictionary<uint, List<Quaternion>> Rotations { get; set; } = [];

    /// <summary>Parsed position keyframes per bone (controller ID -> positions).</summary>
    public Dictionary<uint, List<Vector3>> Positions { get; set; } = [];

    /// <summary>Parsed rotation key times per bone (controller ID -> times).</summary>
    public Dictionary<uint, List<float>> RotationTimes { get; set; } = [];

    /// <summary>Parsed position key times per bone (controller ID -> times).</summary>
    public Dictionary<uint, List<float>> PositionTimes { get; set; } = [];
}
