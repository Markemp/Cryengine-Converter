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
            Transform = new Matrix44
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
                m41 = Utils.SwapSingleEndian(b.ReadSingle()),
                m42 = Utils.SwapSingleEndian(b.ReadSingle()),
                m43 = Utils.SwapSingleEndian(b.ReadSingle()),
                m44 = Utils.SwapSingleEndian(b.ReadSingle()),
            };

            // Read the position Pos Vector3
            Pos = new Vector3
            {
                x = Utils.SwapSingleEndian(b.ReadSingle() / 100),
                y = Utils.SwapSingleEndian(b.ReadSingle() / 100),
                z = Utils.SwapSingleEndian(b.ReadSingle() / 100),
            };

            // Read the rotation Rot Quad
            Rot = new Quat
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
