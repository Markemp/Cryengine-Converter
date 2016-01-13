using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkMeshSubsets : Chunk // cccc0017:  The different parts of a mesh.  Needed for obj exporting
    {
        public UInt32 Flags; // probably the offset
        public UInt32 NumMeshSubset; // number of mesh subsets
        public MeshSubset[] MeshSubsets;

        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.Flags = b.ReadUInt32();   // Might be a ref to this chunk
            this.NumMeshSubset = b.ReadUInt32();  // number of mesh subsets
            this.SkipBytes(b, 8);
            this.MeshSubsets = new MeshSubset[NumMeshSubset];
            for (Int32 i = 0; i < NumMeshSubset; i++)
            {
                this.MeshSubsets[i].FirstIndex = b.ReadUInt32();
                this.MeshSubsets[i].NumIndices = b.ReadUInt32();
                this.MeshSubsets[i].FirstVertex = b.ReadUInt32();
                this.MeshSubsets[i].NumVertices = b.ReadUInt32();
                this.MeshSubsets[i].MatID = b.ReadUInt32();
                this.MeshSubsets[i].Radius = b.ReadSingle();
                this.MeshSubsets[i].Center.x = b.ReadSingle();
                this.MeshSubsets[i].Center.y = b.ReadSingle();
                this.MeshSubsets[i].Center.z = b.ReadSingle();
            }
        }
        public override void WriteChunk()
        {
            Console.WriteLine("*** START MESH SUBSET CHUNK ***");
            Console.WriteLine("    ChunkType:       {0}", ChunkType);
            Console.WriteLine("    Mesh SubSet ID:  {0:X}", ID);
            Console.WriteLine("    Number of Mesh Subsets: {0}", NumMeshSubset);
            for (Int32 i = 0; i < NumMeshSubset; i++)
            {
                Console.WriteLine("        ** Mesh Subset:          {0}", i);
                Console.WriteLine("           First Index:          {0}", MeshSubsets[i].FirstIndex);
                Console.WriteLine("           Number of Indices:    {0}", MeshSubsets[i].NumIndices);
                Console.WriteLine("           First Vertex:         {0}", MeshSubsets[i].FirstVertex);
                Console.WriteLine("           Number of Vertices:   {0}  (next will be {1})", MeshSubsets[i].NumVertices, MeshSubsets[i].NumVertices + MeshSubsets[i].FirstVertex);
                Console.WriteLine("           Material ID:          {0}", MeshSubsets[i].MatID);
                Console.WriteLine("           Radius:               {0}", MeshSubsets[i].Radius);
                Console.WriteLine("           Center:   {0},{1},{2}", MeshSubsets[i].Center.x, MeshSubsets[i].Center.y, MeshSubsets[i].Center.z);
                Console.WriteLine("        ** Mesh Subset {0} End", i);
            }
            Console.WriteLine("*** END MESH SUBSET CHUNK ***");
        }
    }
}