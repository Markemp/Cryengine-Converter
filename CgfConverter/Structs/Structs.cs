using Extensions;
using CgfConverter.Structs;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace CgfConverter
{
    public struct RangeEntity
    {
        public string Name { get; set; } // String32!  32 byte char array.
        public int Start { get; set; }
        public int End { get; set; }
    }

    /// <summary>
    /// Vertex with position p(Vector3) and normal n(Vector3)
    /// </summary>
    public struct Vertex
    {
        public Vector3 p;  // position
        public Vector3 n;  // normal
    }

    public struct Face        // mesh face (3 vertex, Material index, smoothing group.  All ints)
    {
        public int v0; // first vertex
        public int v1; // second vertex
        public int v2; // third vertex
        public int Material; // Material Index
        public int SmGroup; //smoothing group
    }

    public struct MeshSubset
    {
        public int FirstIndex;
        public int NumIndices;
        public int FirstVertex;
        public int NumVertices;
        public int MatID;
        public float Radius;
        public Vector3 Center;
    }  // Contains data about the parts of a mesh, such as vertices, radius and center.

    public struct Key
    {
        public int Time; // Time in ticks
        public Vector3 AbsPos; // absolute position
        public Vector3 RelPos; // relative position
        public Quaternion RelQuat; //Relative Quaternion if ARG==1?
        public Vector3 Unknown1; // If ARG==6 or 10?
        public float[] Unknown2; // If ARG==9?  array length = 2
    }

    public struct UV
    {
        public float U;
        public float V;
    }

    public struct UVFace
    {
        public int t0; // first vertex index
        public int t1; // second vertex index
        public int t2; // third vertex index
    }

    public struct ControllerInfo
    {
        public uint ControllerID;
        public uint PosKeyTimeTrack;
        public uint PosTrack;
        public uint RotKeyTimeTrack;
        public uint RotTrack;
    }

    /*public struct TextureMap
    {

    }*/
    // Fill this in later.  line 369 in cgf.xml.

    public struct IRGB
    {
        public byte r; // red
        public byte g; // green
        public byte b; // blue

        public IRGB Read(BinaryReader b)
        {
            return new IRGB
            {
                r = b.ReadByte(),
                g = b.ReadByte(),
                b = b.ReadByte()
            };
        }
    }

    public struct IRGBA
    {
        public byte r; // red
        public byte g; // green
        public byte b; // blue
        public byte a; // alpha
        public IRGBA Read(BinaryReader b)
        {
            return new IRGBA
            {
                r = b.ReadByte(),
                g = b.ReadByte(),
                b = b.ReadByte(),
                a = b.ReadByte()
            };
        }
    }

    public struct Tangent
    {
        // Tangents.  Divide each component by 32767 to get the actual value
        public float x;
        public float y;
        public float z;
        public float w;  // Handness?  Either 32767 (+1.0) or -32767 (-1.0)
    }

    public struct SkinVertex
    {
        public int Volumetric;
        public int[] Index;     // Array of 4 ints
        public float[] w;       // Array of 4 floats
        public Matrix3x3 M;
    }

    /// <summary> WORLDTOBONE is also the Bind Pose Matrix (BPM) </summary>
    public struct WORLDTOBONE
    {
        //public float[,] worldToBone;   //  4x3 structure

        //public WORLDTOBONE(Matrix3x3 worldRotation, Vector3 worldTransform) : this()
        //{
        //    worldToBone = new float[3, 4];
        //    worldToBone[0, 0] = worldRotation.M11;
        //    worldToBone[0, 1] = worldRotation.M12;
        //    worldToBone[0, 2] = worldRotation.M13;
        //    worldToBone[0, 3] = worldTransform.X;
        //    worldToBone[1, 0] = worldRotation.M21;
        //    worldToBone[1, 1] = worldRotation.M22;
        //    worldToBone[1, 2] = worldRotation.M23;
        //    worldToBone[1, 3] = worldTransform.Y;
        //    worldToBone[2, 0] = worldRotation.M31;
        //    worldToBone[2, 1] = worldRotation.M32;
        //    worldToBone[2, 2] = worldRotation.M33;
        //    worldToBone[2, 3] = worldTransform.Z;
        //}

        //public void GetWorldToBone(BinaryReader b)
        //{
        //    worldToBone = new float[3, 4];
        //    for (int i = 0; i < 3; i++)
        //    {
        //        for (int j = 0; j < 4; j++)
        //        {
        //            worldToBone[i, j] = b.ReadSingle();  // this might have to be switched to [j,i].  Who knows???
        //        }
        //    }
        //    return;
        //}

        //internal Matrix3x3 GetWorldToBoneRotationMatrix()
        //{
        //    Matrix3x3 result = new Matrix3x3
        //    {
        //        M11 = worldToBone[0, 0],
        //        M12 = worldToBone[0, 1],
        //        M13 = worldToBone[0, 2],
        //        M21 = worldToBone[1, 0],
        //        M22 = worldToBone[1, 1],
        //        M23 = worldToBone[1, 2],
        //        M31 = worldToBone[2, 0],
        //        M32 = worldToBone[2, 1],
        //        M33 = worldToBone[2, 2]
        //    };
        //    return result;
        //}

        //internal Vector3 GetWorldToBoneTranslationVector()
        //{
        //    Vector3 result = new Vector3
        //    {
        //        X = (float)worldToBone[0, 3],
        //        Y = (float)worldToBone[1, 3],
        //        Z = (float)worldToBone[2, 3]
        //    };
        //    return result;
        //}
    }

    /// <summary> BONETOWORLD contains the world space location/rotation of a bone. </summary>
    public struct BONETOWORLD
    {
        //public float[,] boneToWorld;   //  4x3 structure

        //public BONETOWORLD(Matrix3x3 matrix33, Vector3 relativeTransform) : this()
        //{
        //    boneToWorld = new float[3, 4];
        //    boneToWorld[0, 0] = matrix33.M11;
        //    boneToWorld[0, 1] = matrix33.M12;
        //    boneToWorld[0, 2] = matrix33.M13;
        //    boneToWorld[1, 0] = matrix33.M21;
        //    boneToWorld[1, 1] = matrix33.M22;
        //    boneToWorld[1, 2] = matrix33.M23;
        //    boneToWorld[2, 0] = matrix33.M31;
        //    boneToWorld[2, 1] = matrix33.M32;
        //    boneToWorld[2, 2] = matrix33.M33;
        //    boneToWorld[0, 3] = relativeTransform.X;
        //    boneToWorld[1, 3] = relativeTransform.Y;
        //    boneToWorld[2, 3] = relativeTransform.Z;
        //}

        //public void ReadBoneToWorld(BinaryReader b)
        //{
        //    boneToWorld = new float[3, 4];

        //    for (int i = 0; i < 3; i++)
        //    {
        //        for (int j = 0; j < 4; j++)
        //        {
        //            boneToWorld[i, j] = b.ReadSingle();
        //        }
        //    }
        //    return;
        //}

        //public Matrix3x3 GetBoneToWorldRotationMatrix()
        //{
        //    Matrix3x3 result = new Matrix3x3
        //    {
        //        M11 = boneToWorld[0, 0],
        //        M12 = boneToWorld[0, 1],
        //        M13 = boneToWorld[0, 2],
        //        M21 = boneToWorld[1, 0],
        //        M22 = boneToWorld[1, 1],
        //        M23 = boneToWorld[1, 2],
        //        M31 = boneToWorld[2, 0],
        //        M32 = boneToWorld[2, 1],
        //        M33 = boneToWorld[2, 2]
        //    };
        //    return result;
        //}

        //public Vector3 GetBoneToWorldTranslationVector()
        //{
        //    Vector3 result = new Vector3
        //    {
        //        X = boneToWorld[0, 3],
        //        Y = boneToWorld[1, 3],
        //        Z = boneToWorld[2, 3]
        //    };
        //    return result;
        //}
    }

    public struct PhysicsGeometry
    {
        public uint physicsGeom;
        public uint flags;              // 0x0C ?
        public Vector3 min;
        public Vector3 max;
        public Vector3 spring_angle;
        public Vector3 spring_tension;
        public Vector3 damping;
        public Matrix3x3 framemtx;

        public void ReadPhysicsGeometry(BinaryReader b)      // Read a PhysicsGeometry structure
        {
            physicsGeom = b.ReadUInt32();
            flags = b.ReadUInt32();
            min = b.ReadVector3();
            max = b.ReadVector3();
            spring_angle = b.ReadVector3();
            spring_tension = b.ReadVector3();
            damping = b.ReadVector3();
            framemtx = b.ReadMatrix3x3();
            return;
        }
    }

    public class CompiledPhysicalBone
    {
        public uint BoneIndex;
        public uint ParentOffset;
        public uint NumChildren;
        public uint ControllerID;
        public char[] prop;
        public PhysicsGeometry PhysicsGeometry;

        // Calculated values
        public long offset;
        public uint parentID;                       // ControllerID of parent
        public List<uint> childIDs;                 // Not part of read struct.  Contains the controllerIDs of the children to this bone.

        public void ReadCompiledPhysicalBone(BinaryReader b)
        {
            // Reads just a single 584 byte entry of a bone. At the end the seek position will be advanced, so keep that in mind.
            BoneIndex = b.ReadUInt32();                // unique id of bone (generated from bone name)
            ParentOffset = b.ReadUInt32();
            NumChildren = b.ReadUInt32();
            ControllerID = b.ReadUInt32();
            prop = b.ReadChars(32);                    // Not sure what this is used for.
            PhysicsGeometry.ReadPhysicsGeometry(b);

            childIDs = new List<uint>();               // Calculated
        }
    }

    public struct InitialPosMatrix
    {
        // A bone initial position matrix.
        Matrix3x3 Rotation;              // type="Matrix33">
        Vector3 Position;                // type="Vector3">
    }

    public struct BoneLink
    {
        public int BoneID;
        public Vector3 offset;
        public float Blending;
    }

    public class DirectionalBlends
    {
        public string AnimToken;
        public uint AnimTokenCRC32;
        public string ParaJointName;
        public short ParaJointIndex;
        public short RotParaJointIndex;
        public string StartJointName;
        public short StartJointIndex;
        public short RotStartJointIndex;
        public string ReferenceJointName;
        public short ReferenceJointIndex;

        public DirectionalBlends()
        {
            AnimToken = string.Empty;
            AnimTokenCRC32 = 0;
            ParaJointName = string.Empty;
            ParaJointIndex = -1;
            RotParaJointIndex = -1;
            StartJointName = string.Empty;
            StartJointIndex = -1;
            RotStartJointIndex = -1;
            ReferenceJointName = string.Empty;
            ReferenceJointIndex = 1;  //by default we use the Pelvis
        }
    };

    #region Skinning Structures

    public struct BoneEntity
    {
        readonly int Bone_Id;                 //" type="int">Bone identifier.</add>
        readonly int Parent_Id;               //" type="int">Parent identifier.</add>
        readonly int Num_Children;            //" type="uint" />
        readonly uint Bone_Name_CRC32;         //" type="uint">CRC32 of bone name as listed in the BoneNameListChunk.  In Python this can be calculated using zlib.crc32(name)</add>
        readonly string Properties;            //" type="String32" />
        BonePhysics Physics;            //" type="BonePhysics" />
    }

    public struct BonePhysics           // 26 total words = 104 total bytes
    {
        readonly uint Geometry;                //" type="Ref" template="BoneMeshChunk">Geometry of a separate mesh for this bone.</add>
                                                 //<!-- joint parameters -->

        readonly uint Flags;                   //" type="uint" />
        Vector3 Min;                   //" type="Vector3" />
        Vector3 Max;                   //" type="Vector3" />
        Vector3 Spring_Angle;          //" type="Vector3" />
        Vector3 Spring_Tension;        //" type="Vector3" />
        Vector3 Damping;               //" type="Vector3" />
        Matrix3x3 Frame_Matrix;        //" type="Matrix33" />
    }

    public struct MeshBoneMapping
    {
        // 4 bones, 4 weights for each vertex mapping.
        public int[] BoneIndex;
        public int[] Weight;                    // Byte / 256?
    }

    public struct MeshPhysicalProxyHeader
    {
        public uint ChunkID;
        public uint NumPoints;
        public uint NumIndices;
        public uint NumMaterials;
    }

    public struct MeshMorphTargetHeader
    {
        public uint MeshID;
        public uint NameLength;
        public uint NumIntVertices;
        public uint NumExtVertices;
    }

    public struct MeshMorphTargetVertex
    {
        public uint VertexID;
        public Vector3 Vertex;

        public static MeshMorphTargetVertex Read(BinaryReader b)
        {
            MeshMorphTargetVertex vertex = new MeshMorphTargetVertex();
            vertex.VertexID = b.ReadUInt32();
            vertex.Vertex = b.ReadVector3();
            return vertex;
        }
    }

    public struct MorphTargets
    {
        readonly uint MeshID;
        readonly string Name;
        readonly List<MeshMorphTargetVertex> IntMorph;
        readonly List<MeshMorphTargetVertex> ExtMorph;
    }

    public struct TFace
    {
        public ushort I0 { get; set; }
        public ushort I1 { get; set; }
        public ushort I2 { get; set; }
    }

    public class MeshCollisionInfo
    {
        // AABB AABB;       // Bounding box structures?
        // OBB OBB;         // Has an m33, h and c value.
        public Vector3 Position;
        public List<short> Indices;
        public int BoneID;
    }

    public struct IntSkinVertex
    {
        public Vector3 Obsolete0;
        public Vector3 Position;
        public Vector3 Obsolete2;
        public ushort[] BoneIDs;     // 4 bone IDs
        public float[] Weights;     // Should be 4 of these
        public IRGBA Color;
    }

    public struct SpeedChunk
    {
        public float Speed;
        public float Distance;
        public float Slope;
        public int AnimFlags;
        public float[] MoveDir;
        public Quaternion StartPosition;
    }

    public struct PhysicalProxy
    {
        public uint ID;             // Chunk ID (although not technically a chunk
        public uint FirstIndex;
        public uint NumIndices;
        public uint FirstVertex;
        public uint NumVertices;
        public uint Material;     // Size of the weird data at the end of the hitbox structure.
        public Vector3[] Vertices;    // Array of vertices (x,y,z) length NumVertices
        public ushort[] Indices;      // Array of indices
    }

    public struct PhysicalProxyStub
    {
        readonly uint ChunkID;
        readonly List<Vector3> Points;
        readonly List<short> Indices;
        readonly List<string> Materials;
    }

    #endregion

    public struct PhysicsCube
    {
        public PhysicsStruct1 Unknown14;
        public PhysicsStruct1 Unknown15;
        public int Unknown16;
    }

    public struct PhysicsPolyhedron
    {
        public uint NumVertices;
        public uint NumTriangles;
        public int Unknown17;
        public int Unknown18;
        public byte HasVertexMap;
        public ushort[] VertexMap; // Array length NumVertices.  If the (non-physics) mesh has say 200 vertices, then the first 200
                                   //entries of this map give a mapping identifying the unique vertices.
                                   //The meaning of the extra entries is unknown.
        public byte UseDatasStream;
        public Vector3[] Vertices; // Array Length NumVertices
        public ushort[] Triangles; // Array length NumTriangles
        public byte Unknown210;
        public byte[] TriangleFlags; // Array length NumTriangles
        public ushort[] TriangleMap; // Array length NumTriangles
        public byte[] Unknown45; // Array length 16
        public int Unknown461;  //0
        public int Unknown462;  //0
        public float Unknown463; // 0.02
        public float Unknown464;
        // There is more.  See cgf.xml for the rest, but probably not really needed
    }

    public struct PhysicsCylinder
    {
        public float[] Unknown1;  // array length 8
        public int Unknown2;
        public PhysicsDataType2 Unknown3;
    }

    public struct PhysicsShape6
    {
        public float[] Unknown1; // array length 8
        public int Unknown2;
        public PhysicsDataType2 Unknown3;
    }

    public struct PhysicsDataType0
    {
        public int NumData;
        public PhysicsStruct2[] Data; // Array length NumData
        public int[] Unknown33; // array length 3
        public float Unknown80;
    }

    public struct PhysicsDataType1
    {
        public uint NumData1;  // usually 4294967295
        public PhysicsStruct50[] Data1; // Array length NumData1
        public int NumData2;
        public PhysicsStruct50[] Data2; // Array length NumData2
        public float[] Unknown60; // array length 6
        public Matrix3x3 Unknown61; // Rotation matrix?
        public int[] Unknown70; //Array length 3
        public float Unknown80;
    }

    public struct PhysicsDataType2
    {
        public Matrix3x3 Unknown1;
        public int Unknown;
        public float[] Unknown3; // array length 6
        public int Unknown4;
    }

    public struct PhysicsStruct1
    {
        public Matrix3x3 Unknown1;
        public int Unknown2;
        public float[] Unknown3; // array length 6
    }

    public struct PhysicsStruct2
    {
        public Matrix3x3 Unknown1;
        public float[] Unknown2;  // array length 6
        public int[] Unknown3; // array length 3
    }

    public struct PhysicsStruct50
    {
        public short Unknown11;
        public short Unknown12;
        public short Unknown21;
        public short Unknown22;
        public short Unknown23;
        public short Unknown24;
    }
}