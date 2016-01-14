using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkCompiledPhysicalProxies : Chunk        // 0xACDC0003:  Hit boxes?
    {
        // Properties.  VERY similar to datastream, since it's essential vertex info.
        public UInt32 Flags2;
        public UInt32 NumBones; // Number of data entries
        public UInt32 BytesPerElement; // Bytes per data entry
        //public UInt32 Reserved1;
        //public UInt32 Reserved2;
        public HitBox[] HitBoxes;

        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.NumBones = b.ReadUInt32(); // number of Bones in this chunk.
            // Utils.Log(LogLevelEnum.Debug, "Number of bones (hitboxes): {0}", NumBones);
            this.HitBoxes = new HitBox[NumBones];    // now have an array of hitboxes
            for (Int32 i = 0; i < NumBones; i++)
            {
                // Start populating the hitbox array
                this.HitBoxes[i].ID = b.ReadUInt32();
                this.HitBoxes[i].NumVertices = b.ReadUInt32();
                this.HitBoxes[i].NumIndices = b.ReadUInt32();
                this.HitBoxes[i].Unknown2 = b.ReadUInt32();      // Probably a fill of some sort?
                this.HitBoxes[i].Vertices = new Vector3[HitBoxes[i].NumVertices];
                this.HitBoxes[i].Indices = new UInt16[HitBoxes[i].NumIndices];

                //Utils.Log(LogLevelEnum.Debug, "Hitbox {0}, {1:X} Vertices and {2:X} Indices", i, HitBoxes[i].NumVertices, HitBoxes[i].NumIndices);
                for (Int32 j = 0; j < HitBoxes[i].NumVertices; j++)
                {
                    HitBoxes[i].Vertices[j].x = b.ReadSingle();
                    HitBoxes[i].Vertices[j].y = b.ReadSingle();
                    HitBoxes[i].Vertices[j].z = b.ReadSingle();
                    // Utils.Log(LogLevelEnum.Debug, "{0} {1} {2}",HitBoxes[i].Vertices[j].x,HitBoxes[i].Vertices[j].y,HitBoxes[i].Vertices[j].z);
                }
                // Read the indices
                for (Int32 j = 0; j < HitBoxes[i].NumIndices; j++)
                {
                    HitBoxes[i].Indices[j] = b.ReadUInt16();
                    //Utils.Log(LogLevelEnum.Debug, "Indices: {0}", HitBoxes[i].Indices[j]);
                }
                // Utils.Log(LogLevelEnum.Debug, "Index 0 is {0}, Index 9 is {1}", HitBoxes[i].Indices[0],HitBoxes[i].Indices[9]);
                // read the crap at the end so we can move on.
                for (Int32 j = 0; j < HitBoxes[i].Unknown2 / 2; j++)
                {
                    b.ReadUInt16();
                }
                // HitBoxes[i].WriteHitBox();
            }

        }
        public override void WriteChunk()
        {
            base.WriteChunk();
        }
    }
}
