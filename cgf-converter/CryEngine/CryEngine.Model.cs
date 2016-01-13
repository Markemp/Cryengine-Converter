using OpenTK.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CgfConverter.CryEngine_Core;
using System.Linq.Expressions;
using System.Reflection;

namespace CgfConverter
{
    public partial class CryEngine
    {
        // MatType for type 800, 0x1 is material library, 0x12 is child, 0x10 is solo material

        /// <summary>
        /// CryEngine cgf/cga/skin file handler
        /// </summary>
        public class Model
        {
            public Int32 NodeCount { get { return this.ChunkMap.Values.Where(c => c.ChunkType == ChunkTypeEnum.Node).Count(); } }
            public String FileName { get; internal set; }

            // Header, ChunkTable and Chunks are what are in a file.  1 header, 1 table, and a chunk for each entry in the table.
            internal static FileVersionEnum FILE_VERSION;
            internal  static UInt32 NUM_CHUNKS;          // number of chunks in the chunk table

            public static Model FromFile(String fileName)
            {
                Model buffer = new Model();
                buffer.Load(fileName);
                return buffer;
            }

            public Model()
            {
                this.ChunkMap = new Dictionary<UInt32, CryEngine_Core.Chunk> { };
                this.ChunkHeaders = new List<ChunkHeader> { };
                this.Headers = new ChunkTable { };
            }

            #region Legacy

            public FileHeader CgfHeader { get; internal set; }
            /// <summary>
            /// CgfChunkTable contains a list of all the Chunks.
            /// </summary>
            public ChunkTable Headers { get; internal set; }
            public Dictionary<UInt32, CryEngine_Core.Chunk> ChunkMap { get; internal set; }
            public ChunkNode RootNode { get; set; }
            public List<ChunkHeader> ChunkHeaders {get; internal set; }

            /// <summary>
            /// Load a cgf/cga/skin file
            /// </summary>
            /// <param name="fileName"></param>
            public void Load(String fileName)
            {
                FileInfo inputFile = new FileInfo(fileName);

                Console.Title = String.Format("Processing {0}...", inputFile.Name);

                this.FileName = inputFile.Name;

                if (!inputFile.Exists)
                    throw new FileNotFoundException();

                // Open the file for reading.
                BinaryReader cgfReader = new BinaryReader(File.Open(fileName, FileMode.Open));
                // Get the header.  This isn't essential for .cgam files, but we need this info to find the version and offset to the chunk table
                this.CgfHeader = new FileHeader();                       // Gets the header of the file (3-5 objects dep on version)
                this.CgfHeader.Read(cgfReader);

                FILE_VERSION = this.CgfHeader.FileVersion;
                NUM_CHUNKS = this.CgfHeader.NumChunks;

                switch (this.CgfHeader.FileVersion)
                {
                    case (FileVersionEnum.CryTek_3_4):
                        this.Headers.GetChunkTable<CryEngine_Core.ChunkHeader_744>(cgfReader, CgfHeader.ChunkTableOffset);
                        break;
                    case (FileVersionEnum.CryTek_3_5):
                        this.Headers.GetChunkTable<CryEngine_Core.ChunkHeader_745>(cgfReader, CgfHeader.ChunkTableOffset);
                        break;
                    case (FileVersionEnum.CryTek_3_6):
                        this.Headers.GetChunkTable<CryEngine_Core.ChunkHeader_746>(cgfReader, CgfHeader.ChunkTableOffset);
                        break;
                    default:
                        Console.WriteLine("Unknown Header");
                        this.CgfHeader.WriteChunk();
                        break;
                }

                foreach (ChunkHeader chkHdr in this.Headers.Items)
                {
                    this.ChunkMap[chkHdr.ID] = Chunk.New(chkHdr.ChunkType, chkHdr.Version);
                    this.ChunkMap[chkHdr.ID].Load(this, chkHdr);
                    this.ChunkMap[chkHdr.ID].Read(cgfReader);

                    // Ensure we read to end of structure
                    this.ChunkMap[chkHdr.ID].SkipBytes(cgfReader);
                    
                    // TODO: Change this to detect node with NULL or 0xFFFFFFFF parent ID
                    // Assume first node read is root node
                    if (chkHdr.ChunkType == ChunkTypeEnum.Node && this.RootNode == null)
                    {
                        this.RootNode = this.ChunkMap[chkHdr.ID] as ChunkNode;
                    }
                }
            }

            public void WriteTransform(Vector3 transform)
            {
                Console.WriteLine("Transform:");
                Console.WriteLine("{0}    {1}    {2}", transform.x, transform.y, transform.z);
                Console.WriteLine();
            }

            #region DataTypes

            public class FileHeader
            {
                public String FileSignature; // The CGF file signature.  CryTek for 3.5, CrCh for 3.6
                public FileTypeEnum FileType; // The CGF file type (geometry or animation)  3.5 only
                public FileVersionEnum FileVersion; // The version of the chunk table 3.5 only
                public Int32 ChunkTableOffset; // Position of the chunk table in the CGF file
                /// <summary>
                /// 3.6 Only - Number of chunks in the Chunk Table
                /// </summary>
                public UInt32 NumChunks { get; internal set; }
                //public Int32 FileVersion;         // 0 will be 3.4 and older, 1 will be 3.6 and newer.  THIS WILL CHANGE
                // methods
                public void Read(BinaryReader b)  //constructor with 1 arg
                {
                    #region Detect FileSignature v3.6+

                    b.BaseStream.Seek(0, 0);
                    this.FileSignature = b.ReadFString(4);

                    if (this.FileSignature == "CrCh")
                    {
                        // Version 3.6 or later
                        this.FileVersion = (FileVersionEnum)b.ReadUInt32();    // 0x746
                        this.NumChunks = b.ReadUInt32();       // number of Chunks in the chunk table
                        this.ChunkTableOffset = b.ReadInt32(); // location of the chunk table

                        return;
                    }

                    #endregion

                    #region Detect FileSignature v3.5-

                    b.BaseStream.Seek(0, 0);
                    this.FileSignature = b.ReadFString(8);

                    if (this.FileSignature == "CryTek")
                    {

                        // Version 3.5 or earlier
                        this.FileType = (FileTypeEnum)b.ReadUInt32();
                        this.FileVersion = (FileVersionEnum)b.ReadUInt32();    // 0x744 0x745
                        this.ChunkTableOffset = b.ReadInt32() + 4;
                        this.NumChunks = b.ReadUInt32();       // number of Chunks in the chunk table
                        
                        return;
                    }

                    #endregion

                    throw new NotSupportedException(String.Format("Unsupported FileSignature {0}", this.FileSignature));
                }

                public void Write(BinaryWriter writer)
                {
                    throw new NotImplementedException();
                }

                public void WriteChunk()  // output header to console for testing
                {
                    Console.WriteLine("*** HEADER ***");
                    Console.WriteLine("    Header Filesignature: {0}", this.FileSignature);
                    Console.WriteLine("    FileType:            {0:X}", this.FileType);
                    Console.WriteLine("    ChunkVersion:        {0:X}", this.FileVersion);
                    Console.WriteLine("    ChunkTableOffset:    {0:X}", this.ChunkTableOffset);
                    Console.WriteLine("    NumChunks:           {0:X}", this.NumChunks);

                    Console.WriteLine("*** END HEADER ***");
                    return;
                }
            }

            /// <summary>
            /// Table that contains list of Chunk Headers
            /// </summary>
            public class ChunkTable
            {
                public List<ChunkHeader> Items = new List<ChunkHeader>();

                public void GetChunkTable<TChunkHeader>(BinaryReader b, Int32 f) where TChunkHeader : ChunkHeader, new()
                {
                    // need to seek to the start of the table here.  foffset points to the start of the table
                    b.BaseStream.Seek(f, SeekOrigin.Begin);

                    for (Int32 i = 0; i < NUM_CHUNKS; i++)
                    {
                        TChunkHeader tempChkHdr = new TChunkHeader();

                        tempChkHdr.Read(b);

                        this.Items.Add(tempChkHdr);
                    }
                }

                public void WriteChunk()
                {
                    Console.WriteLine("*** Chunk Header Table***");
                    Console.WriteLine("Chunk Type              Version   ID        Size      Offset    ");
                    foreach (ChunkHeader chkHdr in this.Items)
                    {
                        Console.WriteLine("{0,-24:X}{1,-10:X}{2,-10:X}{3,-10:X}{4,-10:X}", chkHdr.ChunkType.ToString(), chkHdr.Version, chkHdr.ID, chkHdr.Size, chkHdr.Offset);
                    }
                }
            }

            #endregion

            #endregion
        }
    }
}