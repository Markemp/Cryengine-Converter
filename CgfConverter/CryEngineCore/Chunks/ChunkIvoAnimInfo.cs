namespace CgfConverter.CryEngineCore;

/// <summary>
/// Animation info chunk for Star Citizen #ivo CAF files.
/// Chunk ID: 0x4733C6ED (IvoAnimInfo)
/// Contains animation metadata: bone count, position track count, scale, precision.
/// </summary>
public class ChunkIvoAnimInfo : Chunk
{
    /// <summary>Flags (usually 2).</summary>
    public uint Flags { get; set; }

    /// <summary>Unknown value.</summary>
    public ushort Unknown1 { get; set; }

    /// <summary>Number of bones in the animation.</summary>
    public ushort NumBones { get; set; }

    /// <summary>Unknown value (usually 0).</summary>
    public uint Unknown2 { get; set; }

    /// <summary>Number of bones with position animation data.</summary>
    public uint NumPositionTracks { get; set; }

    /// <summary>Bounding box minimum or special values.</summary>
    public float[] BoundMin { get; set; } = new float[3];

    /// <summary>Scale factor (approximately 1.0).</summary>
    public float Scale { get; set; }

    /// <summary>Precision value (very small double).</summary>
    public double Precision { get; set; }

    /// <summary>Padding/reserved bytes.</summary>
    public uint[] Padding { get; set; } = new uint[2];
}
