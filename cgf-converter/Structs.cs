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
}