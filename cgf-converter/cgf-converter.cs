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
        public List<Chunk> CgfChunks = new List<Chunk>();

        public CgfFormat(string cgffile)  // Constructor for CgfFormat.
        {
            CgfFile = cgffile;
            using (BinaryReader cgfreader = new BinaryReader(File.Open(CgfFile, FileMode.Open)))
            {
                CgfHeader = new Header(cgfreader);// Gets the header of the file (3-5 objects dep on version)
                int offset = CgfHeader.fileOffset;  // location of the Chunk table.
                cgfreader.BaseStream.Seek(offset, 0);  // will now start to read from the start of the chunk table
                //Console.WriteLine("Current offset is {0:X}", cgfreader.BaseStream.Position);    // for testing
                //Console.ReadKey();     
                CgfChunkTable = new ChunkTable(cgfreader, offset);

                foreach (ChunkHeader ChkHdr in CgfChunkTable.chunkHeaders) 
                {
                    ChunkType chkType = ChkHdr.type;
                    Console.WriteLine("Processing {0}", chkType);
                    switch (ChkHdr.type)
                    {
                        case ChunkType.SourceInfo:
                        {
                            ChunkSourceInfo chkSrcInfo = new ChunkSourceInfo();
                            chkSrcInfo.GetChunkSourceInfo(cgfreader,ChkHdr.offset);
                            CgfChunks.Add(chkSrcInfo);
                            Console.WriteLine("Source info chunk here");
                            chkSrcInfo.WriteChunkSourceInfo();
                            break;
                        }
                        case ChunkType.Timing:
                        {
                            Console.WriteLine("Timing Chunk here");
                            break;
                        }
                        case ChunkType.Mtl:
                        {
                            Console.WriteLine("Mtl Chunk here");
                            break;
                        }
                        default:
                        {
                            Console.WriteLine("Chunk type found that didn't match known versions");
                            break;
                        }
                    }
                }

            }
                
            return;
        }
}

    // Structures
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
        public  char[] Data;
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
        public Header (BinaryReader binReader)  //constructor with 1 arg
        {
            //Header cgfHeader = new Header();
            // populate the Header objects
            fileSignature = new char[8];
            fileSignature = binReader.ReadChars(8);
            fileType = binReader.ReadUInt32();
            chunkVersion = binReader.ReadUInt32();
            fileOffset = binReader.ReadInt32();
            WriteHeader();  // For testing.  This will write out what we find.
            
            return;
        }
        public void WriteHeader()  // output header to console for testing
        {
            string tmpFileSig;
            tmpFileSig = new string(fileSignature);
            Console.WriteLine("Header Filesignature: {0}", tmpFileSig);
            Console.WriteLine("Header FileType: {0:X}", fileType);
            Console.WriteLine("Header ChunkVersion: {0:X}", chunkVersion);
            Console.WriteLine("Header ChunkTableOffset: {0:X}", fileOffset);
            return;
        }
    }
    public class ChunkHeader  
    {
        public ChunkType type;
        public ChunkVersion version;
        public uint offset;
        public uint id;
        public uint unknown; //  there are 2 uints(?) at the end of each chunk header.  Not sure what ID and unknown refer to.

        // methods
        public ChunkHeader()
        {
            type = new ChunkType();
            version = new ChunkVersion();
            offset = new uint();
            id = new uint();
            unknown = new uint();
        }
        public void WriteChunkHeader()  // write the Chunk Header Table to the console.  For testing.
        {
            Console.Write("ChunkType: {0}", type);
            Console.Write("ChunkVersion: {0:X}", version);
            Console.Write("offset: {0:X}", offset);
            Console.WriteLine("ID: {0:X}", id);
            //Console.ReadKey();
        }
    }
    public class ChunkTable  // reads the chunk table
    {
        public uint numChunks;
        public List<ChunkHeader> chunkHeaders = new List<ChunkHeader>();

        // methods
        public ChunkTable (BinaryReader binReader, int foffset)
        {
            // need to seek to the start of the table here.  foffset points to the start of the table
            binReader.BaseStream.Seek(foffset, 0);
            numChunks = binReader.ReadUInt32();  // number of Chunks in the table.
            int i; // counter for loop to read all the chunkHeaders
            for (i = 0; i < numChunks; i++ )
            {
                //Console.WriteLine("Loop {0}", i);
                ChunkHeader tempChkHdr = new ChunkHeader(); // Add this chunk header to the list
                uint headerType = binReader.ReadUInt32(); // read the value, then parse it
                tempChkHdr.type = (ChunkType)Enum.ToObject(typeof(ChunkType), headerType);
                //Console.WriteLine("headerType: '{0}'", tempChkHdr.type);
                uint chunkversionType = binReader.ReadUInt32();
                tempChkHdr.version = (ChunkVersion)Enum.ToObject(typeof(ChunkVersion), chunkversionType);
                tempChkHdr.offset = binReader.ReadUInt32();
                tempChkHdr.id = binReader.ReadUInt32();
                tempChkHdr.unknown = binReader.ReadUInt32();

                chunkHeaders.Add(tempChkHdr);
            }
        }
        public void WriteChunkTable(ChunkTable writeme)
        {
            foreach (ChunkHeader chkHdr in writeme.chunkHeaders)
            {
                chkHdr.WriteChunkHeader();
            }
        }
    }
    public class Chunk // Main class has Fileoffset to identify where the chunk starts
    {
        FileOffset fOffset;
    }

    public class ChuckTimingFormat : Chunk
    {
        public ChunkType ChunkTiming;   //
        public float SecsPerTick;
        public int TicksPerFrame;
        public RangeEntity GlobalRange;
        public int NumSubRanges;

        public void ChunkTimingFormat(BinaryReader b, int foffset)
        {
            b.BaseStream.Seek(foffset, 0); // seek to the beginning of the Timing Format chunk
            uint tmpChkType = b.ReadUInt32();
            ChunkTiming = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChkType);
            //SecsPerTick = b.read

        }
    }
    public class ChunkSourceInfo : Chunk
    {
        public string SourceFile;
        public string Date;
        public string Author;

        public void GetChunkSourceInfo(BinaryReader b, uint f)  //
        {
            b.BaseStream.Seek(f, 0);
            // you'd think ReadString() would read from the current offset to the next null byte, but IT DOESN'T.
            int count = 0;                      // read original file
            while (b.ReadChar() != 0) 
            {
                count++;
            } // count now has the null position relative to the seek position
            b.BaseStream.Seek(f, 0);
            char[] tmpSource = new char[count];
            tmpSource = b.ReadChars(count+1);
            SourceFile = new string(tmpSource);

            count = 0;                          // Read date
            while (b.ReadChar() != 0)
            {
                count++;
            } // count now has the null position relative to the seek position
            b.BaseStream.Seek(b.BaseStream.Position - count - 1, 0);
            char[] tmpDate = new char[count];
            tmpDate = b.ReadChars(count+1);
            Date = new string(tmpDate);

            count = 0;                           // Read Author
            while (b.ReadChar() != 0)
            {
                count++;
            } // count now has the null position relative to the seek position
            b.BaseStream.Seek(b.BaseStream.Position - count - 1, 0);
            char[] tmpAuthor = new char[count];
            tmpAuthor = b.ReadChars(count+1);
            Author = new string(tmpAuthor);
        }

        public void WriteChunkSourceInfo()
        {
            Console.WriteLine("***SOURCE INFO CHUNK***");
            Console.WriteLine("Sourcefile: {0}.  Length {1}", SourceFile, SourceFile.Length);
            Console.WriteLine("Date:       {0}.  Length {1}", Date, Date.Length);
            Console.WriteLine("Author:     {0}.  Length {1}", Author, Author.Length);
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
            
            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey(); // Press any key to continue
            
            return;
        }
    }
}
