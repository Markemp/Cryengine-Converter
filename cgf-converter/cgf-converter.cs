using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter
{
    public class CgfFormat // Stores all information about the cgf file format.
    {
        public string CgfFile; //the name of the file we are reading
        public Header CgfHeader;
        public ChunkTable CgfChunkTable;
        public List<Chunk> CgfChunks;

        public CgfFormat(string cgffile)  // the entire cgfformat in one handy class
        {
            CgfFile = cgffile;
            CgfHeader = new Header();
            CgfChunkTable = new ChunkTable();
            List<Chunk> CgfChunks = new List<Chunk>();

            // Need to implement the chunks here.  A list of the Chunk class.
        }
        public void GetData() // Read the full cgf file and put it into
        {
            //CgfFormat datafile = new CgfFormat(cgfData); // datafile will be where we store all the components
            using (BinaryReader cgfreader = new BinaryReader(File.Open(CgfFile, FileMode.Open)))
            {
                CgfHeader = CgfHeader.GetHeader(cgfreader);
                int offset = CgfHeader.fileOffset;  // location of the Chunk table.
                cgfreader.BaseStream.Seek(offset, 0);  // will now start to read from the start of the chunk table
                //Console.WriteLine("Current offset is {0:X}", cgfreader.BaseStream.Position);    // for testing
                //Console.ReadKey();                                                              // for testing
                CgfChunkTable = CgfChunkTable.GetChunkTable(cgfreader, offset);
            }
            return;
        }
    }

    // Structures
    public struct String16
    {
        public fixed char Data[16];
    }   // 16 byte char array 
    public struct String32
    {
        public fixed char Data[32];
    }    // 32 byte char array 
    public struct String64
    {
        public fixed char Data[64];
    }  // 64 byte char array 
    public struct String128
    {
        public fixed char Data[128];
    }   // 128 byte char array 
    public struct String256
    {
        public fixed char Data[256];
    }   // 256 byte char array 
    public struct RangeEntity
    {
        public String32 Name;
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

    // Aliases
    public class FileOffset
    { 
        public int fOffset;
    }

    // Enums
    public enum FileType : uint
    {
        GEOM = 0xFFFF0000,
        ANIM = 0xFFFF0001
    }  // complete
    public enum ChunkType : uint    // complete
    {
        Any = 0x0,
        Mesh = 0xCCCC0000,
        Helper = 0xCCCC0001,
        VertAnim = 0xCCCC0002,
        BoneAnim = 0xCCCC0003,
        GeomNameList = 0xCCCC0004,
        BoneNameList = 0xCCCC0005,
        MtlList = 0xCCCC0006,
        MRM = 0xCCCC0007, //obsolete
        SceneProps = 0xCCCC0008,
        Light = 0xCCCC0009,
        PatchMesh = 0xCCCC000A,
        Node = 0xCCCC000B,
        Mtl = 0xCCCC000C,
        Controller = 0xCCCC000D,
        Timing = 0xCCCC000E,
        BoneMesh = 0xCCCC000F,
        BoneLightBinding = 0xCCCC0010,
        MeshMorphTarget = 0xCCCC0011,
        BoneInitialPos = 0xCCCC0012,
        SourceInfo = 0xCCCC0013, //Describes the source from which the cgf was exported: source max file, machine and user.
        MtlName = 0xCCCC0014, //provides material name as used in the material.xml file
        ExportFlags = 0xCCCC0015, //Describes export information.
        DataStream = 0xCCCC0016, //A data Stream
        MeshSubsets = 0xCCCC0017, //Describes an array of mesh subsets
        MeshPhysicalData = 0xCCCC0018, //Physicalized mesh data
        CompiledBones = 0xACDC0000, //unknown chunk
        CompiledPhysicalBones = 0xACDC0001, // unknown chunk
        CompiledMorphtargets = 0xACDC0002,  // unknown chunk
        CompiledPhysicalProxies = 0xACDC0003, //unknown chunk
        CompiledIntFaces = 0xACDC0004, //unknown chunk
        CompiledIntSkinVertices = 0xACDC0004, //unknown chunk
        CompiledExt2IntMap = 0xACDC0005, //unknown chunk
        BreakablePhysics = 0xACDC0006, //unknown chunk
        FaceMap = 0xAAFC0000, //unknown chunk
        SpeedInfo = 0xAAFC0002, //Speed and distnace info
        FootPlantInfo = 0xAAFC0003, // Footplant info
        BonesBoxes = 0xAAFC0004, // unknown chunk
        UnknownAAFC0005 = 0xAAFC0005 //unknown chunk
    }
    public enum ChunkVersion : uint
    {
        ChkVersion
    }    //complete
    public enum HelperType : uint
    {
        POINT,
        DUMMY,
        XREF,
        CAMERA,
        GEOMETRY
    }      //complete
    public enum TextureMapping : uint
    {
        NORMAL,
        ENVIRONMENT,
        SCREENENVIRONMENT,
        CUBIC,
        AUTOCUBIC
    }  //complete
    public enum MtlType : uint            //complete
    {
        UNKNOWN,
        STANDARD,
        MULTI,
        TWOSIDED
    }
    public enum MtlNamePhysicsType : uint //complete
    {
        NONE = 0xFFFFFFFF,
        DEFAULT = 0x00000000,
        NOCOLLIDE = 0x00000001,
        OBSTRUCT = 0x00000002,
        DEFAULTPROXY = 0x000000FF  // this needs to be checked.  cgf.xml says 256; not sure if hex or dec
    }
    public enum LightType : uint         //complete
    {
        OMNI,
        SPOT,
        DIRECT,
        AMBIENT
    }
    public enum CtrlType : uint
    {
        NONE,
        CRYBONE,
        LINEAR1,
        LINEAR3,
        LINEARQ,
        BEZIER1,
        BEZIER3,
        BEZIERQ,
        TBC1,
        TBC3,
        TBCQ,
        BSPLINE2O,
        BSPLINE1O,
        BSPLINE2C,
        BSPLINE1C,
        CONST          // this was given a value of 11, which is the same as BSPLINE2o.
    }        //complete
    public enum DataStreamType : uint
    {
        VERTICES,
        NORMALS,
        UVS,
        COLORS,
        COLORS2,
        INDICES,
        TANGENTS,
        SHCOEFFS,
        SHAPEDEFORMATION,
        BONEMAP,
        FACEMAP,
        VERTMATS
    }  //complete
    public enum PhysicsPrimitiveType : uint
    {
        CUBE = 0X0,
        POLYHEDRON = 0X1,
        CYLINDER = 0X5,
        UNKNOWN6 = 0X6   // nothing between 2-4, no idea what unknown is.
    }

    // classes (aka anything more complicated than a fixed size struct, with methods etc.)
    public class Header
    {
        public char[] fileSignature; // The CGF file signature.  CryTek for 3.5, CrChF for 3.6
        public uint fileType; // The CGF file type (geometry or animation)  3.5 only
        public uint chunkVersion; // The version of the chunk table  3.5 only
        public int fileOffset; //Position of the chunk table in the CGF file
        public int numChunks; // Number of chunks in the Chunk Table (3.6 only.  3.5 has it in Chunk Table)
        
        // methods
        public Header GetHeader(BinaryReader binReader)
        {
            Header cgfHeader = new Header();
            // populate the Header objects
            cgfHeader.fileSignature = binReader.ReadChars(8);
            cgfHeader.fileType = binReader.ReadUInt32();
            cgfHeader.chunkVersion = binReader.ReadUInt32();
            cgfHeader.fileOffset = binReader.ReadInt32();
            cgfHeader.WriteHeader(cgfHeader);  // For testing.  This will write out what we find.
            
            return cgfHeader;
        }
        public void WriteHeader(Header cgfHdr)  // output header to console for testing
        {
            Console.WriteLine("Header Filesignature: {0}", fileSignature[0]);
            Console.WriteLine("Header FileType: {0:X}", fileType);
            Console.WriteLine("Header ChunkVersion: {0:X}", chunkVersion);
            Console.WriteLine("Header ChunkTableOffset: {0:X}", fileOffset);

            // Console.ReadKey();

            return;
        }
    }
    // comment
    public class ChunkHeader
    {
        public ChunkType type;
        public ChunkVersion version;
        public uint offset;
        public uint id;
        public uint unknown; //  there are 2 uints(?) at the end of each chunk header.  Not sure what ID and unknown refer to.

        // methods        
        public void GetChunkHeader(BinaryReader binReader)
        {
            // Populate the ChunkHeader objects
            Console.WriteLine("Current offset is {0:X}", binReader.BaseStream.Position);    // for testing
            uint headerType = binReader.ReadUInt32(); // read the value, then parse it
            Console.WriteLine("headerType: '{0:X}'", headerType);
            type = (ChunkType)Enum.ToObject(typeof(ChunkType), headerType);
            uint chunkversionType = binReader.ReadUInt32();
            version = (ChunkVersion)Enum.ToObject(typeof(ChunkVersion), chunkversionType);
            offset = binReader.ReadUInt32();
            id = binReader.ReadUInt32();
            unknown = binReader.ReadUInt32();
            Console.WriteLine("Current offset is {0:X}", binReader.BaseStream.Position);
            //Console.ReadKey();
            WriteChunkHeader(); // For testing.  remove.
            return;
        }
        public void WriteChunkHeader()  // write the Chunk Header Table to the console.  For testing.
        {
            Console.WriteLine("ChunkType: {0}", type);
            Console.WriteLine("ChunkVersion: {0:X}", version);
            Console.WriteLine("offset: {0:X}", offset);
            Console.WriteLine("ID: {0:X}", id);
            //Console.ReadKey();
        }
    }
    public class ChunkTable  // reads the chunk table
    {
        public uint numChunks;
        public List<ChunkHeader> chunkHeaders = new List<ChunkHeader>();

        // methods
        public ChunkTable GetChunkTable (BinaryReader binReader, int foffset)
        {
            ChunkTable chkTbl = new ChunkTable();
            
            // need to seek to the start of the table here.  foffset points to the start of the table
            chkTbl.numChunks = binReader.ReadUInt32();  // number of Chunks in the table.
            int i; // counter for loop to read all the chunkHeaders
            for (i = 0; i < chkTbl.numChunks; i++ )
            {
                Console.WriteLine("Loop {0}", i);
                ChunkHeader tempChkHdr = new ChunkHeader(); // Add this chunk header to the list
                tempChkHdr.GetChunkHeader(binReader); // Now has the next chunkheader
                chunkHeaders.Add(tempChkHdr);
            }
            return chkTbl;
        }
    }

    public class Chunk
    {
        // don't need anything here
    }

    public class ChuckTimingFormat : Chunk
    {
        public float SecsPerTick;
        public int TicksPerFrame;
        public RangeEntity GlobalRange;
        public int NumSubRanges;

        public void ChunkTimingFormat()
        {

        }
    }

    class Program
    {
        /*static void ReadCryHeader(string cgfFile)
        {
            //TBD
        }
        static void ReadChunkTable(string cgfFile)
        {
            // TBD
        }*/  // These 2 functions not needed.  To be implemented in classes

        static void Main(string[] args)
        {
            // Assign the argument to a variable
            int lengthofArgs = args.Length;
            string cgfFile;

            if (args.Length != 0)
            {
                // Console.WriteLine("Args length not 0");
                cgfFile = String.Copy(args[0]);
            }
            else
            {
                Console.WriteLine("Please input cgf/cga file.");
                cgfFile = Console.ReadLine();
            }
            
            Console.WriteLine("Input File is '{0}'" , cgfFile);
            //ReadCryHeader(cgfFile);
            CgfFormat cgfData = new CgfFormat(cgfFile);
            cgfData.GetData();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(); // Press any key to continue
            
            return;
        }
    }
}
