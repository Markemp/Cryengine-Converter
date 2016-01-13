using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            ".chr",
            ".skin",
        };

        #region Constructors

        public CryEngine_Core.ChunkNode RootNode { get; internal set; }

        public String InputFile { get; internal set; }

        public CryEngine(String fileName, String dataDir)
        {
            this.InputFile = fileName;

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

            foreach (CryEngine_Core.ChunkMtlName mtlChunk in this.Models.SelectMany(a => a.ChunkMap.Values).Where(c => c.ChunkType == ChunkTypeEnum.MtlName))
            {
                // Don't process child materials for now
                if (mtlChunk.Version == 0x800 && !(mtlChunk.MatType == 0x01 || mtlChunk.MatType == 0x10))
                    continue;

                // First try relative to file being processed
                FileInfo materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(fileName), mtlChunk.Name));
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try just the last part of the chunk, relative to the file being processed
                if (!materialFile.Exists)
                    materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileName(mtlChunk.Name)));
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
                    Console.WriteLine("Located material file {0}", materialFile.Name);

                    this.Materials = this.FlattenMaterials(material).Skip(1).ToArray();
                    // UInt32 i = 0;
                    // this.MaterialMap = this.Materials.Skip(1).ToArray(); // .ToDictionary(k => ++i, v => v);

                    // Early return - we have the material map
                    return;
                }
                else
                {
                    // Use original name, as that's what we were originally trying to locate
                    Console.WriteLine("Unable to locate material file {0}.mtl", mtlChunk.Name);
                }
            }

            Console.WriteLine("Unable to locate any material file");

            this.Materials = new Material[] { };
        }

        #endregion

        #region Properties

        public List<Model> Models { get; internal set; }
        public Material[] Materials { get; internal set; }

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

        private CryEngine_Core.Chunk[] _chunks;
        public CryEngine_Core.Chunk[] Chunks
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

        public Dictionary<UInt32, CryEngine_Core.Chunk> _chunksByID;
        public Dictionary<UInt32, CryEngine_Core.Chunk> ChunksByID
        {
            get
            {
                if (this._chunksByID == null)
                {
                    this._chunksByID = new Dictionary<UInt32, CryEngine_Core.Chunk> { };

                    foreach (CryEngine_Core.Chunk chunk in this.Chunks)
                    {
                        this._chunksByID[chunk.ID] = chunk;
                    }
                }

                return this._chunksByID;
            }
        }

        private static HashSet<UInt32> _watchedNodeIDs = new HashSet<UInt32>
        {
            0x2228,
            0x2234,
            0x2294,
            0x2300,
            0x506,
            0x512,
            0x560,
            0x566,
        };

        private static HashSet<String> _watchedNodes = new HashSet<String>
        {
            // Misbehaving
            // "LG_Hatch_Aft_Inboard_Left",
            // "LG_Hatch_Aft_Inboard_Right",
            // "LG_Hatch_Aft_Outboard_Left",
            // "LG_Hatch_Aft_Outboard_Right",
            // "LG_Hatch_Front_Center_Left",
            // "LG_Hatch_Front_Center_Right",
            // "LG_Hatch_Front_Inboard_Left",
            // "LG_Hatch_Front_Inboard_Right",
            // "LG_Hatch_Front_Outboard_Left",
            // "LG_Hatch_Front_Outboard_Right",
            "LG_Skid_Aft_Right",
            // "LG_Skid_Front_Right",
            "LG_Skid_Aft_Left",
            // "LG_Skid_Front_Left",

            // Behaving
            // "LG_Arm_Aft_Left",
            // "LG_Arm_Front_Left",
            // "LandingGear_Pod_Aft_Left",
            // "LandingGear_Pod_Aft_Right"
        };

        public Dictionary<String, CryEngine_Core.ChunkNode> _nodeMap;
        public Dictionary<String, CryEngine_Core.ChunkNode> NodeMap
        {
            get
            {
                if (this._nodeMap == null)
                {
                    this._nodeMap = new Dictionary<String, CryEngine_Core.ChunkNode>(StringComparer.InvariantCultureIgnoreCase) { };

                    CryEngine_Core.ChunkNode rootNode = null;

                    Debug.WriteLine("Mapping Nodes");

                    foreach (Model model in this.Models)
                    {
                        model.RootNode = rootNode = (rootNode ?? model.RootNode);

                        foreach (CryEngine_Core.ChunkNode node in model.ChunkMap.Values.Where(c => c.ChunkType == ChunkTypeEnum.Node).Select(c => c as CryEngine_Core.ChunkNode))
                        {
                            // Preserve existing parents
                            if (this._nodeMap.ContainsKey(node.Name))
                            {
                                CryEngine_Core.ChunkNode parentNode = this._nodeMap[node.Name].ParentNode;

                                if (parentNode != null)
                                    parentNode = this._nodeMap[parentNode.Name];

                                node.ParentNode = parentNode;
                            }

                            this._nodeMap[node.Name] = node;

                            
#if DUMP_JSON
                            File.WriteAllText(String.Format("_node-{0}{1}.json", node.Name, Path.GetExtension(model.FileName)), node.ToJSON());
#endif

                        }
                    }
                }

                return this._nodeMap;
            }
        }

        #endregion
    }
}