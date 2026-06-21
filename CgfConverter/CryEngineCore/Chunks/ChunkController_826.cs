using Extensions;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// Controller chunk version 0x826 — TCB/Bezier (Far Cry / CE1–3 era).
///
/// Header (16 bytes after standard chunk header):
///   uint32  type         // CtrlType enum — selects key struct
///   int32   numKeys
///   uint32  nFlags
///   uint32  controllerId // CRC32 of bone name
///
/// Key structs selected by type:
///   Vec3 types  (TBC3/LINEAR3/BEZIER3)  → CryTCB3Key: {int32 time; Vec3 val; float t,c,b,ein,eout;}   36 bytes
///   Quat types  (TBCQ/LINEARQ/BEZIERQ)  → CryTCBQKey: {int32 time; Quat val; float t,c,b,ein,eout;}   40 bytes
///   Float types (TBC1/LINEAR1/BEZIER1)  → CryTCB1Key: {int32 time; float val; float t,c,b,ein,eout;}  28 bytes
///
/// Note: 826 is not currently wired up in CryEngine.cs (Far Cry/CE1–3 era only).
/// </summary>
internal sealed class ChunkController_826 : ChunkController
{
    public CtrlType ControllerType { get; internal set; }
    public int NumKeys { get; internal set; }
    public uint ControllerFlags { get; internal set; }
    public uint ControllerID { get; internal set; }

    /// <summary>Vec3 keyframes (TBC3/LINEAR3/BEZIER3). TCB params are read and discarded.</summary>
    public List<(int Time, Vector3 Value)> Vec3Keys { get; internal set; } = [];

    /// <summary>Quaternion keyframes (TBCQ/LINEARQ/BEZIERQ). TCB params are read and discarded.</summary>
    public List<(int Time, Quaternion Value)> QuatKeys { get; internal set; } = [];

    /// <summary>Scalar keyframes (TBC1/LINEAR1/BEZIER1). TCB params are read and discarded.</summary>
    public List<(int Time, float Value)> FloatKeys { get; internal set; } = [];

    public override string ToString() =>
        $"Chunk Type: {ChunkType}, ID: {ID:X}, NumKeys: {NumKeys}, Controller ID: {ControllerID:X}, Controller Type: {ControllerType}, Flags: {ControllerFlags}";

    public override void Read(BinaryReader b)
    {
        base.Read(b);

        ControllerType = (CtrlType)b.ReadUInt32();
        NumKeys = b.ReadInt32();
        ControllerFlags = b.ReadUInt32();
        ControllerID = b.ReadUInt32();

        switch (ControllerType)
        {
            // CryTCB3Key — 36 bytes: int32 time, Vec3 val, 5× float TCB params
            case CtrlType.TBC3:
            case CtrlType.LINEAR3:
            case CtrlType.BEZIER3:
                for (int i = 0; i < NumKeys; i++)
                {
                    int time = b.ReadInt32();
                    Vector3 val = b.ReadVector3();
                    b.ReadSingle(); b.ReadSingle(); b.ReadSingle(); // t, c, b
                    b.ReadSingle(); b.ReadSingle();                  // ein, eout
                    Vec3Keys.Add((time, val));
                }
                break;

            // CryTCBQKey — 40 bytes: int32 time, Quat val, 5× float TCB params
            case CtrlType.TBCQ:
            case CtrlType.LINEARQ:
            case CtrlType.BEZIERQ:
                for (int i = 0; i < NumKeys; i++)
                {
                    int time = b.ReadInt32();
                    Quaternion val = b.ReadQuaternion();
                    b.ReadSingle(); b.ReadSingle(); b.ReadSingle(); // t, c, b
                    b.ReadSingle(); b.ReadSingle();                  // ein, eout
                    QuatKeys.Add((time, val));
                }
                break;

            // CryTCB1Key — 28 bytes: int32 time, float val, 5× float TCB params
            case CtrlType.TBC1:
            case CtrlType.LINEAR1:
            case CtrlType.BEZIER1:
                for (int i = 0; i < NumKeys; i++)
                {
                    int time = b.ReadInt32();
                    float val = b.ReadSingle();
                    b.ReadSingle(); b.ReadSingle(); b.ReadSingle(); // t, c, b
                    b.ReadSingle(); b.ReadSingle();                  // ein, eout
                    FloatKeys.Add((time, val));
                }
                break;

            default:
                // NONE, CRYBONE, BSPLINE* etc. — not used in practice for CAF animation
                break;
        }
    }
}
