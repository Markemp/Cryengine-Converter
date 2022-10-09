using Extensions;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkCompiledPhysicalProxies_800 : ChunkCompiledPhysicalProxies
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        SkinningInfo skin = GetSkinningInfo();

        NumPhysicalProxies = b.ReadUInt32(); // number of Bones in this chunk.
        PhysicalProxies = new PhysicalProxy[NumPhysicalProxies];    // now have an array of physical proxies
        
        for (int i = 0; i < NumPhysicalProxies; i++)
        {
            // Start populating the physical proxy array.  This is the Header.
            PhysicalProxies[i].ID = b.ReadUInt32();
            PhysicalProxies[i].NumVertices = b.ReadUInt32();
            PhysicalProxies[i].NumIndices = b.ReadUInt32();
            PhysicalProxies[i].Material = b.ReadUInt32();      // Probably a fill of some sort?
            PhysicalProxies[i].Vertices = new Vector3[PhysicalProxies[i].NumVertices];
            PhysicalProxies[i].Indices = new ushort[PhysicalProxies[i].NumIndices];

            for (int j = 0; j < PhysicalProxies[i].NumVertices; j++)
            {
                PhysicalProxies[i].Vertices[j] = b.ReadVector3();
            }
            
            for (int j = 0; j < PhysicalProxies[i].NumIndices; j++)
            {
                PhysicalProxies[i].Indices[j] = b.ReadUInt16();
            }
            // read the crap at the end so we can move on.
            for (int j = 0; j < PhysicalProxies[i].Material; j++)
            {
                b.ReadByte();
            }
        }
        skin.PhysicalBoneMeshes = PhysicalProxies.ToList();
    }
}
