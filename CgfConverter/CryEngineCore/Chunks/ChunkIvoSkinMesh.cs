using CgfConverter.Models;

namespace CgfConverter.CryEngineCore;

public class ChunkIvoSkinMesh : Chunk
{
    public GeometryInfo geometryInfo { get; set; }

    public required IvoGeometryMeshDetails MeshDetails { get; set; }
    public required IvoMeshSubset IvoMeshSubset { get; set; }
    public required IvoDatastream<uint> Indices { get; set; }
    public IvoDatastream<VertUv> VertsUvs { get; set; }
    public IvoDatastream<Tangent> Tangents { get; set; }
    public IvoDatastream<IRGBA> Colors { get; set; }
    public IvoDatastream<IRGBA> Colors2 { get; set; }
}
