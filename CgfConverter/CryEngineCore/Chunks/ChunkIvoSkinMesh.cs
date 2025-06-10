using CgfConverter.Models;
using CgfConverter.Models.Structs;
using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

public class ChunkIvoSkinMesh : Chunk
{
    public required IvoGeometryMeshDetails MeshDetails { get; set; }
    public required List<MeshSubset> MeshSubsets { get; set; } = [];
    public required Datastream<uint> Indices { get; set; }
    public Datastream<VertUV>? VertsUvs { get; set; }
    public Datastream<Vector3>? Normals { get; set; }
    public Datastream<Quaternion>? Tangents { get; set; }
    public Datastream<Quaternion>? BiTangents { get; set; }
    public Datastream<Quaternion>? QTangents { get; set; }
    public Datastream<IRGBA>? Colors { get; set; }
    public Datastream<IRGBA>? Colors2 { get; set; }
    public Datastream<MeshBoneMapping>? BoneMappings { get; set; }
}
