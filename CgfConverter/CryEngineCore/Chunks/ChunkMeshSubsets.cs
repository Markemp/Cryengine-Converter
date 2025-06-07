using CgfConverter.Utilities;
using System.Collections.Generic;

namespace CgfConverter.CryEngineCore;

public abstract class ChunkMeshSubsets : Chunk // cccc0017:  The different parts of a mesh.  Needed for obj exporting
{
    public uint Flags { get; set; } // probably the offset
    public uint NumMeshSubset { get; set; } // number of mesh subsets
    public List<MeshSubset> MeshSubsets { get; set; } = [];

    public override string ToString() => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Number of Mesh Subsets: {NumMeshSubset}";

    public void WriteChunk()
    {
        HelperMethods.Log(LogLevelEnum.Verbose, "*** START MESH SUBSET CHUNK ***");
        HelperMethods.Log(LogLevelEnum.Verbose, "    ChunkType:       {0}", ChunkType);
        HelperMethods.Log(LogLevelEnum.Verbose, "    Mesh SubSet ID:  {0:X}", ID);
        HelperMethods.Log(LogLevelEnum.Verbose, "    Number of Mesh Subsets: {0}", NumMeshSubset);
        for (int i = 0; i < NumMeshSubset; i++)
        {
            HelperMethods.Log(LogLevelEnum.Verbose, "        ** Mesh Subset:          {0}", i);
            HelperMethods.Log(LogLevelEnum.Verbose, "           First Index:          {0}", MeshSubsets[i].FirstIndex);
            HelperMethods.Log(LogLevelEnum.Verbose, "           Number of Indices:    {0}", MeshSubsets[i].NumIndices);
            HelperMethods.Log(LogLevelEnum.Verbose, "           First Vertex:         {0}", MeshSubsets[i].FirstVertex);
            HelperMethods.Log(LogLevelEnum.Verbose, "           Number of Vertices:   {0}  (next will be {1})", MeshSubsets[i].NumVertices, MeshSubsets[i].NumVertices + MeshSubsets[i].FirstVertex);
            HelperMethods.Log(LogLevelEnum.Verbose, "           Material ID:          {0}", MeshSubsets[i].MatID);
            HelperMethods.Log(LogLevelEnum.Verbose, "           Radius:               {0}", MeshSubsets[i].Radius);
            HelperMethods.Log(LogLevelEnum.Verbose, "           Center:   {0},{1},{2}", MeshSubsets[i].Center.X, MeshSubsets[i].Center.Y, MeshSubsets[i].Center.Z);
            HelperMethods.Log(LogLevelEnum.Verbose, "        ** Mesh Subset {0} End", i);
        }
        HelperMethods.Log(LogLevelEnum.Verbose, "*** END MESH SUBSET CHUNK ***");
    }
}
