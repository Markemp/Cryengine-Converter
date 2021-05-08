using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkMtlName_800 : ChunkMtlName
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            MatType = (MtlNameType)b.ReadUInt32();
            // if 0x01, then material lib.  If 0x12, mat name.  This is actually a bitstruct.
            SkipBytes(b, 4);               // NFlags2
            Name = b.ReadFString(128);
            PhysicsType = new MtlNamePhysicsType[] { (MtlNamePhysicsType)b.ReadUInt32() };
            NumChildren = b.ReadUInt32();
            // Now we need to read the Children references.  2 parts; the number of children, and then 66 - numchildren padding
            ChildIDs = new uint[NumChildren];
            for (int i = 0; i < NumChildren; i++)
            {
                ChildIDs[i] = b.ReadUInt32();
            }
            SkipBytes(b, 32);
        }
    }
}
