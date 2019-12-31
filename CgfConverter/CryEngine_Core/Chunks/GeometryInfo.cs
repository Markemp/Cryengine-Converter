using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    /// <summary>
    /// Geometry info contains all the vertex, color, normal, UV, tangent, index, etc.  Basically if you have a Node chunk with a Mesh and Submesh, this will contain the summary of all
    /// the datastream chunks that contain geometry info.
    /// </summary>
    public class GeometryInfo
    {
        /// <summary>
        /// The MeshSubset chunk that contains this geometry.
        /// </summary>
        public ChunkMeshSubsets GeometrySubset { get; set; }
        public Vector3[] Vertices { get; set; }  // For dataStreamType of 0, length is NumElements. 
        public Vector3[] Normals { get; set; }   // For dataStreamType of 1, length is NumElements.
        public UV[] UVs { get; set; }            // for datastreamType of 2, length is NumElements.
        public IRGB[] RGBColors { get; set; }    // for dataStreamType of 3, length is NumElements.  Bytes per element of 3
        public IRGBA[] RGBAColors { get; set; }  // for dataStreamType of 4, length is NumElements.  Bytes per element of 4
        public UInt32[] Indices { get; set; }    // for dataStreamType of 5, length is NumElements.
        // For Tangents on down, this may be a 2 element array.  See line 846+ in cgf.xml
        public Tangent[,] Tangents { get; set; }  // for dataStreamType of 6, length is NumElements, 2.  
        public Byte[,] ShCoeffs { get; set; }     // for dataStreamType of 7, length is NumElement,BytesPerElements.
        public Byte[,] ShapeDeformation { get; set; } // for dataStreamType of 8, length is NumElements,BytesPerElement.
        public Byte[,] BoneMap { get; set; }      // for dataStreamType of 9, length is NumElements,BytesPerElement, 2.
        //public MeshBoneMapping[] BoneMap;      // for dataStreamType of 9, length is NumElements,BytesPerElement.
        public Byte[,] FaceMap { get; set; }      // for dataStreamType of 10, length is NumElements,BytesPerElement.
        public Byte[,] VertMats { get; set; }     // for dataStreamType of 11, length is NumElements,BytesPerElement.


    }
}
