using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter
{
    // TODO: Move this to CryEngine_Core
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
                Utils.Log(LogLevelEnum.Debug, "Warning: Unsupported file extension - please use a cga, cgf, chr or skin file");
                throw new FileLoadException("Warning: Unsupported file extension - please use a cga, cgf, chr or skin file", fileName);
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

            this.Models = new List<CryEngine_Core.Model> { };

            foreach (var file in inputFiles)
            {
                CryEngine_Core.Model model = CryEngine_Core.Model.FromFile(file.FullName);
                this.RootNode = this.RootNode ?? model.RootNode;
                this.Models.Add(model);
            }

            foreach (CryEngine_Core.ChunkMtlName mtlChunk in this.Models.SelectMany(a => a.ChunkMap.Values).Where(c => c.ChunkType == ChunkTypeEnum.MtlName))
            {
                // Don't process child materials for now
                if (mtlChunk.MatType == MtlNameTypeEnum.Child)
                    continue;

                String cleanName = mtlChunk.Name;
                Console.WriteLine("CleanName is {0}", mtlChunk.Name);

                // TODO: Investigate if we want to clean paths nicer
                // TODO: Modify this so mtlfile chunks with slashes (object/mechs/adder/cockpit/adr_cockpit.mtl) work properly
                FileInfo materialFile;

                if (mtlChunk.Name.Contains(@"/") || mtlChunk.Name.Contains(@"\"))
                {
                    // The mtlname has a path.  Most likely starts at the Objects directory.
                    // 
                    string[] stringSeparators = new string[] { @"\", @"/" };
                    string[] result;
                    // if objectdir is provided, check objectdir + mtlchunk.name
                    if (dataDir != null)
                    {
                        materialFile = new FileInfo(Path.Combine(dataDir, mtlChunk.Name));
                    } else
                    {
                        // object dir not provided, but we have a path.  Just grab the last part of the name and check the dir of the cga file
                        result = mtlChunk.Name.Split(stringSeparators, StringSplitOptions.None);
                        materialFile = new FileInfo(result[result.Length - 1]);
                    }
                } else
                {
                    var charsToClean = cleanName.ToCharArray().Intersect(Path.GetInvalidFileNameChars()).ToArray();
                    if (charsToClean.Length > 0)
                    {
                        foreach(Char character in charsToClean)
                        {
                            cleanName = cleanName.Replace(character.ToString(), "");
                        }
                    }
                    materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(fileName), cleanName));
                }
                // First try relative to file being processed
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try just the last part of the chunk, relative to the file being processed
                if (!materialFile.Exists)
                    materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileName(cleanName)));
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try relative to the ObjectDir
                if (!materialFile.Exists && dataDir != null)
                    materialFile = new FileInfo(Path.Combine(dataDir, cleanName));
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try just the fileName.mtl
                if (!materialFile.Exists)
                    materialFile = new FileInfo(fileName);
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // TODO: Try more paths

                CryEngine_Core.Material material = CryEngine_Core.Material.FromFile(materialFile);

                if (material != null)
                {
                    // Utils.Log(LogLevelEnum.Debug, "Located material file {0}", materialFile.Name);

                    this.Materials = CryEngine.FlattenMaterials(material).Skip(1).ToArray();

                    // Early return - we have the material map
                    return;
                }
                else
                {
                    // Utils.Log(LogLevelEnum.Debug, "Unable to locate material file {0}.mtl", mtlChunk.Name);
                }
            }

            // Utils.Log(LogLevelEnum.Debug, "Unable to locate any material file");

            this.Materials = new CryEngine_Core.Material[] { };
        }

        #endregion

        #region Properties

        public List<CryEngine_Core.Model> Models { get; internal set; }
        public CryEngine_Core.Material[] Materials { get; internal set; }

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

                    foreach (CryEngine_Core.Model model in this.Models)
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

        /// <summary>
        /// Flatten all child materials into a one dimensional list
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public static IEnumerable<CryEngine_Core.Material> FlattenMaterials(CryEngine_Core.Material material)
        {
            if (material != null)
            {
                yield return material;

                if (material.SubMaterials != null)
                    foreach (var subMaterial in material.SubMaterials.SelectMany(m => CryEngine.FlattenMaterials(m)))
                        yield return subMaterial;
            }
        }

        #region Private Methods



        #endregion
    }
}