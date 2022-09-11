using System;
using System.IO;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

public class ChunkNode_80000823 : ChunkNode
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        Name = b.ReadFString(64);
        if (string.IsNullOrEmpty(Name))
            Name = "unknown";
        ObjectNodeID = Utils.SwapIntEndian(b.ReadInt32()); // Object reference ID
        ParentNodeID = Utils.SwapIntEndian(b.ReadInt32());
        __NumChildren = Utils.SwapIntEndian(b.ReadInt32());
        MatID = Utils.SwapIntEndian(b.ReadInt32());  // Material ID?
        SkipBytes(b, 4);

        // Read the 4x4 transform matrix.
        Matrix4x4 transform = new Matrix4x4
        {
            M11 = Utils.SwapSingleEndian(b.ReadSingle()),
            M12 = Utils.SwapSingleEndian(b.ReadSingle()),
            M13 = Utils.SwapSingleEndian(b.ReadSingle()),
            M14 = Utils.SwapSingleEndian(b.ReadSingle()),
            M21 = Utils.SwapSingleEndian(b.ReadSingle()),
            M22 = Utils.SwapSingleEndian(b.ReadSingle()),
            M23 = Utils.SwapSingleEndian(b.ReadSingle()),
            M24 = Utils.SwapSingleEndian(b.ReadSingle()),
            M31 = Utils.SwapSingleEndian(b.ReadSingle()),
            M32 = Utils.SwapSingleEndian(b.ReadSingle()),
            M33 = Utils.SwapSingleEndian(b.ReadSingle()),
            M34 = Utils.SwapSingleEndian(b.ReadSingle()),
            M41 = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
            M42 = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
            M43 = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
            M44 = Utils.SwapSingleEndian(b.ReadSingle()),
        };
        //original transform matrix is 3x4 stored as 4x4
        transform.M14 = transform.M24 = transform.M34 = 0f;
        transform.M44 = 1f;
        Transform = transform;

        // Read the position Pos Vector3
        Pos = new Vector3
        {
            X = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
            Y = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
            Z = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
        };

        // Read the rotation Rot Quad
        Rot = new Quaternion
        {
            X = Utils.SwapSingleEndian(b.ReadSingle()),
            Y = Utils.SwapSingleEndian(b.ReadSingle()),
            Z = Utils.SwapSingleEndian(b.ReadSingle()),
            W = Utils.SwapSingleEndian(b.ReadSingle()),
        };

        // Read the Scale Vector 3
        Scale = new Vector3
        {
            X = Utils.SwapSingleEndian(b.ReadSingle()),
            Y = Utils.SwapSingleEndian(b.ReadSingle()),
            Z = Utils.SwapSingleEndian(b.ReadSingle()),
        };

        // read the controller pos/rot/scale
        PosCtrlID = Utils.SwapIntEndian(b.ReadInt32());
        RotCtrlID = Utils.SwapIntEndian(b.ReadInt32());
        SclCtrlID = Utils.SwapIntEndian(b.ReadInt32());

        Properties = b.ReadPString();
        // Good enough for now.
    }
}
