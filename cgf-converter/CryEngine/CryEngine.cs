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
                Utils.Log(LogLevelEnum.Debug, "Warning: Unsupported file extension - please use a cga, cgf or skin file");
                throw new FileLoadException("Warning: Unsupported file extension - please use a cga, cgf or skin file", fileName);
            }

            #region m-File Auto-Detection

            FileInfo mFile = new FileInfo(Path.ChangeExtension(fileName, String.Format("{0}m", inputFile.Extension)));

            if (mFile.Exists)
            {
                Utils.Log(LogLevelEnum.Debug, "Found mFile file {0}", mFile.Name);

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
                if (mtlChunk.MatType == MtlNameTypeEnum.Child)
                    continue;

                String cleanName = mtlChunk.Name;

                // TODO: Investigate if we want to clean paths nicer
                var charsToClean = cleanName.ToCharArray().Intersect(Path.GetInvalidFileNameChars()).ToArray();
                if (charsToClean.Length > 0)
                {
                    foreach(Char character in charsToClean)
                    {
                        cleanName = cleanName.Replace(character.ToString(), "");
                    }
                }

                // First try relative to file being processed
                FileInfo materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(fileName), cleanName));
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try just the last part of the chunk, relative to the file being processed
                if (!materialFile.Exists)
                    materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileName(cleanName)));
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try relative to the ObjectDir
                if (!materialFile.Exists)
                    materialFile = new FileInfo(Path.Combine(dataDir, cleanName));
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
                    // Utils.Log(LogLevelEnum.Debug, "Located material file {0}", materialFile.Name);

                    this.Materials = this.FlattenMaterials(material).Skip(1).ToArray();

                    // Early return - we have the material map
                    return;
                }
                else
                {
                    // Utils.Log(LogLevelEnum.Debug, "Unable to locate material file {0}.mtl", mtlChunk.Name);
                }
            }

            // Utils.Log(LogLevelEnum.Debug, "Unable to locate any material file");

            this.Materials = new Material[] { };
        }

        #endregion

        #region Properties

        public List<Model> Models { get; internal set; }
        public Material[] Materials { get; internal set; }

        #endregion

        #region Calculater Properties

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

        public Dictionary<String, CryEngine_Core.ChunkNode> _nodeMap;
        public Dictionary<String, CryEngine_Core.ChunkNode> NodeMap
        {
            get
            {
                if (this._nodeMap == null)
                {
                    this._nodeMap = new Dictionary<String, CryEngine_Core.ChunkNode>(StringComparer.InvariantCultureIgnoreCase) { };

                    CryEngine_Core.ChunkNode rootNode = null;

                    Utils.Log(LogLevelEnum.Info, "Mapping Nodes");

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
                        }
                    }
                }

                return this._nodeMap;
            }
        }

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

        #endregion
    }
}