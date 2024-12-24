using CgfConverter.Models;

namespace CgfConverter.CryEngineCore;

public class ChunkIvoSkinMesh : Chunk
{
    public GeometryInfo geometryInfo { get; set; }

    public ChunkMesh meshChunk { get; set; }
    public ChunkMeshSubsets meshSubsetsChunk { get; set; }
    public ChunkDataStream indices { get; set; }
    public ChunkDataStream vertsUvs { get; set; }
    public ChunkDataStream tangents { get; set; }
    public ChunkDataStream colors { get; set; }
    public ChunkDataStream colors2 { get; set; }
}
