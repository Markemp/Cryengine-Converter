using System;
using System.IO;

namespace CgfConverter
{
    public struct String16
    {
        public char[] Data;
    }   // 16 byte char array.  THESE MUST BE CALLED WITH THE PROPER LENGTH!
    public struct String32
    {
        public char[] Data;
    }    // 32 byte char array 
    public struct String64
    {
        public char[] Data;
    }  // 64 byte char array 
    public struct String128
    {
        public char[] Data;
    }   // 128 byte char array 
    public struct String256
    {
        public char[] Data;

        public String ReadString256(BinaryReader b)  // Read 256 chars, return a String.
        {
            char[] tempData = new char[256];
            int stringLength = 0;
            String tempString;

            for (int i = 0; i < 256; i++)
            {
                tempData[i] = b.ReadChar();
            }
            for (int i = 0; i < tempData.Length; i++)
            {
                if (tempData[i] == 0)
                {
                    stringLength = i;
                    break;
                }
            }
            tempString = new string(tempData, 0, stringLength);
            return tempString;
        }
    }   // 256 byte char array 
    public struct RangeEntity
    {
        public char[] Name; // String32!  32 byte char array.
        public int Start;
        public int End;
    } // String32 Name, int Start, int End - complete
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3 ReadVector3(BinaryReader b)
        {
            this.x = b.ReadSingle();
            this.y = b.ReadSingle();
            this.z = b.ReadSingle();
            return this;
        }
    }  // Vector in 3D space {x,y,z}
    public struct Matrix33    // a 3x3 transformation matrix
    {
        public float m11;
        public float m12;
        public float m13;
        public float m21;
        public float m22;
        public float m23;
        public float m31;
        public float m32;
        public float m33;

        public bool Is_Identity()
        {
            if (System.Math.Abs(m11 - 1.0) > 0.00001) { return false; }
            if (System.Math.Abs(m12) > 0.00001) { return false; }
            if (System.Math.Abs(m13) > 0.00001) { return false; }
            if (System.Math.Abs(m21) > 0.00001) { return false; }
            if (System.Math.Abs(m22 - 1.0) > 0.00001) { return false; }
            if (System.Math.Abs(m23) > 0.00001) { return false; }
            if (System.Math.Abs(m31) > 0.00001) { return false; }
            if (System.Math.Abs(m32) > 0.00001) { return false; }
            if (System.Math.Abs(m33 - 1.0) > 0.00001) { return false; }
            return true;
        }  // returns true if this is an identy matrix
        public Matrix33 Get_Copy()    // returns a copy of the matrix33
        {
            Matrix33 mat = new Matrix33();
            mat.m11 = m11;
            mat.m12 = m12;
            mat.m13 = m13;
            mat.m21 = m21;
            mat.m22 = m22;
            mat.m23 = m23;
            mat.m31 = m31;
            mat.m32 = m32;
            mat.m33 = m33;
            return mat;
        }
        public float Get_Determinant()
        {
            return (m11 * m22 * m33
                + m12 * m23 * m31
                + m13 * m21 * m32
                - m31 * m22 * m13
                - m21 * m12 * m33
                - m11 * m32 * m23);
        }
        public Matrix33 Get_Transpose()    // returns a copy of the matrix33
        {
            Matrix33 mat = new Matrix33();
            mat.m11 = m11;
            mat.m12 = m21;
            mat.m13 = m31;
            mat.m21 = m12;
            mat.m22 = m22;
            mat.m23 = m32;
            mat.m31 = m13;
            mat.m32 = m23;
            mat.m33 = m33;
            return mat;
        }
        public Matrix33 Mult(Matrix33 mat)
        {
            Matrix33 mat2 = new Matrix33();
            mat2.m11 = this.m11 * mat.m11 + this.m12 * mat.m21 + this.m13 * mat.m31;
            mat2.m12 = this.m11 * mat.m12 + this.m12 * mat.m22 + this.m13 * mat.m32;
            mat2.m13 = this.m11 * mat.m13 + this.m12 * mat.m23 + this.m13 * mat.m33;
            mat2.m21 = this.m21 * mat.m11 + this.m22 * mat.m21 + this.m23 * mat.m31;
            mat2.m22 = this.m21 * mat.m12 + this.m22 * mat.m22 + this.m23 * mat.m32;
            mat2.m23 = this.m21 * mat.m13 + this.m22 * mat.m23 + this.m23 * mat.m33;
            mat2.m31 = this.m31 * mat.m11 + this.m32 * mat.m21 + this.m33 * mat.m31;
            mat2.m32 = this.m31 * mat.m12 + this.m32 * mat.m22 + this.m33 * mat.m32;
            mat2.m33 = this.m31 * mat.m13 + this.m32 * mat.m23 + this.m33 * mat.m33;
            return mat2;
        }
        public bool Is_Scale_Rotation() // Returns true if the matrix decomposes nicely into scale * rotation\
        {
            Matrix33 self_transpose,mat = new Matrix33();
            self_transpose = this.Get_Transpose();
            mat = this.Mult(self_transpose);
            if (System.Math.Abs(mat.m12) + System.Math.Abs(mat.m13)
                + System.Math.Abs(mat.m21) + System.Math.Abs(mat.m23)
                + System.Math.Abs(mat.m31) + System.Math.Abs(mat.m32) > 0.01) {
                    Console.WriteLine(" is a Scale_Rot matrix");
                    return false;
                }
            Console.WriteLine(" is not a Scale_Rot matrix"); 
            return true;
        }
        public Vector3 Get_Scale()
        {
            // Get the scale, assuming is_scale_rotation is true
            Matrix33 mat = this.Mult(this.Get_Transpose());
            Vector3 scale = new Vector3();
            scale.x = (float)System.Math.Pow(mat.m11, 0.5);
            scale.y = (float)System.Math.Pow(mat.m22, 0.5);
            scale.z = (float)System.Math.Pow(mat.m33, 0.5);
            if (this.Get_Determinant() < 0)
            {
                scale.x = 0 - scale.x;
                scale.y = 0 - scale.y;
                scale.z = 0 - scale.z;
                return scale;
            }
            else
            {
                return scale;
            }

        }
        public Vector3 Get_Scale_Rotation()   // Gets the scale.  this should also return the rotation matrix, but..eh...
        {
            Vector3 scale = this.Get_Scale();
            return scale;
        }
        public bool Is_Rotation()
        {
            // NOTE: 0.01 instead of CgfFormat.EPSILON to work around bad files
            if (!this.Is_Scale_Rotation()) { return false; }
            Vector3 scale = this.Get_Scale();
            if (System.Math.Abs(scale.x - 1.0) > 0.01 || System.Math.Abs(scale.y - 1.0) > 0.01 || System.Math.Abs(scale.z - 1.0) > 0.1)
            {
                return false;
            }
            return true;
        }
        public void Write_Matrix()
        {
            Console.WriteLine("=============================================");
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", m11, m12, m13);
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", m21, m22, m23);
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", m31, m32, m33);
        }
            
    }
    public struct Matrix44    // a 4x4 transformation matrix
    {
        public float m11;
        public float m12;
        public float m13;
        public float m14;
        public float m21;
        public float m22;
        public float m23;
        public float m24;
        public float m31;
        public float m32;
        public float m33;
        public float m34;
        public float m41;
        public float m42;
        public float m43;
        public float m44;
    }
    public struct Quat        // A quaternion (x,y,z,w)
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }
    public struct Vertex      // position p(Vector3) and normal n(Vector3)
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
        public uint FirstIndex;
        public uint NumIndices;
        public uint FirstVertex;
        public uint NumVertices;
        public uint MatID;
        public float Radius;
        public Vector3 Center;
    }  // Contains data about the parts of a mesh, such as vertices, radius and center.
    public struct Key
    {
        public int Time; // Time in ticks
        public Vector3 AbsPos; // absolute position
        public Vector3 RelPos; // relative position
        public Quat RelQuat; //Relative Quaternion if ARG==1?
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
    /*public struct TextureMap
    {

    }*/
    // Fill this in later.  line 369 in cgf.xml.
    public struct IRGB
    {
        public byte r; // red
        public byte g; // green
        public byte b; // blue
    }
    public struct IRGBA
    {
        public byte r; // red
        public byte g; // green
        public byte b; // blue
        public byte a; // alpha

    }
    public struct FRGB
    {
        public float r; // float Red
        public float g; // float green
        public float b; // float blue
    }
    public struct Tangent
    {
        // Tangents.  Divide each component by 32767 to get the actual value
        public short x;
        public short y;
        public short z;
        public short w;  // Handness?  Either 32767 (+1.0) or -32767 (-1.0)
    }
    public struct WORLDTOBONE
    {
        public float[,] worldToBone;   //  4x3 structure
        
        public WORLDTOBONE GetWorldToBone(BinaryReader b)
        {
            WORLDTOBONE tempW2B = new WORLDTOBONE();
            worldToBone = new float[4,3];
            for (int i = 0; i<4; i++) 
            {
                for (int j = 0; j < 3; j++)
                {
                    tempW2B.worldToBone[i, j] = b.ReadSingle();  // this might have to be switched to [j,i].  Who knows???
                }
            }
            return tempW2B;
        }
    }
    public struct BONETOWORLD
    {
        public float[,] boneToWorld;   //  4x3 structure

        public BONETOWORLD GetBoneToWorld(BinaryReader b)
        {
            WORLDTOBONE tempB2W = new WORLDTOBONE();
            boneToWorld = new float[4, 3];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    tempB2W.[i, j] = b.ReadSingle();  // this might have to be switched to [j,i].  Who knows???
                }
            }
            return tempB2W;
        }

    }

    public struct PhysicsGeometry
    {
        public UInt32 physicsGeom;
        public UInt32 flags;              // 0x0C ?
        public Vector3 min;
        public Vector3 max;
        public Vector3 spring_angle;
        public Vector3 spring_tension;
        public Vector3 damping;
        public Matrix33 framemtx;
        
        public PhysicsGeometry ReadPhysicsGeometry(BinaryReader b)      // Read a PhysicsGeometry structure
        {
            physicsGeom = b.ReadUInt32();
            flags = b.ReadUInt32();
            min.ReadVector3(b);
            max.ReadVector3(b);
            spring_angle.ReadVector3(b);
            spring_tension.ReadVector3(b);
            damping.ReadVector3(b);

            return this;
        }
    }
    public struct CompiledBone
    {
        public UInt32 controllerID;
        public PhysicsGeometry[] physicsGeometry; // 2 of these.
        public float mass;                  // 0xD8 ?
        public WORLDTOBONE worldToBone;     // 4x3 matrix
        public BONETOWORLD boneToWorld;     // 4x3 matrix
        public String boneName;             // String256 in old terms; convert to a real null terminated string.
        public UInt32 limbID;               // ID of this limb... usually just 0xFFFFFFFF
        public UInt32 offsetParent;         // offset to the parent in number of CompiledBone structs (584 bytes)
        public UInt32 numChildren;          // Number of children to this bone
        public UInt32 offsetChild;          // Offset to the first child to this bone in number of CompiledBone structs

        public CompiledBone GetCompiledBone(BinaryReader b)
        {
            controllerID = b.ReadUInt32();
            physicsGeometry = new PhysicsGeometry[2];
            physicsGeometry[0].ReadPhysicsGeometry(b);
            physicsGeometry[1].ReadPhysicsGeometry(b);
            mass = b.ReadSingle();

        }
    }

    public struct HitBox
    {
        public uint MatID;          // this is a guess for now
        public uint FirstIndex;
        public uint NumIndices;
        public uint FirstVertex;
        public uint NumVertices;
        public UInt32 Unknown1;     // unknown
        public UInt32 Unknown2;     // Unknown
        public Vector3[] Vertices;    // Array of vertices (x,y,z) length NumVertices
        public UInt16[] Indices;      // Array of indices
    }
    // Bone Structures courtesy of revelation
    public struct InitialPosMatrix
    {
        // A bone initial position matrix.
        Matrix33 Rot;              // type="Matrix33">
        Vector3 Pos;                // type="Vector3">
    }
    public struct BoneNameListChunk
    {
        //<version num="745">Far Cry, Crysis, Aion</version>
        UInt32 Num_Names;           //" type="uint" />
        UInt16[]  boneNames;        // uint 8?
    }
    public struct BoneInitialPosChunk
    {
        UInt32 Mesh;      // type="Ptr" template="MeshChunk">The mesh with bone info for which these bone initial positions are applicable. There might be some unused bones here as well. There must be the same number of bones as in the other chunks - they are placed in BoneId order.</add>
        UInt32 Num_Bones; // Number of bone initial positions.
        InitialPosMatrix[] Initial_Pos_Matrices; // type="InitialPosMatrix" arr1="Num Bones"
    }
    public struct BonePhysics           // 26 total words = 104 total bytes
    {
        UInt32 Geometry;                //" type="Ref" template="BoneMeshChunk">Geometry of a separate mesh for this bone.</add>
        //<!-- joint parameters -->
        UInt32 Flags;                   //" type="uint" />
        Vector3  Min;                   //" type="Vector3" />
        Vector3 Max;                   //" type="Vector3" />
        Vector3 Spring_Angle;          //" type="Vector3" />
        Vector3 Spring_Tension;        //" type="Vector3" />
        Vector3 Damping;               //" type="Vector3" />
        Matrix33  Frame_Matrix;        //" type="Matrix33" />
    }       // 
    public struct BoneEntity
    {
        UInt32 Bone_Id;                 //" type="int">Bone identifier.</add>
        UInt32 Parent_Id;               //" type="int">Parent identifier.</add>
        UInt32 Num_Children;            //" type="uint" />
        UInt32 Bone_Name_CRC32;         //" type="uint">CRC32 of bone name as listed in the BoneNameListChunk.  In Python this can be calculated using zlib.crc32(name)</add>
        char[]   Properties;            //" type="String32" />
        BonePhysics Physics;            //" type="BonePhysics" />
    }
    public struct BoneAnimChunk
    {
        //<version num="290">Far Cry, Crysis, Aion</version>
        UInt32 Num_Bones;               //" type="uint" />
        BoneEntity[] Bones;             //" type="BoneEntity" arr1="Num Bones" />
    }

}