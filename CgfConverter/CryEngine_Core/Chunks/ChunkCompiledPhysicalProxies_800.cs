using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkCompiledPhysicalProxies_800 : ChunkCompiledPhysicalProxies
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            SkinningInfo skin = GetSkinningInfo();

            this.NumPhysicalProxies = b.ReadUInt32(); // number of Bones in this chunk.
            //Utils.Log(LogLevelEnum.Debug, "Number of bones (physical proxies): {0}", NumPhysicalProxies);
            this.PhysicalProxies = new PhysicalProxy[NumPhysicalProxies];    // now have an array of physical proxies
            for (Int32 i = 0; i < NumPhysicalProxies; i++)
            {
                // Start populating the physical proxy array.  This is the Header.
                this.PhysicalProxies[i].ID = b.ReadUInt32();
                this.PhysicalProxies[i].NumVertices = b.ReadUInt32();
                this.PhysicalProxies[i].NumIndices = b.ReadUInt32();
                this.PhysicalProxies[i].Material = b.ReadUInt32();      // Probably a fill of some sort?
                this.PhysicalProxies[i].Vertices = new Vector3[PhysicalProxies[i].NumVertices];
                this.PhysicalProxies[i].Indices = new UInt16[PhysicalProxies[i].NumIndices];

                for (Int32 j = 0; j < PhysicalProxies[i].NumVertices; j++)
                {
                    PhysicalProxies[i].Vertices[j].x = b.ReadSingle();
                    PhysicalProxies[i].Vertices[j].y = b.ReadSingle();
                    PhysicalProxies[i].Vertices[j].z = b.ReadSingle();
                }
                // Read the indices
                for (Int32 j = 0; j < PhysicalProxies[i].NumIndices; j++)
                {
                    PhysicalProxies[i].Indices[j] = b.ReadUInt16();
                    //Utils.Log(LogLevelEnum.Debug, "Indices: {0}", HitBoxes[i].Indices[j]);
                }
                // Utils.Log(LogLevelEnum.Debug, "Index 0 is {0}, Index 9 is {1}", HitBoxes[i].Indices[0],HitBoxes[i].Indices[9]);
                // read the crap at the end so we can move on.
                for (Int32 j = 0; j < PhysicalProxies[i].Material; j++)
                {
                    b.ReadByte();
                }
            }
            skin.PhysicalBoneMeshes = PhysicalProxies.ToList();
        }

        public override void WriteChunk()
        {
            base.WriteChunk();
        }
    }
}
