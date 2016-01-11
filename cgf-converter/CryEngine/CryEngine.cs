using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter
{
    public partial class CryEngine
    {
        /// <summary>
        /// File extensions processed by CryEngine
        /// </summary>
        private static HashSet<String> _validExtensions = new HashSet<String>
        {
            ".cgf",
            ".cga",
            ".skin",
        };

        #region Constructors

        public Model.ChunkNode RootNode { get; private set; }

        public CryEngine(String fileName, String dataDir)
        {
            FileInfo inputFile = new FileInfo(fileName);
            List<FileInfo> inputFiles = new List<FileInfo> { inputFile };

            // Validate file extension - handles .cgam / skinm
            if (!CryEngine._validExtensions.Contains(inputFile.Extension))
            {
                Console.WriteLine("Warning: Unsupported file extension - please use a cga, cgf or skin file");
                throw new FileLoadException("Warning: Unsupported file extension - please use a cga, cgf or skin file", fileName);
            }

            #region m-File Auto-Detection

            FileInfo mFile = new FileInfo(Path.ChangeExtension(fileName, String.Format("{0}m", inputFile.Extension)));

            if (mFile.Exists)
            {
                Console.WriteLine("Found mFile file {0}", mFile.Name);

                // Add to list of files to process
                inputFiles.Add(mFile);
            }

            #endregion

            this.Models = new List<Model> { };

            foreach (var file in inputFiles)
            {
                Model model = Model.FromFile(file.FullName);
                this.RootNode = this.RootNode ?? model.RootNode;
                this.Models.Add(model);
            }

            foreach (CryEngine.Model.ChunkMtlName mtlChunk in this.Models.SelectMany(a => a.ChunkMap.Values).Where(c => c.ChunkType == ChunkTypeEnum.MtlName))
            {
                // Don't process child materials for now
                if (mtlChunk.Version == 0x800 && !(mtlChunk.MatType == 0x01 || mtlChunk.MatType == 0x10))
                    continue;

                Console.WriteLine("Found material {0}", mtlChunk.Name);

                // First try relative to file being processed
                FileInfo materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(fileName), mtlChunk.Name));
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try relative to the ObjectDir
                if (!materialFile.Exists)
                    materialFile = new FileInfo(Path.Combine(dataDir, mtlChunk.Name));
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try just the fileName.mtl
                if (!materialFile.Exists)
                    materialFile = new FileInfo(fileName);
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // TODO: Try more paths

                CryEngine.Material material = CryEngine.Material.FromFile(materialFile);

                if (material != null)
                {
                    this.Materials = this.FlattenMaterials(material).Skip(1).ToArray();
                    // UInt32 i = 0;
                    // this.MaterialMap = this.Materials.Skip(1).ToArray(); // .ToDictionary(k => ++i, v => v);

                    // Early return - we have the material map
                    return;
                }
            }

            this.Materials = new Material[] { };
        }

        #endregion

        #region Properties

        public List<Model> Models { get; private set; }
        public Material[] Materials { get; private set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Flatten all child materials into a one dimensional list
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        private IEnumerable<Material> FlattenMaterials(Material material)
        {
            if (material != null)
            {
                yield return material;

                if (material.SubMaterials != null)
                    foreach (var subMaterial in material.SubMaterials.SelectMany(m => this.FlattenMaterials(m)))
                        yield return subMaterial;
            }
        }

        private Model.Chunk[] _chunks;
        public Model.Chunk[] Chunks
        {
            get
            {
                if (this._chunks == null)
                {
                    this._chunks = this.Models.SelectMany(m => m.ChunkMap.Values).ToArray();
                }

                return this._chunks;
            }
        }

        public Dictionary<UInt32, Model.Chunk> _chunksByID;
        public Dictionary<UInt32, Model.Chunk> ChunksByID
        {
            get
            {
                if (this._chunksByID == null)
                {
                    this._chunksByID = new Dictionary<UInt32, Model.Chunk> { };
                    foreach (Model.Chunk chunk in this.Chunks)
                    {
                        this._chunksByID[chunk.ID] = chunk;
                    }
                }

                return this._chunksByID;
            }
        }

        public Dictionary<String, Model.ChunkNode> _nodeMap;
        public Dictionary<String, Model.ChunkNode> NodeMap
        {
            get
            {
                if (this._nodeMap == null)
                {
                    this._nodeMap = new Dictionary<String, Model.ChunkNode>(StringComparer.InvariantCultureIgnoreCase) { };
                    foreach (Model model in this.Models)
                    {
                        foreach (Model.ChunkNode node in model.ChunkMap.Values.Where(c => c.ChunkType == ChunkTypeEnum.Node).Select(c => c as Model.ChunkNode))
                        {
                            if (node.ParentNodeID == 0xFFFFFFFF && this._nodeMap.ContainsKey(node.Name))
                            {
                                node.ParentNode = this._nodeMap[node.Name].ParentNode;
                                node.ParentNodeID = this._nodeMap[node.Name].ParentNodeID;
                                this._nodeMap[node.Name] = node;
                            }
                            else
                            {
                                this._nodeMap[node.Name] = node;
                            }
                        }
                    }
                }

                return this._nodeMap;
            }
        }

        #endregion

        public uint CurrentVertexPosition { get; set; }

        public uint TempIndicesPosition { get; set; }

        public uint TempVertexPosition { get; set; }

        public uint CurrentIndicesPosition { get; set; }
    }
}
