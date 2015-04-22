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
        public string CgfFile { get; set; } //the name of the file we are reading
        public Header CgfHeader { get; set; }
        public ChunkTable CgfChunkTable { get; set; }
        public List<ChunkBase> CgfChunks { get; set; }

        public CgfFormat(string cgffile)
        {
            CgfFile = cgffile;
            CgfHeader = new Header();
            CgfChunkTable = new ChunkTable();
        }
    }
    // Aliases
    public class ChunkVersion
    {
        public uint ChunkVersion;

        public ChunkVersion()
        {
            //ChunkVersions = new ChunkVersion;
        }
    }
    public class FileOffset
    { 
        public int FileOffset;
    }
    // Enums
    public class FileType
    {
        public enum value
        {
            GEOM = 0xFFFF0000,
            ANIM = 0xFFFF0001
        }
    }
    public class ChunkType
    {
        public enum value
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
    }

    // structs
    public class Header
    {
        public char[] fileSignature; // The CGF file signature.  CryTek for 3.5, CrChF for 3.6
        public uint fileType; // The CGF file type (geometry or animation)  3.5 only
        public uint chunkVersion // The version of the chunk table  3.5 only
        {
            get;
            set;
        }
        public int fileOffset //Position of the chunk table in the CGF file
        {
            get;
            set;
        }
        public int numChunks // Number of chunks in the Chunk Table (3.6 only.  3.5 has it in Chunk Table)
        {
            get;
            set;
        }
    }
    // comment
    public class ChunkHeader
    {
        public ChunkType type;
        public ChunkVersion version;
        public uint offset;
        public uint id;
    }
    public class ChunkTable
    {
        public uint numChunks;
        public ChunkHeader[] chunkHeaders;

    }

    public class ChunkBase
    {
        // don't need anything here
    }

    public class ChuckTimingFormat : ChunkBase
    {
        public void ChunkTimingFormat()
        {


        }
        public float SecsPerTick;
        public int TicksPerFrame;
    }

    class Program
    {
        static void ReadCryHeader(string cgfFile)
        {
            CgfFormat cgfData = new CgfFormat();
            
            using (BinaryReader b = new BinaryReader(File.Open(cgfFile, FileMode.Open)))
            {
                // header.FileSignature 
                cgfData.CgfHeader.fileSignature = b.ReadChars(8);
                cgfData.CgfHeader.fileType = b.ReadUInt32();
                cgfData.CgfHeader.chunkVersion = b.ReadUInt32();
                cgfData.CgfHeader.fileOffset = b.ReadInt32();
            }
            Console.Write("File Signature set to:  ");
            Console.WriteLine(cgfData.CgfHeader.fileSignature);
            Console.WriteLine("File Type set to: '{0:X}'", cgfData.fileType);
            Console.WriteLine("Chunk Version set to : '{0:X}' ", cgfData.chunkVersion);
            Console.WriteLine("Chunk Table Offset set to: {0:X}'  ", cgfData.fileOffset);
        }
        static void ReadChunkTable(string cgfFile)
        {
            
        }

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
            ReadCryHeader(cgfFile);
            Console.ReadKey();
            
            return;
        }
    }
}
