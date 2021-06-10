using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkNode_80000823 : ChunkNode {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            Name = b.ReadFString(64);
            if (String.IsNullOrEmpty(Name))
                Name = "unknown";
            ObjectNodeID = Utils.SwapIntEndian(b.ReadInt32()); // Object reference ID
            ParentNodeID = Utils.SwapIntEndian(b.ReadInt32());
            __NumChildren = Utils.SwapIntEndian(b.ReadInt32());
            MatID = Utils.SwapIntEndian(b.ReadInt32());  // Material ID?
            SkipBytes(b, 4);

            // Read the 4x4 transform matrix.
            Matrix44 transform = new Matrix44
            {
                m11 = Utils.SwapSingleEndian(b.ReadSingle()),
                m12 = Utils.SwapSingleEndian(b.ReadSingle()),
                m13 = Utils.SwapSingleEndian(b.ReadSingle()),
                m14 = Utils.SwapSingleEndian(b.ReadSingle()),
                m21 = Utils.SwapSingleEndian(b.ReadSingle()),
                m22 = Utils.SwapSingleEndian(b.ReadSingle()),
                m23 = Utils.SwapSingleEndian(b.ReadSingle()),
                m24 = Utils.SwapSingleEndian(b.ReadSingle()),
                m31 = Utils.SwapSingleEndian(b.ReadSingle()),
                m32 = Utils.SwapSingleEndian(b.ReadSingle()),
                m33 = Utils.SwapSingleEndian(b.ReadSingle()),
                m34 = Utils.SwapSingleEndian(b.ReadSingle()),
                m41 = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
                m42 = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
                m43 = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
                m44 = Utils.SwapSingleEndian(b.ReadSingle()),
            };
            //original transform matrix is 3x4 stored as 4x4
            transform.m14 = transform.m24 = transform.m34 = 0d;
            transform.m44 = 1d;
            Transform = transform;

            // Read the position Pos Vector3
            Pos = new Vector3
            {
                x = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
                y = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
                z = Utils.SwapSingleEndian(b.ReadSingle()) * VERTEX_SCALE,
            };

            // Read the rotation Rot Quad
            Rot = new Quaternion
            {
                w = Utils.SwapSingleEndian(b.ReadSingle()),
                x = Utils.SwapSingleEndian(b.ReadSingle()),
                y = Utils.SwapSingleEndian(b.ReadSingle()),
                z = Utils.SwapSingleEndian(b.ReadSingle()),
            };

            // Read the Scale Vector 3
            Scale = new Vector3
            {
                x = Utils.SwapSingleEndian(b.ReadSingle()),
                y = Utils.SwapSingleEndian(b.ReadSingle()),
                z = Utils.SwapSingleEndian(b.ReadSingle()),
            };

            // read the controller pos/rot/scale
            PosCtrlID = Utils.SwapIntEndian(b.ReadInt32());
            RotCtrlID = Utils.SwapIntEndian(b.ReadInt32());
            SclCtrlID = Utils.SwapIntEndian(b.ReadInt32());

            Properties = b.ReadPString();
            // Good enough for now.
        }
    }
}
