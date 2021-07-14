using System.Numerics;

namespace CgfConverter.CryEngineCore
{
    /// <summary>
    /// Geometry info contains all the vertex, color, normal, UV, tangent, index, etc.  Basically if you have a Node chunk with a Mesh and Submesh, 
    /// this will contain the summary of all the datastream chunks that contain geometry info.
    /// </summary>
    public class GeometryInfo
    {
        public ChunkMeshSubsets GeometrySubset { get; set; }
        public Vector3[] Vertices { get; set; }
        public Vector3[] Normals { get; set; }
        public UV[] UVs { get; set; }
        public IRGBA[] Colors { get; set; }
        public uint[] Indices { get; set; }
        // For Tangents on down, this may be a 2 element array.  See line 846+ in cgf.xml
        public Tangent[,] Tangents { get; set; }
        public byte[,] ShCoeffs { get; set; }
        public byte[,] ShapeDeformation { get; set; }
        public byte[,] BoneMap { get; set; }
        public byte[,] FaceMap { get; set; }
        public byte[,] VertMats { get; set; }
    }
}
