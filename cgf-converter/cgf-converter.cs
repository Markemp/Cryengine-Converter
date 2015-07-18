using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter
{
    public class CgfData // Stores all information about the cgf file format.
    {
        // public string CgfFile; //the name of the file we are reading.  Removed to use dataFile, a File object
        // Header, ChunkTable and Chunks are what are in a file.  1 header, 1 table, and a chunk for each entry in the table.
        public FileInfo dataFile;
        public FileInfo objOutputFile;  // want this to be the datafile minus the extension and .obj
        public Header CgfHeader;
        public ChunkTable CgfChunkTable;
        public List<Chunk> CgfChunks = new List<Chunk>();

        public CgfData(FileInfo dataFile)  // Constructor for CgfFormat.  This populates the structure
        {
            // CgfFile = cgffile;  // Don't use string, use File Obj
            //using (BinaryReader cgfreader = new BinaryReader(File.Open(dataFile.ToString(), FileMode.Open)))
            BinaryReader cgfreader = new BinaryReader(File.Open(dataFile.ToString(), FileMode.Open));
            CgfHeader = new Header(cgfreader);// Gets the header of the file (3-5 objects dep on version)
            CgfHeader.WriteChunk();
            int offset = CgfHeader.fileOffset;  // location of the Chunk table.
            cgfreader.BaseStream.Seek(offset, 0);  // will now start to read from the start of the chunk table
            CgfChunkTable = new ChunkTable();
            CgfChunkTable.GetChunkTable(cgfreader, offset);

            foreach (ChunkHeader ChkHdr in CgfChunkTable.chunkHeaders) 
            {
                ChunkType chkType = ChkHdr.type;
                //Console.WriteLine("Processing {0}", chkType);
                switch (ChkHdr.type)
                {
                    case ChunkType.SourceInfo:
                    {
                        ChunkSourceInfo chkSrcInfo = new ChunkSourceInfo();
                        chkSrcInfo.GetChunk(cgfreader,ChkHdr.offset);
                        CgfChunks.Add(chkSrcInfo);
                        //chkSrcInfo.WriteChunk(); 
                        break;
                    }
                    case ChunkType.Timing:
                    {
                        ChunkTimingFormat chkTiming = new ChunkTimingFormat();
                        chkTiming.GetChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkTiming);
                        //chkTiming.WriteChunk();
                        break;
                    }
                    case ChunkType.ExportFlags:
                    {
                        ChunkExportFlags chkExportFlag = new ChunkExportFlags();
                        chkExportFlag.GetChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkExportFlag);
                        //chkExportFlag.WriteChunk();
                        break;
                    }
                    case ChunkType.Mtl:
                    {
                        //Console.WriteLine("Mtl Chunk here");
                        break;
                    }
                    case ChunkType.MtlName:
                    {
                        ChunkMtlName chkMtlName = new ChunkMtlName();
                        chkMtlName.GetChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkMtlName);
                        //chkMtlName.WriteChunk();
                        break;
                    }
                    case ChunkType.DataStream:
                    {
                        ChunkDataStream chkDataStream = new ChunkDataStream();
                        chkDataStream.GetChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkDataStream);
                        //chkDataStream.WriteChunk();
                        break;
                    }
                         
                    case ChunkType.Mesh:
                    {
                        ChunkMesh chkMesh = new ChunkMesh();
                        chkMesh.GetChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkMesh);
                        // chkMesh.WriteChunk();
                        break;
                    }
                    case ChunkType.MeshSubsets:
                    {
                        ChunkMeshSubsets chkMeshSubsets = new ChunkMeshSubsets();
                        chkMeshSubsets.GetChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkMeshSubsets);
                        //chkMeshSubsets.WriteChunk();
                        break;
                    }
                    default:
                    {
                        //Console.WriteLine("Chunk type found that didn't match known versions: {0}",ChkHdr.type);
                        break;
                    }
                }
            }
            /*foreach (Chunk chnk in CgfChunks)  // Write out all chunks, for debugging purposes.
            {
                chnk.WriteChunk();
            }
            return;*/
        }

        public void WriteObjFile()
        {
            // At this point, we should have a CgfData object, fully populated.
            // We need to create the obj header, then for each submech write the vertex, UV and normal data.
            // First, let's figure out the name of the output file.  Should be <object name>.obj
            // Console.WriteLine("Output file is {0}", objOutputFile.Name);

            // Each Mesh will have a mesh subset and a series of datastream objects.  Need temporary pointers to these
            // so we can manipulate

            ChunkMesh tmpMesh;
            ChunkMeshSubsets tmpMeshSubSet;
            ChunkDataStream tmpDataStreamVertices;
            ChunkDataStream tmpDataStreamNormals;
            
            // let's practice writing the submeshes to console instead of the output file
            Console.WriteLine("# cgf-converter .obj export Version 0.1");
            Console.WriteLine("#");
            // write out the mtl file name with mtllib.  Right now we assume one mtl file per .cgf
            foreach (Chunk chunk in CgfChunks)
            {
                // First get the material file name.
                if (chunk.chunkType == ChunkType.MtlName) 
                {
                    if (chunk.chunkMtlName.Filler1 == 0x01)  // this is the mtllib file.  0x12 is mtl name
                    {
                        string tmpName;
                        tmpName = new string(chunk.chunkMtlName.Name);
                        //Console.WriteLine("mtllib {0}", tmpName);
                    }
                }
            }
            Console.WriteLine("o " );  // really need to add object name in here.
            // Dump out all the vertices (v), UVs (vt), and normals (vn) for each mesh subset
            foreach (Chunk chunk in CgfChunks)
            {
                if (chunk.chunkType == ChunkType.Mesh) // a group start.  Dump vertices, UVs and normals.
                {
                    tmpMesh = chunk.chunkMesh;
                    tmpMesh.WriteChunk();
                    // Now we have a mesh.  We need to populate the submeshes and datastreams

                    Console.WriteLine("g");
                    uint meshSubSetID = tmpMesh.id;
                    Console.WriteLine("Found Mesh ID {0:X}", meshSubSetID);
                    // Get the Submesh object based on that ID.
                    foreach (Chunk meshSubSet in CgfChunks)
                    {
                        if (meshSubSet.id == meshSubSetID)
                        {
                            // chunkMeshSubset is the object we are looking for.
                            //Console.WriteLine("Found the meshsubset for our mesh {0}", meshSubSet.id);
                        }
                    }
                    for (int i = 0; i < chunk.chunkMesh.NumMeshSubsets; i++)
                    {
                        //Console.WriteLine(" Mesh subset {0} First vertex {1}",i);
                    }

                    // Get the mesh subset for this mesh.
                }
                
            }

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

    }*/ // Fill this in later.  line 369 in cgf.xml.
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
    public enum TextureMapping : uint
    {
        NORMAL,
        ENVIRONMENT,
        SCREENENVIRONMENT,
        CUBIC,
        AUTOCUBIC
    }  //complete
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
            
            return;
        }
        public void WriteChunk()  // output header to console for testing
        {
            string tmpFileSig;
            tmpFileSig = new string(fileSignature);
            Console.WriteLine("*** HEADER ***");
            Console.WriteLine("    Header Filesignature: {0}", tmpFileSig);
            Console.WriteLine("    Header FileType: {0:X}", fileType);
            Console.WriteLine("    Header ChunkVersion: {0:X}", chunkVersion);
            Console.WriteLine("    Header ChunkTableOffset: {0:X}", fileOffset);
            Console.WriteLine("*** END HEADER ***");
            return;
        }
    }
    public class ChunkTable  // reads the chunk table
    {
        public uint numChunks;
        public List<ChunkHeader> chunkHeaders = new List<ChunkHeader>();

        // methods
        public void GetChunkTable (BinaryReader b, int f)
        {
            // need to seek to the start of the table here.  foffset points to the start of the table
            b.BaseStream.Seek(f, 0);
            numChunks = b.ReadUInt32();  // number of Chunks in the table.
            int i; // counter for loop to read all the chunkHeaders
            for (i = 0; i < numChunks; i++ )
            {
                //Console.WriteLine("Loop {0}", i);
                ChunkHeader tempChkHdr = new ChunkHeader(); // Add this chunk header to the list
                uint headerType = b.ReadUInt32(); // read the value, then parse it
                tempChkHdr.type = (ChunkType)Enum.ToObject(typeof(ChunkType), headerType);
                //Console.WriteLine("headerType: '{0}'", tempChkHdr.type);
                uint chunkversionType = b.ReadUInt32();
                tempChkHdr.version = (ChunkVersion)Enum.ToObject(typeof(ChunkVersion), chunkversionType);
                tempChkHdr.offset = b.ReadUInt32();
                tempChkHdr.id = b.ReadUInt32();  // This is the reference number to identify the mesh/datastream
                tempChkHdr.unknown = b.ReadUInt32();

                chunkHeaders.Add(tempChkHdr);
                //tempChkHdr.WriteChunk();
            }
        }
        public void WriteChunk()
        {
            foreach (ChunkHeader chkHdr in chunkHeaders)
            {
                chkHdr.WriteChunk();
            }
        }
    }
    public class ChunkHeader  
    {
        public ChunkType type;
        public ChunkVersion version;
        public uint offset;
        public uint id;
        public uint unknown; //  TBD

        // methods
        /*public void GetChunkHeader()
        {
            type = new ChunkType();
            version = new ChunkVersion();
            offset = new uint();
            id = new uint();
            unknown = new uint();
        }*/  // never used; refactor
        public void WriteChunk()  // write the Chunk Header Table to the console.  For testing.
        {
            Console.WriteLine("*** CHUNK HEADER ***");
            Console.WriteLine("    ChunkType: {0}", type);
            Console.WriteLine("    ChunkVersion: {0:X}", version);
            Console.WriteLine("    offset: {0:X}", offset);
            Console.WriteLine("    ID: {0:X}", id);
            Console.WriteLine("*** END CHUNK HEADER ***");
        }
    }

    public class Chunk // Main class has Fileoffset to identify where the chunk starts
    {
        public FileOffset fOffset;  // starting offset of the chunk.  Int, not uint
        public ChunkType chunkType; // Type of chunk
        public uint version; // version of this chunk
        public uint id; // ID of the chunk.
        // These will be uninstantiated if it's not that kind of chunk
        public ChunkTimingFormat chunkTimingFormat;
        public ChunkMtlName chunkMtlName;
        public ChunkExportFlags chunkExportFlags;
        public ChunkSourceInfo chunkSourceInfo;
        public ChunkDataStream chunkDataStream;  
        public ChunkMeshSubsets chunkMeshSubsets;  
        public ChunkMesh chunkMesh;  

        public virtual void GetChunk(BinaryReader b, uint f) 
        {
            // Don't do anything.  This is just a placeholder
        }
        public virtual void WriteChunk()
        {
            // Don't do anything.  Placeholder
        }
    }
    public class ChunkTimingFormat : Chunk // cccc000e:  Timing format chunk
    {
        // public ChunkType ChunkTiming;    // Moved to Chunk
        // public uint Version;             // Moved to Chunk
        public float SecsPerTick;
        public int TicksPerFrame;
        public uint Unknown1; // 4 bytes, not sure what they are
        public uint Unknown2; // 4 bytes, not sure what they are
        public RangeEntity GlobalRange;
        public int NumSubRanges;

        public override void GetChunk(BinaryReader b, uint f)
        {
            b.BaseStream.Seek(f, 0); // seek to the beginning of the Timing Format chunk
            uint tmpChkType = b.ReadUInt32();
            chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChkType);
            version = b.ReadUInt32();  //0x00000918 is Far Cry, Crysis, MWO, Aion
            //fOffset = b.ReadUInt32();
            //id = b.ReadUInt32();
            SecsPerTick = b.ReadSingle();
            TicksPerFrame = b.ReadInt32();
            Unknown1 = b.ReadUInt32();
            Unknown2 = b.ReadUInt32();
            GlobalRange.Name = new char[32];
            GlobalRange.Name = b.ReadChars(32);  // Name is technically a String32, but F those structs
            GlobalRange.Start = b.ReadInt32();
            GlobalRange.End = b.ReadInt32();
            chunkTimingFormat = this;
        }

        public override void WriteChunk()
        {
            string tmpName = new string(GlobalRange.Name);
            Console.WriteLine("*** TIMING CHUNK ***");
            Console.WriteLine("    Version: {0:X}", version);
            Console.WriteLine("    Secs Per Tick: {0}", SecsPerTick);
            Console.WriteLine("    Ticks Per Frame: {0}", TicksPerFrame);
            Console.WriteLine("    Global Range:  Name: {0}", tmpName);
            Console.WriteLine("    Global Range:  Start: {0}", GlobalRange.Start);
            Console.WriteLine("    Global Range:  End:  {0}", GlobalRange.End);
            Console.WriteLine("*** END TIMING CHUNK ***");
        }
    }
    public class ChunkExportFlags : Chunk  // cccc0015:  Export Flags
    {
        // public ChunkType ExportFlag;
        //public uint Version;  // Far Cry and Crysis:  1
        public uint ChunkOffset;  // for some reason the offset of Export Flag chunk is stored here.
        public uint Flags;    // ExportFlags type technically, but it's just 1 value
        public uint Unknown1; // uint, no idea what they are
        public uint[] RCVersion;  // 4 uints
        public char[] RCVersionString;  // Technically String16
        public uint[] Reserved;  // 32 uints
        public override void GetChunk(BinaryReader b, uint f)
        {
            b.BaseStream.Seek(f, 0); // seek to the beginning of the Timing Format chunk
            uint tmpExportFlag = b.ReadUInt32();
            chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpExportFlag);
            version = b.ReadUInt32();
            ChunkOffset = b.ReadUInt32();
            id = b.ReadUInt32();
            Unknown1 = b.ReadUInt32();
            RCVersion = new uint[4];
            int count = 0;
            for (count = 0; count < 4; count++)
            {
                RCVersion[count] = b.ReadUInt32();
            }
            RCVersionString = new char[16];
            RCVersionString = b.ReadChars(16);
            Reserved = new uint[32];
            for (count = 0; count < 4; count++)
            {
                Reserved[count] = b.ReadUInt32();
            }
            chunkExportFlags = this;
        }
        public override void WriteChunk() {
            string tmpVersionString = new string(RCVersionString);
            Console.WriteLine("*** START EXPORT FLAGS ***");
            Console.WriteLine("    ChunkType: {0}",chunkType);
            Console.WriteLine("    Version: {0}", version);
            Console.WriteLine("    Flags: {0}", Flags);
            Console.Write("    RC Version: ");
            for (int i = 0; i < 4; i++)
            {
                Console.Write(RCVersion[i]);
            }
            Console.WriteLine();
            Console.WriteLine("    RCVersion String: {0}", tmpVersionString);
            Console.WriteLine("    Reserved: {0:X}", Reserved);
            Console.WriteLine("*** END EXPORT FLAGS ***");
        }

        //<version num="1">Far Cry, Crysis</version>
        //<add name="Flags" type="ExportFlags" />
        //<add name="RC Version" type="uint" arr1="4" />
        //<add name="RC Version String" type="String16" />
        //<add name="Reserved" type="uint" arr1="32" />


    }
    public class ChunkSourceInfo : Chunk  // cccc0013:  Source Info chunk.  Pretty useless overall
    {
        public string SourceFile;
        public string Date;
        public string Author;

        public override void GetChunk(BinaryReader b, uint f)  //
        {
            b.BaseStream.Seek(f, 0);
            chunkType = ChunkType.SourceInfo; // this chunk doesn't actually have the chunktype header.
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
            tmpDate = b.ReadChars(count+1);  //strip off last 2 characters, because it contains a return
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
            chunkSourceInfo = this;
        }
        public override void WriteChunk()
        {
            Console.WriteLine("*** SOURCE INFO CHUNK ***");
            Console.WriteLine("    Sourcefile: {0}.  Length {1}", SourceFile, SourceFile.Length);
            Console.WriteLine("    Date:       {0}.  Length {1}", Date, Date.Length);
            Console.WriteLine("    Author:     {0}.  Length {1}", Author, Author.Length);
            Console.WriteLine("*** END SOURCE INFO CHUNK ***");
        }
    }
    public class ChunkMtlName : Chunk  // cccc000c:  provides material name as used in the .mtl file
    {
        // public ChunkType ChunkMaterialName;
        // need to find the material ID used by the mesh subsets
        public uint Flags1;  // pointer to the start of this chunk?
        public uint Flags2;  // unknown
        public uint Filler1; // for type 800, unknown value
        public uint Filler2; // for type 800, unknown value
        public uint Filler3; // for type 802, unknown value (number of materials for type 802?)
        public uint Filler4; // for type 802, unknown value
        public char[] Name; // technically a String128 class
        public MtlNamePhysicsType PhysicsType; // enum of a 4 byte uint
        public uint NumChildren; // number of materials in this name. Max is 66
        // need to implement an array of references here?  Name of Children
        public uint[] Children;  
        public uint[] Padding;  // array length of 32
        public uint AdvancedData;  // probably not used
        public float Opacity; // probably not used
        public int[] Reserved;  // array length of 32

        public override void GetChunk(BinaryReader b, uint f)
        {
            b.BaseStream.Seek(f, 0); // seek to the beginning of the Material Name chunk
            uint tmpChunkMtlName = b.ReadUInt32();
            chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkMtlName);
            version = b.ReadUInt32();
            Flags1 = b.ReadUInt32();  // offset to this chunk
            Flags2 = b.ReadUInt32();  // ref/chunk number?
            // at this point we need to differentiate between Version 800 and 802, since the format differs.
            if (version == 0x800 || version == 0x744)  // guessing on the 744. Aion.
            {
                Filler1 = b.ReadUInt32();  // if 0x1, then material lib.  If 0x12, mat name?
                Filler2 = b.ReadUInt32();
                // read the material Name, which is a 128 byte char array.  really want it as a string...
                Name = new char[128];
                Name = b.ReadChars(128);
                uint tmpPhysicsType = b.ReadUInt32();
                PhysicsType = (MtlNamePhysicsType)Enum.ToObject(typeof(MtlNamePhysicsType), tmpPhysicsType);
                NumChildren = b.ReadUInt32();
                // Now we need to read the Children references.  2 parts; the number of children, and then 66 - numchildren padding
                Children = new uint[NumChildren];
                for (int i=0; i < NumChildren; i++)
                {
                    Children[i] = b.ReadUInt32();
                }
                // Now dump the rest of the padding
                Padding = new uint[66 - NumChildren];
                for (int i = 0; i < 66 - NumChildren; i++)
                {
                    Padding[i] = b.ReadUInt32();
                }
            }
            if (version == 0x802)
            {
                // Don't need fillers for this type, but numchildren is weird...
                Name = new char[128];
                Name = b.ReadChars(128);
                Filler3 = b.ReadUInt32();
                uint tmpPhysicsType = b.ReadUInt32();
                PhysicsType = (MtlNamePhysicsType)Enum.ToObject(typeof(MtlNamePhysicsType), tmpPhysicsType);
            }
            chunkMtlName = this;
        }
        public override void WriteChunk()
        {
            string tmpMtlName = new string(Name);
            Console.WriteLine("*** START MATERIAL NAMES ***");
            Console.WriteLine("    ChunkType: {0}", chunkType);
            Console.WriteLine("    Material Name:  {0}", tmpMtlName);
            Console.WriteLine("    Number of Children: {0}", NumChildren);
            Console.WriteLine("*** END MATERIAL NAMES ***");
        }
    }
    public class ChunkDataStream : Chunk // cccc0016:  Contains data such as vertices, normals, etc.
    {
        // public ChunkType ChkDataStream; //  cccc0016.
        // public uint version;  // 800 for Crysis.  inherited from Chunk
        public uint Flags; // not used, but looks like the start of the Data Stream chunk
        public uint Flags1; // not used.  uint after Flags that looks like offsets
        public uint Flags2; // not used, looks almost like a filler.
        public DataStreamType dataStreamType; // type of data (vertices, normals, uv, etc)
        public uint NumElements; // Number of data entries
        public uint BytesPerElement; // Bytes per data entry
        public uint Reserved1;
        public uint Reserved2;
        // Need to be careful with using float for Vertices and normals.  technically it's a floating point of length BytesPerElement.  May need to fix this.
        public Vector3[] Vertices;  // For dataStreamType of 0, length is NumElements. 
        public float[] VertexList; // list of vertices?  Not part of default structures. Temp for old SC files.
        public Vector3[] Normals;   // For dataStreamType of 1, length is NumElements.
        public UV[] UVs;            // for datastreamType of 2, length is NumElements.
        public IRGB[] RGBColors;    // for dataStreamType of 3, length is NumElements.  Bytes per element of 3
        public IRGBA[] RGBAColors;  // for dataStreamType of 4, length is NumElements.  Bytes per element of 4
        public ushort[] Indices;    // for dataStreamType of 5, length is NumElements.
        // For Tangents on down, this may be a 2 element array.  See line 846+ in cgf.xml
        public Tangent[,] Tangents;  // for dataStreamType of 6, length is NumElements,2.  
        public byte[,] ShCoeffs;     // for dataStreamType of 7, length is NumElement,BytesPerElements.
        public byte[,] ShapeDeformation; // for dataStreamType of 8, length is NumElements,BytesPerElement.
        public byte[,] BoneMap;      // for dataStreamType of 9, length is NumElements,BytesPerElement.
        public byte[,] FaceMap;      // for dataStreamType of 10, length is NumElements,BytesPerElement.
        public byte[,] VertMats;     // for dataStreamType of 11, length is NumElements,BytesPerElement.

        public override void GetChunk(BinaryReader b, uint f)
        {
            b.BaseStream.Seek(f, 0); // seek to the beginning of the DataStream chunk
            uint tmpChunkDataStream = b.ReadUInt32();
            chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkDataStream);
            version = b.ReadUInt32();
            Flags = b.ReadUInt32();  // Offset to this chunk
            //int tmpfOffset = b.ReadInt32();
            //fOffset = (FileOffset)Enum.ToObject(typeof(FileOffset), tmpfOffset);  // offset of this datastream chunk
            id = b.ReadUInt32();  // Reference to the data stream type!  4th word after the start of the chunk
            Flags2 = b.ReadUInt32(); // another filler
            uint tmpdataStreamType = b.ReadUInt32();
            dataStreamType = (DataStreamType)Enum.ToObject(typeof(DataStreamType), tmpdataStreamType);
            NumElements = b.ReadUInt32(); // number of elements in this chunk
            BytesPerElement = b.ReadUInt32(); // bytes per element
            Reserved1 = b.ReadUInt32();
            Reserved2 = b.ReadUInt32();
            // Now do loops to read for each of the different Data Stream Types.  If vertices, need to populate Vector3s for example.
            switch (dataStreamType)
            {
                case DataStreamType.VERTICES:  // Ref is 0x00000000
                {
                    Vertices = new Vector3[NumElements];
                    if (BytesPerElement == 12)
                    {
                        for (int i = 0; i < NumElements; i++)
                        {
                            Vertices[i].x = b.ReadSingle();
                            Vertices[i].y = b.ReadSingle();
                            Vertices[i].z = b.ReadSingle();
                        }
                    }
                    if (BytesPerElement == 8)  // Old Star Citizen files
                    {
                        // Need to figure out this case.  Star Citizen old files are like this.  Annoying.
                        VertexList = new float[NumElements * BytesPerElement / 4];  //
                        for (int i = 0; i < VertexList.Length; i++)
                        {
                            VertexList[i] = b.ReadSingle();
                            // Console.WriteLine("{0,10:F6}", VertexList[i]);
                        }

                    }
                    // Console.WriteLine("{0} elements read", VertexList.Length);
                    // Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                    break;
                }
                case  DataStreamType.INDICES:  // Ref is 
                {
                    Indices = new ushort[NumElements];
                    for (int i = 0; i < NumElements; i++)
                    {
                        Indices[i] = b.ReadUInt16();
                        // Console.WriteLine("Indice {0} is {1}", i, Indices[i]);
                        if (i > NumElements - 20)
                        {
                            //Console.WriteLine("Indices {0} is {1}", i, Indices[i]);
                        }
                    }
                    //Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                    break;
                }
                case DataStreamType.NORMALS:
                {
                    Normals = new Vector3[NumElements];
                    for (int i=0; i < NumElements ; i++)
                    {
                        Normals[i].x = b.ReadSingle();
                        Normals[i].y = b.ReadSingle();
                        Normals[i].z = b.ReadSingle();
                        // Console.WriteLine("{0}  {1}  {2}", Normals[i].x, Normals[i].y, Normals[i].z);
                    }
                    //Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                    break;

                }
                case DataStreamType.UVS:
                {
                    UVs = new UV[NumElements];
                    for (int i = 0; i < NumElements; i++)
                    {
                        UVs[i].U = b.ReadSingle();
                        UVs[i].V = b.ReadSingle();
                        // Console.WriteLine("{0}   {1}", UVs[i].U, UVs[i].V);
                    }
                    //Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                    break;
                }
                case DataStreamType.TANGENTS:
                {
                    Tangents = new Tangent[NumElements,2];
                    for (int i = 0; i < NumElements; i++)
                    {
                        // These have to be divided by 32767 to be used properly (value between 0 and 1)
                        Tangents[i, 0].x = b.ReadInt16();
                        Tangents[i, 0].y = b.ReadInt16();
                        Tangents[i, 0].z = b.ReadInt16();
                        Tangents[i, 0].w = b.ReadInt16();
                        Tangents[i, 1].x = b.ReadInt16();
                        Tangents[i, 1].y = b.ReadInt16();
                        Tangents[i, 1].z = b.ReadInt16();
                        Tangents[i, 1].w = b.ReadInt16();
                        //Console.WriteLine("{0} {1} {2} {3}", Tangents[i, 0].x, Tangents[i, 0].y, Tangents[i, 0].z, Tangents[i, 0].w);
                    }
                    // Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                    break;
                }
                case DataStreamType.COLORS:
                {
                    if (BytesPerElement == 3)
                    {
                        RGBColors = new IRGB[NumElements];
                        for (int i = 0; i < NumElements; i++)
                        {
                            RGBColors[i].r = b.ReadByte();
                            RGBColors[i].g = b.ReadByte();
                            RGBColors[i].b = b.ReadByte();
                        }
                    }
                    if (BytesPerElement == 4)
                    {
                        RGBAColors = new IRGBA[NumElements];
                        for (int i = 0; i < NumElements; i++)
                        {
                            RGBAColors[i].r = b.ReadByte();
                            RGBAColors[i].g = b.ReadByte();
                            RGBAColors[i].b = b.ReadByte();
                            RGBAColors[i].a = b.ReadByte();

                        }
                    }
                    break;
                }
                default: 
                {
                    Console.WriteLine("***** Unknown DataStream Type *****");
                    break;
                }
            }
            chunkDataStream = this;
        }
        public override void WriteChunk()
        {
            //string tmpDataStream = new string(Name);
            Console.WriteLine("*** START DATASTREAM ***");
            Console.WriteLine("    ChunkType:                       {0}", chunkType);
            Console.WriteLine("    Version:                         {0:X}", version);
            Console.WriteLine("    DataStream chunk starting point: {0:X}", fOffset);
            Console.WriteLine("    Reference Type:                  {0:X}", id);
            Console.WriteLine("    DataStreamType:                  {0}", dataStreamType);
            Console.WriteLine("    Number of Elements:              {0}", NumElements);
            Console.WriteLine("    Bytes per Element:               {0}", BytesPerElement);
            Console.WriteLine("*** END DATASTREAM ***");

        }
    }
    public class ChunkMeshSubsets : Chunk // cccc0017:  The different parts of a mesh.  Needed for obj exporting
    {
        
        public uint Offset;
        public uint Flags; // probably the offset
        public uint ID; // The mesh chunk refers to the ID
        public uint NumMeshSubset; // number of mesh subsets
        public int Reserved1;
        public int Reserved2;
        public int Reserved3;
        public MeshSubset[] MeshSubsets;

        public override void GetChunk(BinaryReader b, uint f) 
        {
            b.BaseStream.Seek(f, 0); // seek to the beginning of the MeshSubset chunk
            uint tmpChunkType = b.ReadUInt32();
            chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkType);
            version = b.ReadUInt32(); // probably 800
            Offset = b.ReadUInt32();  // offset of this chunk
            Flags = b.ReadUInt32();   // Might be a ref to this chunk
            ID = b.ReadUInt32(); // ID of this chunk.  Used to reference the mesh chunk
            NumMeshSubset = b.ReadUInt32();  // number of mesh subsets
            Reserved1 = b.ReadInt32();
            Reserved2 = b.ReadInt32();
            // Reserved3 = b.ReadInt32();
            MeshSubsets = new MeshSubset[NumMeshSubset];
            for (int i = 0; i < NumMeshSubset; i++)
            {
                MeshSubsets[i].FirstIndex = b.ReadUInt32();
                MeshSubsets[i].NumIndices = b.ReadUInt32();
                MeshSubsets[i].FirstVertex = b.ReadUInt32();
                MeshSubsets[i].NumVertices = b.ReadUInt32();
                MeshSubsets[i].MatID = b.ReadUInt32();
                MeshSubsets[i].Radius = b.ReadSingle();
                MeshSubsets[i].Center.x = b.ReadSingle();
                MeshSubsets[i].Center.y = b.ReadSingle();
                MeshSubsets[i].Center.z = b.ReadSingle();
            }
            chunkMeshSubsets = this;
        }
        public override void WriteChunk()
        {
            Console.WriteLine("*** START MESH SUBSET CHUNK ***");
            Console.WriteLine("    ChunkType: {0}", chunkType);
            Console.WriteLine("    Number of Mesh Subsets: {0}", NumMeshSubset);
            for (int i = 0; i < NumMeshSubset; i++)
            {
                Console.WriteLine("        ** Mesh Subset:          {0}", i);
                Console.WriteLine("           First Index:          {0}", MeshSubsets[i].FirstIndex);
                Console.WriteLine("           Number of Indices:    {0}", MeshSubsets[i].NumIndices);
                Console.WriteLine("           First Vertex:         {0}", MeshSubsets[i].FirstVertex);
                Console.WriteLine("           Number of Vertices:   {0}", MeshSubsets[i].NumVertices);
                Console.WriteLine("           Material ID:          {0}", MeshSubsets[i].MatID);
                Console.WriteLine("           Radius:               {0}", MeshSubsets[i].Radius);
                Console.WriteLine("           Center:   {0},{1},{2}", MeshSubsets[i].Center.x, MeshSubsets[i].Center.y, MeshSubsets[i].Center.z);
                Console.WriteLine("        ** Mesh Subset {0} End", i);
            }
            Console.WriteLine("*** END MESH SUBSET CHUNK ***");
        }
    }
    public class ChunkMesh : Chunk      //  cccc0000:  Object that points to the datastream chunk.
    {
        //public ChunkType MeshChunk;
        // public uint Version;  // 623 Far Cry, 744 Far Cry, Aion, 800 Crysis
        //public bool HasVertexWeights; // for 744
        //public bool HasVertexColors; // 744
        //public bool InWorldSpace; // 623
        //public byte Reserved1;  // padding byte, 744
        //public byte Reserved2;  // padding byte, 744
        public uint Flags1;  // 800  Offset of this chunk. 
        // public uint ID;  // 800  Chunk ID
        public uint Unknown1; // for 800, not sure what this is.  Value is 2?
        public uint Unknown2; // for 800, not sure what this is.  Value is 0?
        public uint NumVertices; // 
        public uint NumIndices;  // Number of indices (each triangle has 3 indices, so this is the number of triangles times 3).
        //public uint NumUVs; // 744
        //public uint NumFaces; // 744
        // Pointers to various Chunk types
        //public ChunkMtlName Material; // 623, Material Chunk, never encountered?
        public uint NumMeshSubsets; // 800, Number of Mesh subsets
        public uint MeshSubsets; // 800  Reference of the mesh subsets
        // public ChunkVertAnim VertAnims; // 744.  not implemented
        //public Vertex[] Vertices; // 744.  not implemented
        //public Face[,] Faces; // 744.  Not implemented
        //public UV[] UVs; // 744 Not implemented
        //public UVFace[] UVFaces; // 744 not implemented
        // public VertexWeight[] VertexWeights; // 744 not implemented
        //public IRGB[] VertexColors; // 744 not implemented
        public uint VerticesData; // 800
        public uint NormalsData; // 800
        public uint UVsData; // 800
        public uint ColorsData; // 800
        public uint Colors2Data; // 800 
        public uint IndicesData; // 800
        public uint TangentsData; // 800
        public uint ShCoeffsData; // 800
        public uint ShapeDeformationData; //800
        public uint BoneMapData; //800
        public uint FaceMapData; // 800
        public uint VertMatsData; // 800
        public uint[] ReservedData = new uint[4]; // 800 Length 4
        public uint[] PhysicsData = new uint[4]; // 800
        public Vector3 MinBound; // 800 minimum coordinate values
        public Vector3 MaxBound; // 800 Max coord values
        public uint[] Reserved3 = new uint[32]; // 800 array of 32 uint values.

        //public ChunkMeshSubsets chunkMeshSubset; // pointer to the mesh subset that belongs to this mesh

        public override void GetChunk(BinaryReader b, uint f) 
        {
            b.BaseStream.Seek(f, 0); // seek to the beginning of the MeshSubset chunk
            uint tmpChunkType = b.ReadUInt32();
            chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkType);
            version = b.ReadUInt32();
            Flags1 = b.ReadUInt32();  // offset
            id = b.ReadUInt32();  // Chunk ID  0x23 for candle
            Unknown1 = b.ReadUInt32();  // unknown
            Unknown1 = b.ReadUInt32();  // unknown
            NumVertices = b.ReadUInt32();
            NumIndices = b.ReadUInt32();   //
            NumMeshSubsets = b.ReadUInt32();
            MeshSubsets = b.ReadUInt32();  // refers to ID in mesh subsets  1d for candle
            NormalsData = b.ReadUInt32();
            UVsData = b.ReadUInt32();     // refers to the ID in the Normals datastream?
            ColorsData = b.ReadUInt32();
            Colors2Data = b.ReadUInt32();
            IndicesData = b.ReadUInt32();
            TangentsData = b.ReadUInt32();
            ShCoeffsData = b.ReadUInt32();
            ShapeDeformationData = b.ReadUInt32();
            BoneMapData = b.ReadUInt32();
            FaceMapData = b.ReadUInt32();
            VertMatsData = b.ReadUInt32();
            for (int i = 0; i < 4; i++)
            {
                ReservedData[i] = b.ReadUInt32();
            }
            for (int i = 0; i < 4; i++)
            {
                PhysicsData[i] = b.ReadUInt32();
            }
            MinBound.x = b.ReadUInt32();
            MinBound.y = b.ReadUInt32();
            MinBound.z = b.ReadUInt32();
            MaxBound.x = b.ReadUInt32();
            chunkMesh = this;
            // Not going to read the Reserved 32 element array.
        }
        public override void WriteChunk()
        {
            Console.WriteLine("*** START MESH CHUNK ***");
            Console.WriteLine("    ChunkType: {0}", chunkType);
            Console.WriteLine("*** END MESH CHUNK ***");
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
            FileInfo dataFile;  // This is the file passed in args

            if (args.Length != 0 && File.Exists(args[0]))
            {
                dataFile = new FileInfo(args[0]);  // preferred.  We want objects, not strings
            }
            else
            {
                Console.WriteLine("Please input cgf/cga/chrparams file.");
                dataFile = new FileInfo(Console.ReadLine());  // This needs error checking.
            }
            
            Console.WriteLine("Input File is '{0}'" , dataFile.Name);
            //ReadCryHeader(cgfFile);
            CgfData cgfData = new CgfData(dataFile);
            
            // Output to an obj file
            cgfData.WriteObjFile();

            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey(); // Press any key to continue
            
            return;
        }
    }
}
