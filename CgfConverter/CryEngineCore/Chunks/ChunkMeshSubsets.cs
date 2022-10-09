namespace CgfConverter.CryEngineCore;

public abstract class ChunkMeshSubsets : Chunk // cccc0017:  The different parts of a mesh.  Needed for obj exporting
{
    public uint Flags; // probably the offset
    public uint NumMeshSubset; // number of mesh subsets
    public MeshSubset[] MeshSubsets;

    // For bone ID meshes? Not sure where this is used yet.
    public uint NumberOfBoneIDs;
    public ushort[] BoneIDs;

    public override string ToString() => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Number of Mesh Subsets: {NumMeshSubset}";

    public void WriteChunk()
    {
        Utils.Log(LogLevelEnum.Verbose, "*** START MESH SUBSET CHUNK ***");
        Utils.Log(LogLevelEnum.Verbose, "    ChunkType:       {0}", ChunkType);
        Utils.Log(LogLevelEnum.Verbose, "    Mesh SubSet ID:  {0:X}", ID);
        Utils.Log(LogLevelEnum.Verbose, "    Number of Mesh Subsets: {0}", NumMeshSubset);
        for (int i = 0; i < NumMeshSubset; i++)
        {
            Utils.Log(LogLevelEnum.Verbose, "        ** Mesh Subset:          {0}", i);
            Utils.Log(LogLevelEnum.Verbose, "           First Index:          {0}", MeshSubsets[i].FirstIndex);
            Utils.Log(LogLevelEnum.Verbose, "           Number of Indices:    {0}", MeshSubsets[i].NumIndices);
            Utils.Log(LogLevelEnum.Verbose, "           First Vertex:         {0}", MeshSubsets[i].FirstVertex);
            Utils.Log(LogLevelEnum.Verbose, "           Number of Vertices:   {0}  (next will be {1})", MeshSubsets[i].NumVertices, MeshSubsets[i].NumVertices + MeshSubsets[i].FirstVertex);
            Utils.Log(LogLevelEnum.Verbose, "           Material ID:          {0}", MeshSubsets[i].MatID);
            Utils.Log(LogLevelEnum.Verbose, "           Radius:               {0}", MeshSubsets[i].Radius);
            Utils.Log(LogLevelEnum.Verbose, "           Center:   {0},{1},{2}", MeshSubsets[i].Center.X, MeshSubsets[i].Center.Y, MeshSubsets[i].Center.Z);
            Utils.Log(LogLevelEnum.Verbose, "        ** Mesh Subset {0} End", i);
        }
        Utils.Log(LogLevelEnum.Verbose, "*** END MESH SUBSET CHUNK ***");
    }
}