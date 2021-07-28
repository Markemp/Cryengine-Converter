using CgfConverter.CryEngineCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter
{
    public partial class CryEngine
    {
        public string MtlFile { get; private set; }

        private const string invalidExtensionErrorMessage = "Warning: Unsupported file extension - please use a cga, cgf, chr or skin file.";

        private static readonly HashSet<string> validExtensions = new HashSet<string>
        {
            ".cgf",
            ".cga",
            ".chr",
            ".skin",
            ".anim",
            ".soc"
        };
        public List<Model> Models { get; internal set; } = new List<Model>();
        public List<Material> Materials { get; internal set; } = new List<Material>();
        public ChunkNode RootNode { get; internal set; }
        public ChunkCompiledBones Bones { get; internal set; }
        public SkinningInfo SkinningInfo { get; set; }
        public string InputFile { get; internal set; }
        public string DataDir { get; internal set; }
        public List<Chunk> Chunks
        {
            get
            {
                if (_chunks == null)
                {
                    _chunks = Models.SelectMany(m => m.ChunkMap.Values).ToList();
                }

                return _chunks;
            }
        }
        public Dictionary<string, ChunkNode> NodeMap  // Cannot use the Node name for the key.  Across a couple files, you may have multiple nodes with same name.
        {
            get
            {
                if (_nodeMap == null)
                {
                    _nodeMap = new Dictionary<string, ChunkNode>(StringComparer.InvariantCultureIgnoreCase) { };

                    ChunkNode rootNode = null;

                    Utils.Log(LogLevelEnum.Info, "Mapping Nodes");

                    foreach (Model model in Models)
                    {
                        model.RootNode = rootNode = (rootNode ?? model.RootNode);  // Each model will have it's own rootnode.

                        foreach (ChunkNode node in model.ChunkMap.Values.Where(c => c.ChunkType == ChunkType.Node).Select(c => c as ChunkNode))
                        {
                            // Preserve existing parents
                            if (_nodeMap.ContainsKey(node.Name))
                            {
                                ChunkNode parentNode = _nodeMap[node.Name].ParentNode;

                                if (parentNode != null)
                                    parentNode = _nodeMap[parentNode.Name];

                                node.ParentNode = parentNode;
                            }

                            _nodeMap[node.Name] = node;    // TODO:  fix this.  The node name can conflict.
                        }
                    }
                }

                return _nodeMap;
            }
        }

        private List<Chunk> _chunks;
        private Dictionary<string, ChunkNode> _nodeMap;

        public CryEngine(string filename, string dataDir)
        {
            InputFile = filename;
            DataDir = dataDir;
        }

        public void ProcessCryengineFiles()
        {
            FileInfo inputFile = new FileInfo(InputFile);
            List<FileInfo> inputFiles = new List<FileInfo> { inputFile };
            
            if (!validExtensions.Contains(inputFile.Extension))
            {
                Utils.Log(LogLevelEnum.Debug, invalidExtensionErrorMessage);
                throw new FileLoadException(invalidExtensionErrorMessage, InputFile);
            }

            AutoDetectMFile(InputFile, inputFile, inputFiles);
                
            foreach (var file in inputFiles)
            {
                // Each file (.cga and .cgam if applicable) will have its own RootNode.  
                // This can cause problems.  .cga files with a .cgam files won't have geometry for the one root node.
                Model model = Model.FromFile(file.FullName);
                if (RootNode == null)
                    RootNode = model.RootNode;  // This makes the assumption that we read the .cga file before the .cgam file.

                Bones = Bones ?? model.Bones;
                Models.Add(model);
            }

            SkinningInfo = ConsolidateSkinningInfo(Models);

            // Get the material file name
            var allMaterialChunks = Models
                .SelectMany(a => a.ChunkMap.Values)
                .Where(c => c.ChunkType == ChunkType.MtlName || c.ChunkType == ChunkType.MtlNameIvo);

            foreach (ChunkMtlName mtlChunk in allMaterialChunks)
            {
                // Don't process child or collision materials for now
                if (mtlChunk.MatType == MtlNameType.Child || mtlChunk.MatType == MtlNameType.Unknown1)
                    continue;

                if (mtlChunk.Name.Contains(":"))
                {
                    string[] parts = mtlChunk.Name.Split(':');
                    mtlChunk.Name = parts[1];
                }

                string cleanName = mtlChunk.Name;
                FileInfo materialFile;

                if (mtlChunk.Name.Contains("default_body"))
                {
                    // New MWO models for some crazy reason don't put the actual mtl file name in the mtlchunk.  They just have /objects/mechs/default_body
                    // have to assume that it's /objects/mechs/<mechname>/body/<mechname>_body.mtl.  There is also a <mechname>.mtl that contains mtl 
                    // info for hitboxes, but not needed.
                    // TODO:  This isn't right.  Fix it.
                    var charsToClean = cleanName.ToCharArray().Intersect(Path.GetInvalidFileNameChars()).ToArray();
                    if (charsToClean.Length > 0)
                    {
                        foreach (char character in charsToClean)
                        {
                            cleanName = cleanName.Replace(character.ToString(), "");
                        }
                    }
                    materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(InputFile), cleanName));
                }
                else if (mtlChunk.Name.Contains(@"/") || mtlChunk.Name.Contains(@"\"))
                {
                    // The mtlname has a path.  Most likely starts at the Objects directory.
                    string[] stringSeparators = new string[] { @"\", @"/" };
                    string[] result;

                    // if objectdir is provided, check objectdir + mtlchunk.name
                    if (DataDir != null)
                        materialFile = new FileInfo(Path.Combine(DataDir, mtlChunk.Name));
                    else
                    {
                        // object dir not provided, but we have a path.  Just grab the last part of the name and check the dir of the cga file
                        result = mtlChunk.Name.Split(stringSeparators, StringSplitOptions.None);
                        materialFile = new FileInfo(result[result.Length - 1]);
                    }
                }
                else
                {
                    var charsToClean = cleanName.ToCharArray().Intersect(Path.GetInvalidFileNameChars()).ToArray();
                    if (charsToClean.Length > 0)
                    {
                        foreach (char character in charsToClean)
                        {
                            cleanName = cleanName.Replace(character.ToString(), "");
                        }
                    }
                    materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(InputFile), cleanName));
                }

                // First try relative to file being processed
                if (materialFile.Extension != ".mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try just the last part of the chunk, relative to the file being processed
                if (!materialFile.Exists)
                    materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(InputFile), Path.GetFileName(cleanName)));
                if (materialFile.Extension != ".mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try relative to the ObjectDir
                if (!materialFile.Exists && DataDir != null)
                    materialFile = new FileInfo(Path.Combine(DataDir, cleanName));
                if (materialFile.Extension != ".mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try just the fileName.mtl
                if (!materialFile.Exists)
                    materialFile = new FileInfo(InputFile);
                if (materialFile.Extension != ".mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                if (materialFile.Extension == ".mtl")
                    MtlFile = materialFile.Name;

                Material material = Material.FromFile(materialFile);

                if (material != null)
                {
                    Utils.Log(LogLevelEnum.Debug, "Located material file {0}", materialFile.Name);

                    Materials = FlattenMaterials(material).Where(m => m.Textures != null).ToList();

                    if (Materials.Count == 1)
                    {
                        // only one material, so it's a material file with no submaterials.  Check and set the name
                        Materials[0].Name = RootNode.Name;
                    }

                    // Early return - we have the material map
                    return;
                }
                else
                {
                    Utils.Log(LogLevelEnum.Debug, "Unable to locate material file {0}.mtl", mtlChunk.Name);
                }
            }

            Utils.Log(LogLevelEnum.Debug, "Unable to locate any material file.  Creating Default materials.");

            foreach (ChunkMtlName mtlChunk in allMaterialChunks)
            {
                //if (mtlChunk.MatType != MtlNameType.Library)
                //{
                Materials.Add(Material.CreateDefaultMaterial(mtlChunk.Name));
                //}
            }
        }

        private void AutoDetectMFile(string filename, FileInfo inputFile, List<FileInfo> inputFiles)
        {
            FileInfo mFile = new FileInfo(Path.ChangeExtension(filename, string.Format("{0}m", inputFile.Extension)));

            if (mFile.Exists)
            {
                Utils.Log(LogLevelEnum.Debug, "Found geometry file {0}", mFile.Name);
                inputFiles.Add(mFile);
            }
        }

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
                    foreach (var subMaterial in material.SubMaterials.SelectMany(m => FlattenMaterials(m)))
                        yield return subMaterial;
            }
        }

        private SkinningInfo ConsolidateSkinningInfo(List<Model> models)
        {
            SkinningInfo skin = new SkinningInfo
            {
                HasSkinningInfo = models.Any(a => a.SkinningInfo.HasSkinningInfo == true),
                HasBoneMapDatastream = models.Any(a => a.SkinningInfo.HasBoneMapDatastream == true)
            };

            foreach (Model model in models)
            {
                if (model.SkinningInfo.IntFaces != null)
                {
                    skin.IntFaces = model.SkinningInfo.IntFaces;
                }
                if (model.SkinningInfo.IntVertices != null)
                {
                    skin.IntVertices = model.SkinningInfo.IntVertices;
                }
                if (model.SkinningInfo.LookDirectionBlends != null)
                {
                    skin.LookDirectionBlends = model.SkinningInfo.LookDirectionBlends;
                }
                if (model.SkinningInfo.MorphTargets != null)
                {
                    skin.MorphTargets = model.SkinningInfo.MorphTargets;
                }
                if (model.SkinningInfo.PhysicalBoneMeshes != null)
                {
                    skin.PhysicalBoneMeshes = model.SkinningInfo.PhysicalBoneMeshes;
                }
                if (model.SkinningInfo.BoneEntities != null)
                {
                    skin.BoneEntities = model.SkinningInfo.BoneEntities;
                }
                if (model.SkinningInfo.BoneMapping != null)
                {
                    skin.BoneMapping = model.SkinningInfo.BoneMapping;
                }
                if (model.SkinningInfo.Collisions != null)
                {
                    skin.Collisions = model.SkinningInfo.Collisions;
                }
                if (model.SkinningInfo.CompiledBones != null)
                {
                    skin.CompiledBones = model.SkinningInfo.CompiledBones;
                }
                if (model.SkinningInfo.Ext2IntMap != null)
                {
                    skin.Ext2IntMap = model.SkinningInfo.Ext2IntMap;
                }
            }
            return skin;
        }
    }
}