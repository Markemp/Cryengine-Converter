using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.Models;

/// <summary>
/// Geometry info contains all the vertex, color, normal, UV, tangent, index, etc.  Basically if you have a Node chunk with a Mesh and Submesh, 
/// this will contain the summary of all the datastream chunks that contain geometry info.
/// </summary>
public sealed class GeometryInfo
{
    //public ChunkMeshSubsets GeometrySubset { get; set; }
    public List<uint> Indices { get; set; } = [];
    public List<UV> UVs { get; set; } = [];
    public List<Vector3> Vertices { get; set; } = [];
    public List<Vector3> Normals { get; set; } = [];
    public List<IRGBA>? Colors { get; set; }
    public List<MeshBoneMapping>? BoneMappings { get; set; }
}
