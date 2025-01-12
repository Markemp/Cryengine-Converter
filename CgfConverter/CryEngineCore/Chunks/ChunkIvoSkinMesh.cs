using CgfConverter.Models;
using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

public class ChunkIvoSkinMesh : Chunk
{
    public GeometryInfo? GeometryInfo { get; set; }

    public required IvoGeometryMeshDetails MeshDetails { get; set; }
    public required List<MeshSubset> MeshSubsets { get; set; } = [];
    public required IvoDatastream<uint> Indices { get; set; }
    public IvoDatastream<VertUV>? VertsUvs { get; set; }
    public IvoDatastream<Vector3>? Normals { get; set; }
    public IvoDatastream<Tangent>? Tangents { get; set; }
    public IvoDatastream<Tangent>? BiTangents { get; set; }
    public IvoDatastream<Quaternion>? QTangents { get; set; }
    public IvoDatastream<IRGBA>? Colors { get; set; }
    public IvoDatastream<IRGBA>? Colors2 { get; set; }
    public IvoDatastream<MeshBoneMapping>? BoneMappings { get; set; }
}
