using CgfConverter.CryEngineCore;
using System;
using System.IO;

namespace CgfConverter.CryEngine_Core.Chunks
{
    class ChunkMtlName_80000800 : ChunkMtlName
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.MatType = (MtlNameTypeEnum)Utils.SwapUIntEndian(b.ReadUInt32());
            // if 0x01, then material lib.  If 0x12, mat name.  This is actually a bitstruct.
            this.SkipBytes(b, 4);               // NFlags2
            this.Name = b.ReadFString(128);
            this.PhysicsType = new MtlNamePhysicsType[] { (MtlNamePhysicsType)Utils.SwapUIntEndian(b.ReadUInt32()) };

            this.NumChildren = Utils.SwapUIntEndian(b.ReadUInt32());
            
            //this.NumChildren = b.ReadUInt32();
            // Now we need to read the Children references.  2 parts; the number of children, and then 66 - numchildren padding
            this.ChildIDs = new uint[this.NumChildren];
            for (Int32 i = 0; i < this.NumChildren; i++)
            {
                this.ChildIDs[i] = Utils.SwapUIntEndian(b.ReadUInt32());
            }
            this.SkipBytes(b, 32);
        }
    }
}
