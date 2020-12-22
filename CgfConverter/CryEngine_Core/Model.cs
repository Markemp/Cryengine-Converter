using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter.CryEngineCore
{

    /// <summary>
    /// CryEngine cgf/cga/skin file handler
    /// 
    /// Structure:
    ///   HEADER        <- Provides information about the format of the file
    ///   CHUNKHEADER[] <- Provides information about locations of CHUNKs
    ///   CHUNK[]
    /// </summary>
    public class Model
    {
        #region Public Properties

        /// <summary>
        /// The Root of the loaded object
        /// </summary>
        public ChunkNode RootNode { get; internal set; }

        /// <summary>
        /// Collection of all loaded Chunks
        /// </summary>
        public List<ChunkHeader> ChunkHeaders { get; internal set; }

        /// <summary>
        /// Lookup Table for Chunks, indexed by ChunkID
        /// </summary>
        public Dictionary<int, Chunk> ChunkMap { get; internal set; }

        /// <summary>
        /// The name of the currently processed file
        /// </summary>
        public string FileName { get; internal set; }

        /// <summary>
        /// The File Signature - CryTek for 3.5 and lower. CrCh for 3.6 and higher
        /// </summary>
        public string FileSignature { get; internal set; }

        /// <summary>
        /// The type of file (geometry or animation)
        /// </summary>
        public FileTypeEnum FileType { get; internal set; }

        /// <summary>
        /// The version of the file
        /// </summary>
        public FileVersionEnum FileVersion { get; internal set; }

        /// <summary>
        /// Position of the Chunk Header table
        /// </summary>
        public int ChunkTableOffset { get; internal set; }

        /// <summary>
        /// Contains all the information about bones and skinning them.  This a reference to the Cryengine object, since multiple Models can exist for a single object).
        /// </summary>
        public SkinningInfo SkinningInfo { get; set; }

        /// <summary>
        /// The Bones in the model.  The CompiledBones chunk will have a unique RootBone.
        /// </summary>
        public ChunkCompiledBones Bones { get; internal set; }

        public uint NumChunks { get; internal set; }

        private Dictionary<int, ChunkNode> nodeMap { get; set; }

        /// <summary>
        /// Node map for this model only.
        /// </summary>
        public Dictionary<int, ChunkNode> NodeMap      // This isn't right.  Nodes can have duplicate names.
        {
            get
            {
                if (this.nodeMap == null)
                {
                    this.nodeMap = new Dictionary<int, ChunkNode>() { };
                    ChunkNode rootNode = null;
                    this.RootNode = rootNode = (rootNode ?? this.RootNode);  // Each model will have it's own rootnode.

                    foreach (CryEngineCore.ChunkNode node in this.ChunkMap.Values.Where(c => c.ChunkType == ChunkTypeEnum.Node).Select(c => c as ChunkNode))
                    {
                        // Preserve existing parents
                        if (this.nodeMap.ContainsKey(node.ID))
                        {
                            ChunkNode parentNode = this.nodeMap[node.ID].ParentNode;

                            if (parentNode != null)
                                parentNode = this.nodeMap[parentNode.ID];

                            node.ParentNode = parentNode;
                        }

                        this.nodeMap[node.ID] = node;
                    }
                }
                return this.nodeMap;
            }
        }

        #endregion

        #region Private Fields

        public List<ChunkHeader> _chunks = new List<ChunkHeader> { };

        #endregion

        #region Calculated Properties

        public int NodeCount { get { return this.ChunkMap.Values.Where(c => c.ChunkType == ChunkTypeEnum.Node).Count(); } }

        public int BoneCount { get { return this.ChunkMap.Values.Where(c => c.ChunkType == ChunkTypeEnum.CompiledBones).Count(); } }

        #endregion

        #region Constructor/s

        public Model()
        {
            this.ChunkMap = new Dictionary<int, CryEngineCore.Chunk> { };
            this.ChunkHeaders = new List<ChunkHeader> { };
            this.SkinningInfo = new SkinningInfo();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load the specified file as a Model
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Model FromFile(String fileName)
        {
            var buffer = new Model();
            buffer.Load(fileName);
            return buffer;
        }

        private void Load(String fileName)
        {
            var inputFile = new FileInfo(fileName);

            Console.Title = String.Format("Processing {0}...", inputFile.Name);

            this.FileName = inputFile.Name;

            if (!inputFile.Exists)
                throw new FileNotFoundException();

            BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open));
            // Get the header.  This isn't essential for .cgam files, but we need this info to find the version and offset to the chunk table
            this.ReadFileHeader(reader);
            this.ReadChunkHeaders(reader);
            this.ReadChunks(reader);

            reader.Dispose();
        }

        #endregion

        private void ReadFileHeader(BinaryReader b)
        {
            b.BaseStream.Seek(0, 0);
            this.FileSignature = b.ReadFString(4);

            if (this.FileSignature == "CrCh")           // file signature v3.6+
            {
                this.FileVersion = (FileVersionEnum)b.ReadUInt32();    // 0x746
                this.NumChunks = b.ReadUInt32();       // number of Chunks in the chunk table
                this.ChunkTableOffset = b.ReadInt32(); // location of the chunk table

                return;
            }

            b.BaseStream.Seek(0, 0);
            this.FileSignature = b.ReadFString(8);

            if (this.FileSignature == "CryTek")         // file signature v3.5-
            {
                this.FileType = (FileTypeEnum)b.ReadUInt32();
                this.FileVersion = (FileVersionEnum)b.ReadUInt32();    // 0x744 0x745
                this.ChunkTableOffset = b.ReadInt32() + 4;
                this.NumChunks = b.ReadUInt32();       // number of Chunks in the chunk table

                return;
            }

            throw new NotSupportedException(String.Format("Unsupported FileSignature {0}", this.FileSignature));
        }

        private void ReadChunkHeaders(BinaryReader b)
        {
            // need to seek to the start of the table here.  foffset points to the start of the table
            b.BaseStream.Seek(this.ChunkTableOffset, SeekOrigin.Begin);

            for (Int32 i = 0; i < this.NumChunks; i++)
            {
                ChunkHeader header = Chunk.New<ChunkHeader>((uint)this.FileVersion);
                header.Read(b);
                this._chunks.Add(header);
            }
        }

        private void ReadChunks(BinaryReader reader)
        {
            foreach (ChunkHeader chkHdr in this._chunks)
            {
                this.ChunkMap[chkHdr.ID] = Chunk.New(chkHdr.ChunkType, chkHdr.Version);
                this.ChunkMap[chkHdr.ID].Load(this, chkHdr);
                this.ChunkMap[chkHdr.ID].Read(reader);

                // Ensure we read to end of structure
                this.ChunkMap[chkHdr.ID].SkipBytes(reader);

                // TODO: Change this to detect node with ~0 (0xFFFFFFFF) parent ID
                // Assume first node read in Model[0] is root node.  This may be bad if they aren't in order!
                if (chkHdr.ChunkType == ChunkTypeEnum.Node && this.RootNode == null)
                {
                    this.RootNode = this.ChunkMap[chkHdr.ID] as ChunkNode;
                }

                // Add Bones to the model.  We are assuming there is only one CompiledBones chunk per file.
                if (chkHdr.ChunkType == ChunkTypeEnum.CompiledBones || chkHdr.ChunkType == ChunkTypeEnum.CompiledBonesSC)
                {
                    this.Bones = this.ChunkMap[chkHdr.ID] as ChunkCompiledBones;
                    SkinningInfo.HasSkinningInfo = true;
                }
            }
        }

        private void WriteChunkTable()
        {
            Utils.Log(LogLevelEnum.Debug, "*** Chunk Header Table***");
            Utils.Log(LogLevelEnum.Debug, "Chunk Type              Version   ID        Size      Offset    ");
            foreach (ChunkHeader chkHdr in this._chunks)
            {
                Utils.Log(LogLevelEnum.Debug, "{0,-24:X}{1,-10:X}{2,-10:X}{3,-10:X}{4,-10:X}", chkHdr.ChunkType.ToString(), chkHdr.Version, chkHdr.ID, chkHdr.Size, chkHdr.Offset);
            }
            Console.WriteLine("*** Chunk Header Table***");
            Console.WriteLine("Chunk Type              Version   ID        Size      Offset    ");
            foreach (ChunkHeader chkHdr in this._chunks)
            {
                Console.WriteLine("{0,-24:X}{1,-10:X}{2,-10:X}{3,-10:X}{4,-10:X}", chkHdr.ChunkType.ToString(), chkHdr.Version, chkHdr.ID, chkHdr.Size, chkHdr.Offset);
            }
        }

        private void WriteFileHeader()
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
    }
}
