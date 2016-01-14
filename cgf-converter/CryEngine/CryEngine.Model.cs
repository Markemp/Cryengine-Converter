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
            // Header, ChunkTable and Chunks are what are in a file.  1 header, 1 table, and a chunk for each entry in the table.
            internal static FileVersionEnum FILE_VERSION;
            internal static UInt32 NUM_CHUNKS;          // number of chunks in the chunk table

            #region Public Properties

            public ChunkNode RootNode { get; internal set; }
            public Dictionary<UInt32, CryEngine_Core.Chunk> ChunkMap { get; internal set; }
            public List<ChunkHeader> ChunkHeaders { get; internal set; }

            public String FileName { get; internal set; }
            public String FileSignature { get; internal set; } // The CGF file signature.  CryTek for 3.5, CrCh for 3.6
            public FileTypeEnum FileType { get; internal set; } // The CGF file type (geometry or animation)  3.5 only
            public FileVersionEnum FileVersion { get; internal set; } // The version of the chunk table 3.5 only
            public Int32 ChunkTableOffset { get; internal set; } // Position of the chunk table in the CGF file
            public UInt32 NumChunks { get; internal set; }
            
            #endregion

            #region Private Fields

            public List<ChunkHeader> _chunks = new List<ChunkHeader> { };

            #endregion

            #region Calculated Properties

            public Int32 NodeCount { get { return this.ChunkMap.Values.Where(c => c.ChunkType == ChunkTypeEnum.Node).Count(); } }

            #endregion

            #region Constructor/s

            public Model()
            {
                this.ChunkMap = new Dictionary<UInt32, CryEngine_Core.Chunk> { };
                this.ChunkHeaders = new List<ChunkHeader> { };
            }

            #endregion

            #region Public Methods

            public static Model FromFile(String fileName)
            {
                Model buffer = new Model();
                buffer.Load(fileName);
                return buffer;
            }

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
                BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open));
                // Get the header.  This isn't essential for .cgam files, but we need this info to find the version and offset to the chunk table
                this.LoadHeader(reader);

                FILE_VERSION = this.FileVersion;
                NUM_CHUNKS = this.NumChunks;

                switch (this.FileVersion)
                {
                    case (FileVersionEnum.CryTek_3_4):
                        this.LoadChunkHeaders<CryEngine_Core.ChunkHeader_744>(reader, this.ChunkTableOffset);
                        break;
                    case (FileVersionEnum.CryTek_3_5):
                        this.LoadChunkHeaders<CryEngine_Core.ChunkHeader_745>(reader, this.ChunkTableOffset);
                        break;
                    case (FileVersionEnum.CryTek_3_6):
                        this.LoadChunkHeaders<CryEngine_Core.ChunkHeader_746>(reader, this.ChunkTableOffset);
                        break;
                    default:
                        Utils.Log(LogLevelEnum.Debug, "Unknown Header");
                        this.WriteHeaderChunk();
                        break;
                }

                this.LoadChunks(reader);
            }

            public void LoadHeader(BinaryReader b)
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

            public void WriteHeaderChunk()  // output header to console for testing
            {
                Utils.Log(LogLevelEnum.Debug, "*** HEADER ***");
                Utils.Log(LogLevelEnum.Debug, "    Header Filesignature: {0}", this.FileSignature);
                Utils.Log(LogLevelEnum.Debug, "    FileType:            {0:X}", this.FileType);
                Utils.Log(LogLevelEnum.Debug, "    ChunkVersion:        {0:X}", this.FileVersion);
                Utils.Log(LogLevelEnum.Debug, "    ChunkTableOffset:    {0:X}", this.ChunkTableOffset);
                Utils.Log(LogLevelEnum.Debug, "    NumChunks:           {0:X}", this.NumChunks);

                Utils.Log(LogLevelEnum.Debug, "*** END HEADER ***");
                return;
            }

            public void LoadChunkHeaders<TChunkHeader>(BinaryReader b, Int32 f) where TChunkHeader : ChunkHeader, new()
            {
                // need to seek to the start of the table here.  foffset points to the start of the table
                b.BaseStream.Seek(f, SeekOrigin.Begin);

                for (Int32 i = 0; i < NUM_CHUNKS; i++)
                {
                    TChunkHeader tempChkHdr = new TChunkHeader();

                    tempChkHdr.Read(b);

                    this._chunks.Add(tempChkHdr);
                }
            }

            public void WriteChunkTable()
            {
                Utils.Log(LogLevelEnum.Debug, "*** Chunk Header Table***");
                Utils.Log(LogLevelEnum.Debug, "Chunk Type              Version   ID        Size      Offset    ");
                foreach (ChunkHeader chkHdr in this._chunks)
                {
                    Utils.Log(LogLevelEnum.Debug, "{0,-24:X}{1,-10:X}{2,-10:X}{3,-10:X}{4,-10:X}", chkHdr.ChunkType.ToString(), chkHdr.Version, chkHdr.ID, chkHdr.Size, chkHdr.Offset);
                }
            }

            public void LoadChunks(BinaryReader reader)
            {
                foreach (ChunkHeader chkHdr in this._chunks)
                {
                    this.ChunkMap[chkHdr.ID] = Chunk.New(chkHdr.ChunkType, chkHdr.Version);
                    this.ChunkMap[chkHdr.ID].Load(this, chkHdr);
                    this.ChunkMap[chkHdr.ID].Read(reader);

                    // Ensure we read to end of structure
                    this.ChunkMap[chkHdr.ID].SkipBytes(reader);

                    // TODO: Change this to detect node with NULL or 0xFFFFFFFF parent ID
                    // Assume first node read is root node
                    if (chkHdr.ChunkType == ChunkTypeEnum.Node && this.RootNode == null)
                    {
                        this.RootNode = this.ChunkMap[chkHdr.ID] as ChunkNode;
                    }
                }
            }

            #endregion
        }
    }
}