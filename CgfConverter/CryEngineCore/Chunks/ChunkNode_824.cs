using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkNode_824 : ChunkNode {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            Name = b.ReadFString(64);
            if (String.IsNullOrEmpty(Name))
                Name = "unknown";
            ObjectNodeID = b.ReadInt32(); // Object reference ID
            ParentNodeID = b.ReadInt32();
            __NumChildren = b.ReadInt32();
            MatID = b.ReadInt32();  // Material ID?
            SkipBytes(b, 4);

            // Read the 4x4 transform matrix.
            Matrix44 transform = new Matrix44
            {
                m11 = b.ReadSingle(),
                m12 = b.ReadSingle(),
                m13 = b.ReadSingle(),
                m14 = b.ReadSingle(),
                m21 = b.ReadSingle(),
                m22 = b.ReadSingle(),
                m23 = b.ReadSingle(),
                m24 = b.ReadSingle(),
                m31 = b.ReadSingle(),
                m32 = b.ReadSingle(),
                m33 = b.ReadSingle(),
                m34 = b.ReadSingle(),
                m41 = b.ReadSingle() * VERTEX_SCALE,
                m42 = b.ReadSingle() * VERTEX_SCALE,
                m43 = b.ReadSingle() * VERTEX_SCALE,
                m44 = b.ReadSingle(),
            };
            //original transform matrix is 3x4 stored as 4x4
            transform.m14 = transform.m24 = transform.m34 = 0d;
            transform.m44 = 1d;
            Transform = transform;

            // Read the position Pos Vector3
            Pos = new Vector3
            {
                x = b.ReadSingle() * VERTEX_SCALE,
                y = b.ReadSingle() * VERTEX_SCALE,
                z = b.ReadSingle() * VERTEX_SCALE,
            };
           
            // Read the rotation Rot Quad
            Rot = new Quaternion
            {
                w = b.ReadSingle(),
                x = b.ReadSingle(),
                y = b.ReadSingle(),
                z = b.ReadSingle(),
            };

            // Read the Scale Vector 3
            Scale = new Vector3
            {
                x = b.ReadSingle(),
                y = b.ReadSingle(),
                z = b.ReadSingle(),
            };

            // read the controller pos/rot/scale
            PosCtrlID = b.ReadInt32();
            RotCtrlID = b.ReadInt32();
            SclCtrlID = b.ReadInt32();

            Properties = b.ReadPString();
            // Good enough for now.
        }
    }
}
