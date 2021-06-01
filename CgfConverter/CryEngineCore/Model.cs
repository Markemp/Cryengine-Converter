using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter.CryEngineCore
{
    public class Model
    {
        /// <summary> The Root of the loaded object </summary>
        public ChunkNode RootNode { get; internal set; }

        /// <summary> Collection of all loaded Chunks </summary>
        public List<ChunkHeader> ChunkHeaders { get; internal set; } = new List<ChunkHeader> { };

        /// <summary> Lookup Table for Chunks, indexed by ChunkID </summary>
        public Dictionary<int, Chunk> ChunkMap { get; internal set; } = new Dictionary<int, Chunk> { };

        /// <summary> The name of the currently processed file </summary>
        public string FileName { get; internal set; }

        /// <summary> The File Signature - CryTek for 3.5 and lower. CrCh for 3.6 and higher. #ivo for some SC files. </summary>
        public string FileSignature { get; internal set; }

        /// <summary> The type of file (geometry or animation) </summary>
        public FileType FileType { get; internal set; }

        public FileVersion FileVersion { get; internal set; }

        /// <summary> Position of the Chunk Header table </summary>
        public int ChunkTableOffset { get; internal set; }

        /// <summary>
        /// Contains all the information about bones and skinning them.  This a reference to the Cryengine object, 
        /// since multiple Models can exist for a single object).
        /// </summary>
        public SkinningInfo SkinningInfo { get; set; } = new SkinningInfo();

        /// <summary> The Bones in the model.  The CompiledBones chunk will have a unique RootBone. </summary>
        public ChunkCompiledBones Bones { get; internal set; }

        public uint NumChunks { get; internal set; }

        private Dictionary<int, ChunkNode> nodeMap { get; set; }

        /// <summary> Node map for this model only. </summary>
        public Dictionary<int, ChunkNode> NodeMap      // This isn't right.  Nodes can have duplicate names.
        {
            get
            {
                if (nodeMap == null)
                {
                    nodeMap = new Dictionary<int, ChunkNode>() { };
                    ChunkNode rootNode = null;
                    RootNode = rootNode = (rootNode ?? RootNode);  // Each model will have it's own rootnode.

                    foreach (ChunkNode node in ChunkMap.Values.Where(c => c.ChunkType == ChunkType.Node).Select(c => c as ChunkNode))
                    {
                        // Preserve existing parents
                        if (nodeMap.ContainsKey(node.ID))
                        {
                            ChunkNode parentNode = nodeMap[node.ID].ParentNode;

                            if (parentNode != null)
                                parentNode = nodeMap[parentNode.ID];

                            node.ParentNode = parentNode;
                        }

                        nodeMap[node.ID] = node;
                    }
                }
                return nodeMap;
            }
        }

        #region Private Fields

        public List<ChunkHeader> chunkHeaders = new List<ChunkHeader> { };

        #endregion

        #region Calculated Properties

        public int NodeCount { get { return ChunkMap.Values.Where(c => c.ChunkType == ChunkType.Node).Count(); } }

        public int BoneCount { get { return ChunkMap.Values.Where(c => c.ChunkType == ChunkType.CompiledBones).Count(); } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load the specified file as a Model
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Model FromFile(string fileName)
        {
            var buffer = new Model();
            buffer.Load(fileName);
            return buffer;
        }

        private void Load(string fileName)
        {
            var inputFile = new FileInfo(fileName);

            Console.Title = string.Format("Processing {0}...", inputFile.Name);

            FileName = inputFile.Name;

            if (!inputFile.Exists)
                throw new FileNotFoundException();

            BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open));
            // Get the header.  This isn't essential for .cgam files, but we need this info to find the version and offset to the chunk table
            ReadFileHeader(reader);
            ReadChunkHeaders(reader);
            ReadChunks(reader);

            reader.Dispose();
        }

        #endregion

        private void ReadFileHeader(BinaryReader b)
        {
            b.BaseStream.Seek(0, 0);
            FileSignature = b.ReadFString(4);

            if (FileSignature == "CrCh")           // file signature v3.6+
            {
                FileVersion = (FileVersion)b.ReadUInt32();    // 0x746
                NumChunks = b.ReadUInt32();       // number of Chunks in the chunk table
                ChunkTableOffset = b.ReadInt32(); // location of the chunk table

                return;
            } 
            else if (FileSignature == "#ivo")
            {
                FileVersion = (FileVersion)b.ReadUInt32();  // 0x0900
                NumChunks = b.ReadUInt32();
                ChunkTableOffset = b.ReadInt32();
                CreateDummyRootNode();
                return;
            }

            b.BaseStream.Seek(0, 0);
            FileSignature = b.ReadFString(8);

            if (FileSignature == "CryTek")         // file signature v3.5-
            {
                FileType = (FileType)b.ReadUInt32();
                FileVersion = (FileVersion)b.ReadUInt32();    // 0x744 0x745
                ChunkTableOffset = b.ReadInt32() + 4;
                NumChunks = b.ReadUInt32();       // number of Chunks in the chunk table

                return;
            }

            throw new NotSupportedException(string.Format("Unsupported FileSignature {0}", FileSignature));
        }

        private void CreateDummyRootNode()
        {
            ChunkNode rootNode = new ChunkNode_823();
            rootNode.Name = FileName;
            rootNode.ObjectNodeID = 1;      // No node IDs in #ivo files
            rootNode.ParentNodeID = ~0;     // No parent
            rootNode.__NumChildren = 0;     // Single object
            rootNode.MatID = 0;
            rootNode.Transform = Matrix44.CreateDefaultRootNodeMatrix();
            rootNode.ChunkType = ChunkType.Node;
            RootNode = rootNode;
        }

        private void ReadChunkHeaders(BinaryReader b)
        {
            b.BaseStream.Seek(ChunkTableOffset, SeekOrigin.Begin);

            for (int i = 0; i < NumChunks; i++)
            {
                ChunkHeader header = Chunk.New<ChunkHeader>((uint)FileVersion);
                header.Read(b);
                chunkHeaders.Add(header);
            }
        }

        private void ReadChunks(BinaryReader reader)
        {
            foreach (ChunkHeader chunkHeaderItem in chunkHeaders)
            {
                ChunkMap[chunkHeaderItem.ID] = Chunk.New(chunkHeaderItem.ChunkType, chunkHeaderItem.Version);
                ChunkMap[chunkHeaderItem.ID].Load(this, chunkHeaderItem);
                ChunkMap[chunkHeaderItem.ID].Read(reader);

                // Ensure we read to end of structure
                ChunkMap[chunkHeaderItem.ID].SkipBytes(reader);

                // Assume first node read in Model[0] is root node.  This may be bad if they aren't in order!
                if (chunkHeaderItem.ChunkType == ChunkType.Node && RootNode == null)
                {
                    RootNode = ChunkMap[chunkHeaderItem.ID] as ChunkNode;
                }

                // Add Bones to the model.  We are assuming there is only one CompiledBones chunk per file.
                if (chunkHeaderItem.ChunkType == ChunkType.CompiledBones || 
                    chunkHeaderItem.ChunkType == ChunkType.CompiledBonesSC ||
                    chunkHeaderItem.ChunkType == ChunkType.CompiledBonesIvo)
                {
                    Bones = ChunkMap[chunkHeaderItem.ID] as ChunkCompiledBones;
                    SkinningInfo.HasSkinningInfo = true;
                }
            }
        }
    }
}
