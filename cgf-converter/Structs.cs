using System;
using System.IO;

namespace CgfConverter
{
    public struct RangeEntity
    {
        public String Name; // String32!  32 byte char array.
        public Int32 Start;
        public Int32 End;
    } // String32 Name, int Start, int End - complete
    public struct Vector3
    {
        public Double x;
        public Double y;
        public Double z;
        public Double w; // Currently Unused
        public void ReadVector3(BinaryReader b)
        {
            this.x = b.ReadSingle();
            this.y = b.ReadSingle();
            this.z = b.ReadSingle();
            return;
        }
        public Vector3 Add(Vector3 vector)
        {
            Vector3 result = new Vector3();
            result.x = vector.x + x;
            result.y = vector.y + y;
            result.z = vector.z + z;
            return result;
        }
        public Vector4 ToVector4()
        {
            Vector4 result = new Vector4();
            result.x = x;
            result.y = y;
            result.z = z;
            result.w = 1;
            return result;
        }
        public void WriteVector3()
        {
            Console.WriteLine("*** WriteVector3");
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", x, y, z);
            Console.WriteLine();
        }

    }  // Vector in 3D space {x,y,z}
    public struct Vector4
    {
        public Double x;
        public Double y;
        public Double z;
        public Double w;

        public Vector3 ToVector3()
        {
            Vector3 result = new Vector3();
            if (w == 0)
            {
                result.x = x;
                result.y = y;
                result.z = z;
            } else
            {
                result.x = x / w;
                result.y = y / w;
                result.z = z / w;
            }
            return result;
        }
        public void WriteVector4()
        {
            Console.WriteLine("=============================================");
            Console.WriteLine("x:{0:F7}  y:{1:F7}  z:{2:F7} w:{3:F7}", x, y, z, w);
        }
    }
    public struct Matrix33    // a 3x3 transformation matrix
    {
        public Double m11;
        public Double m12;
        public Double m13;
        public Double m21;
        public Double m22;
        public Double m23;
        public Double m31;
        public Double m32;
        public Double m33;

        public void ReadMatrix33(BinaryReader b)
        {
            // Reads a Matrix33 structure
            m11 = b.ReadSingle();
            m12 = b.ReadSingle();
            m13 = b.ReadSingle();
            m21 = b.ReadSingle();
            m22 = b.ReadSingle();
            m23 = b.ReadSingle();
            m31 = b.ReadSingle();
            m32 = b.ReadSingle();
            m33 = b.ReadSingle();
        }
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
        public Double Get_Determinant()
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
            mat2.m11 = (m11 * mat.m11) + (m12 * mat.m21) + (m13 * mat.m31);
            mat2.m12 = (m11 * mat.m12) + (m12 * mat.m22) + (m13 * mat.m32);
            mat2.m13 = (m11 * mat.m13) + (m12 * mat.m23) + (m13 * mat.m33);
            mat2.m21 = (m21 * mat.m11) + (m22 * mat.m21) + (m23 * mat.m31);
            mat2.m22 = (m21 * mat.m12) + (m22 * mat.m22) + (m23 * mat.m32);
            mat2.m23 = (m21 * mat.m13) + (m22 * mat.m23) + (m23 * mat.m33);
            mat2.m31 = (m31 * mat.m11) + (m32 * mat.m21) + (m33 * mat.m31);
            mat2.m32 = (m31 * mat.m12) + (m32 * mat.m22) + (m33 * mat.m32);
            mat2.m33 = (m31 * mat.m13) + (m32 * mat.m23) + (m33 * mat.m33);
            return mat2;
        }
        public Vector3 Mult3x1(Vector3 vector)
        {
            // Multiply the 3x3 matrix by a Vector 3 to get the rotation
            Vector3 result = new Vector3();
            //result.x = (vector.x * m11) + (vector.y * m12) + (vector.z * m13);
            //result.y = (vector.x * m21) + (vector.y * m22) + (vector.z * m23);
            //result.z = (vector.x * m31) + (vector.y * m32) + (vector.z * m33);
            result.x = (vector.x * m11) + (vector.y * m21) + (vector.z * m31);
            result.y = (vector.x * m12) + (vector.y * m22) + (vector.z * m32);
            result.z = (vector.x * m13) + (vector.y * m23) + (vector.z * m33);
            return result;
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
            scale.x = (Double)System.Math.Pow(mat.m11, 0.5);
            scale.y = (Double)System.Math.Pow(mat.m22, 0.5);
            scale.z = (Double)System.Math.Pow(mat.m33, 0.5);
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
        public void WriteMatrix33()
        {
            Console.WriteLine("=============================================");
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", m11, m12, m13);
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", m21, m22, m23);
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", m31, m32, m33);
        }
            
    }
    public struct Matrix44    // a 4x4 transformation matrix.  first value is row, second is column
    {
        public Double m11;
        public Double m12;
        public Double m13;
        public Double m14;
        public Double m21;
        public Double m22;
        public Double m23;
        public Double m24;
        public Double m31;
        public Double m32;
        public Double m33;
        public Double m34;
        public Double m41;
        public Double m42;
        public Double m43;
        public Double m44;

        public Vector4 Mult4x1(Vector4 vector)
        {
            // Pass the matrix a Vector4 (4x1) vector to get the transform of the vector
            Vector4 result = new Vector4();
            // result.x = (m11 * vector.x) + (m12 * vector.y) + (m13 * vector.z) + m14 / 100;
            // result.y = (m21 * vector.x) + (m22 * vector.y) + (m23 * vector.z) + m24 / 100;
            // result.z = (m31 * vector.x) + (m32 * vector.y) + (m33 * vector.z) + m34 / 100;
            // result.w = (m41 * vector.x) + (m42 * vector.y) + (m43 * vector.z) + m44 / 100;
            result.x = (m11 * vector.x) + (m21 * vector.y) + (m31 * vector.z) + m41 / 100;
            result.y = (m12 * vector.x) + (m22 * vector.y) + (m32 * vector.z) + m42 / 100;
            result.z = (m13 * vector.x) + (m23 * vector.y) + (m33 * vector.z) + m43 / 100;
            result.w = (m14 * vector.x) + (m24 * vector.y) + (m34 * vector.z) + m44 / 100;

            return result;
        }
        public Matrix33 To3x3()
        {
            Matrix33 result = new Matrix33();
            result.m11 = m11;
            result.m12 = m12;
            result.m13 = m13;
            result.m21 = m21;
            result.m22 = m22;
            result.m23 = m23;
            result.m31 = m31;
            result.m32 = m32;
            result.m33 = m33;
            return result;
        }
        public Vector3 GetTranslation()
        {
            Vector3 result = new Vector3();
            result.x = m41 / 100;
            result.y = m42 / 100;
            result.z = m43 / 100;
            return result;
        }
        public void WriteMatrix44()
        {
            Console.WriteLine("=============================================");
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}  {3:F7}", m11, m12, m13, m14);
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}  {3:F7}", m21, m22, m23, m24);
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}  {3:F7}", m31, m32, m33, m34);
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}  {3:F7}", m41, m42, m43, m44);
            Console.WriteLine();
        }

    }

    public struct Quat        // A quaternion (x,y,z,w)
    {
        public Double x;
        public Double y;
        public Double z;
        public Double w;
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
        public Double Radius;
        public Vector3 Center;

        public void WriteMeshSubset()
        {
            Console.WriteLine("*** Mesh Subset ***");
            Console.WriteLine("    First Index:  {0}", FirstIndex);
            Console.WriteLine("    Num Indices:  {0}", NumIndices);
            Console.WriteLine("    First Vertex: {0}", FirstVertex);
            Console.WriteLine("    Num Vertices: {0}", NumVertices);
            Console.WriteLine("    Mat ID:       {0}", MatID);
            Console.WriteLine("    Radius:       {0:F7}", Radius);
            Console.WriteLine("    Center:");
            Center.WriteVector3();
        }
    }  // Contains data about the parts of a mesh, such as vertices, radius and center.
    public struct Key
    {
        public int Time; // Time in ticks
        public Vector3 AbsPos; // absolute position
        public Vector3 RelPos; // relative position
        public Quat RelQuat; //Relative Quaternion if ARG==1?
        public Vector3 Unknown1; // If ARG==6 or 10?
        public Double[] Unknown2; // If ARG==9?  array length = 2
    }
    public struct UV
    {
        public Double U;
        public Double V;
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
        public Double r; // Double Red
        public Double g; // Double green
        public Double b; // Double blue
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
        public Double[,] worldToBone;   //  4x3 structure
        
        public void GetWorldToBone(BinaryReader b)
        {
            worldToBone = new Double[4,3];
            //Console.WriteLine("GetWorldToBone {0:X}", b.BaseStream.Position);
            for (int i = 0; i<4; i++) 
            {
                for (int j = 0; j < 3; j++)
                {
                    worldToBone[i, j] = b.ReadSingle();  // this might have to be switched to [j,i].  Who knows???
                    //tempW2B.worldToBone[i, j] = b.ReadSingle();  // this might have to be switched to [j,i].  Who knows???
                    //Console.WriteLine("worldToBone: {0:F7}", worldToBone[i, j]);
                }
            }
            return;
        }
        public void WriteWorldToBone()
        {
            //Console.WriteLine();
            //Console.WriteLine("     *** World to Bone ***");
            Console.WriteLine("     {0:F7}  {1:F7}  {2:F7}", this.worldToBone[0, 0], this.worldToBone[0, 1], this.worldToBone[0, 2]);
            Console.WriteLine("     {0:F7}  {1:F7}  {2:F7}", this.worldToBone[1, 0], this.worldToBone[1, 1], this.worldToBone[1, 2]);
            Console.WriteLine("     {0:F7}  {1:F7}  {2:F7}", this.worldToBone[2, 0], this.worldToBone[2, 1], this.worldToBone[2, 2]);
            Console.WriteLine("     {0:F7}  {1:F7}  {2:F7}", this.worldToBone[3, 0], this.worldToBone[3, 1], this.worldToBone[3, 2]);
            //Console.WriteLine();
        }
    }
    public struct BONETOWORLD
    {
        public Double[,] boneToWorld;   //  4x3 structure

        public void GetBoneToWorld(BinaryReader b)
        {
            //BONETOWORLD tempB2W = new BONETOWORLD();
            boneToWorld = new Double[4, 3];
            //Console.WriteLine("GetBoneToWorld");
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    boneToWorld[i, j] = b.ReadSingle();  // this might have to be switched to [j,i].  Who knows???
                    //Console.WriteLine("boneToWorld: {0:F7}", boneToWorld[i, j]);
                }
            }
            return;
        }
        public void WriteBoneToWorld()
        {
            Console.WriteLine();
            Console.WriteLine("*** Bone to World ***");
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", this.boneToWorld[0, 0], this.boneToWorld[0, 1], this.boneToWorld[0, 2]);
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", this.boneToWorld[1, 0], this.boneToWorld[1, 1], this.boneToWorld[1, 2]);
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", this.boneToWorld[2, 0], this.boneToWorld[2, 1], this.boneToWorld[2, 2]);
            Console.WriteLine("{0:F7}  {1:F7}  {2:F7}", this.boneToWorld[3, 0], this.boneToWorld[3, 1], this.boneToWorld[3, 2]);
            Console.WriteLine();
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
        
        public void ReadPhysicsGeometry(BinaryReader b)      // Read a PhysicsGeometry structure
        {
            physicsGeom = b.ReadUInt32();
            flags = b.ReadUInt32();
            min.ReadVector3(b);
            // min.WriteVector3();
            max.ReadVector3(b);
            // max.WriteVector3();
            spring_angle.ReadVector3(b);
            spring_tension.ReadVector3(b);
            damping.ReadVector3(b);
            framemtx.ReadMatrix33(b);
            return;
        }
        public void WritePhysicsGeometry()
        {
            Console.WriteLine("WritePhysicsGeometry");
        }
        
    }
    public struct CompiledBone
    {
        public UInt32 controllerID;
        public PhysicsGeometry[] physicsGeometry; // 2 of these.
        public Double mass;                  // 0xD8 ?
        public WORLDTOBONE worldToBone;     // 4x3 matrix
        public BONETOWORLD boneToWorld;     // 4x3 matrix
        public String boneName;             // String256 in old terms; convert to a real null terminated string.
        public UInt32 limbID;               // ID of this limb... usually just 0xFFFFFFFF
        public Int32  offsetParent;         // offset to the parent in number of CompiledBone structs (584 bytes)
        public Int32 offsetChild;           // Offset to the first child to this bone in number of CompiledBone structs
        public UInt32 numChildren;          // Number of children to this bone
        public String parentID;             // Not part of the read structure, but the name of the parent bone put into the Bone Dictionary (the key)
        public Int64 offset;                 // Not part of the structure, but where this one started.
        public String[] childNames;         // Not part of read struct.  Contains the keys of the children to this bone

        public void ReadCompiledBone(BinaryReader b)
        {
            // Reads just a single 584 byte entry of a bone. At the end the seek position will be advanced, so keep that in mind.
            controllerID = b.ReadUInt32();
            physicsGeometry = new PhysicsGeometry[2];
            physicsGeometry[0].ReadPhysicsGeometry(b);
            physicsGeometry[1].ReadPhysicsGeometry(b);
            mass = b.ReadSingle();
            worldToBone = new WORLDTOBONE();
            worldToBone.GetWorldToBone(b);
            boneToWorld = new BONETOWORLD();
            boneToWorld.GetBoneToWorld(b);
            boneName = b.ReadFString(256);
            limbID = b.ReadUInt32();
            offsetParent = b.ReadInt32();
            numChildren = b.ReadUInt32();
            offsetChild = b.ReadInt32();
            childNames = new String[numChildren];
        }
        public void WriteCompiledBone()
        {
            // Output the bone to the console
            Console.WriteLine();
            Console.WriteLine("*** Compiled bone {0}", boneName);
            Console.WriteLine("    Parent Name: {0}", parentID);
            Console.WriteLine("    Offset in file: {0:X}", offset);
            Console.WriteLine("    Controller ID: {0}", controllerID);
            Console.WriteLine("    World To Bone:");
            worldToBone.WriteWorldToBone();
            //Console.WriteLine("    Bone To World:");
            //boneToWorld.WriteBoneToWorld();
            Console.WriteLine("    Limb ID: {0}", limbID);
            Console.WriteLine("    Parent Offset: {0}", offsetParent);
            Console.WriteLine("    Child Offset:  {0}", offsetChild);
            Console.WriteLine("    Number of Children:  {0}", numChildren);
            Console.WriteLine("*** End Bone {0}", boneName);
        }
    }

    public struct HitBox
    {
        public uint ID;             // Chunk ID (although not technically a chunk
        public uint FirstIndex;
        public uint NumIndices;
        public uint FirstVertex;
        public uint NumVertices;
        public UInt32 Unknown1;     // unknown
        public UInt32 Unknown2;     // Size of the weird data at the end of the hitbox structure.
        public Vector3[] Vertices;    // Array of vertices (x,y,z) length NumVertices
        public UInt16[] Indices;      // Array of indices

        public void WriteHitBox()
        {
            Console.WriteLine("     ** Hitbox **");
            Console.WriteLine("        ID: {0:X}", ID);
            Console.WriteLine("        Num Vertices: {0:X}", NumVertices);
            Console.WriteLine("        Num Indices:  {0:X}", NumIndices);
            Console.WriteLine("        Unknown2: {0:X}", Unknown2);
        }
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