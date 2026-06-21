using System.Numerics;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// Motion Parameters chunk (0x3002) - Contains animation timing and motion data.
/// Similar to MotionParams905 struct in DBA files, but as a standalone chunk in CAF files.
/// </summary>
public abstract class ChunkMotionParameters : Chunk
{
    public uint AssetFlags { get; internal set; }
    public uint Compression { get; internal set; }
    public int TicksPerFrame { get; internal set; }
    public float SecsPerTick { get; internal set; }
    public int Start { get; internal set; }
    public int End { get; internal set; }
    public float MoveSpeed { get; internal set; }
    public float TurnSpeed { get; internal set; }
    public float AssetTurn { get; internal set; }
    public float Distance { get; internal set; }
    public float Slope { get; internal set; }

    // StartLocation (QuatT = Quaternion + Vector3)
    public Quaternion StartLocationQ { get; internal set; }
    public Vector3 StartLocationT { get; internal set; }

    // EndLocation (QuatT = Quaternion + Vector3)
    public Quaternion EndLocationQ { get; internal set; }
    public Vector3 EndLocationT { get; internal set; }

    // Foot plant timing data
    public float LHeelStart { get; internal set; }
    public float LHeelEnd { get; internal set; }
    public float LToe0Start { get; internal set; }
    public float LToe0End { get; internal set; }
    public float RHeelStart { get; internal set; }
    public float RHeelEnd { get; internal set; }
    public float RToe0Start { get; internal set; }
    public float RToe0End { get; internal set; }

    /// <summary>Duration in seconds based on ticks.</summary>
    public float Duration => (End - Start) * SecsPerTick;

    /// <summary>Frame count.</summary>
    public int FrameCount => TicksPerFrame > 0 ? (End - Start) / TicksPerFrame : 0;

    public override string ToString() =>
        $"ChunkMotionParameters: Start={Start}, End={End}, TicksPerFrame={TicksPerFrame}, SecsPerTick={SecsPerTick}, Duration={Duration:F3}s";
}
