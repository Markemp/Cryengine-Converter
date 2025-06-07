using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.Models;

/// <summary>
/// Geometry info contains all the vertex, color, normal, UV, indices, etc.  Basically if you have a Node chunk with a Mesh and Submesh, 
/// this will contain the summary of all the datastream chunks that contain geometry info.
/// </summary>
public sealed record GeometryInfo
{
    public List<MeshSubset>? GeometrySubsets { get; set; } = [];
    public Datastream<uint>? Indices { get; set; }
    public Datastream<Vector3>? Vertices { get; set; }
    public Datastream<UV>? UVs { get; set; }
    public Datastream<Vector3>? Normals { get; set; }
    public Datastream<IRGBA>? Colors { get; set; }
    public Datastream<VertUV>? VertUVs { get; set; }
    public Datastream<MeshBoneMapping>? BoneMappings { get; set; }
    public required BoundingBox BoundingBox { get; set; }
    public PhysicsData? PhysicsData { get; set; }
}
