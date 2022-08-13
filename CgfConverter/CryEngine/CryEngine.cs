using CgfConverter.CryEngineCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter;

public partial class CryEngine
{
    public string MtlFile { get; private set; }  // TODO: A model can support multiple material files.

    private const string invalidExtensionErrorMessage = "Warning: Unsupported file extension - please use a cga, cgf, chr or skin file.";

    private static readonly HashSet<string> validExtensions = new()
    {
        ".cgf",
        ".cga",
        ".chr",
        ".skin",
        ".anim",
        ".soc"
    };

    public List<Model> Models { get; internal set; } = new ();
    public List<Material> Materials { get; internal set; } = new ();
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
                _chunks = Models.SelectMany(m => m.ChunkMap.Values).ToList();

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

                Utils.Log(LogLevelEnum.Debug, "Mapping Nodes");

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
        var inputFile = new FileInfo(InputFile);
        var inputFiles = new List<FileInfo> { inputFile };

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

        // Create materials
        CreateMaterials();

        // Get the material file name
        var allMaterialChunks = Models
            .SelectMany(a => a.ChunkMap.Values)
            .Where(c => c.ChunkType == ChunkType.MtlName || c.ChunkType == ChunkType.MtlNameIvo);

        foreach (ChunkMtlName mtlChunk in allMaterialChunks)
        {
            // Don't process child or collision materials for now
            if (mtlChunk.MatType == MtlNameType.Child || mtlChunk.MatType == MtlNameType.Unknown1)
                continue;

            
        }

        Utils.Log(LogLevelEnum.Debug, "Unable to locate any material file.  Creating Default materials.");

        // Create dummy materials
        // 0x800:  1 material per mtlname chunk.  If NFlags1 = 1, it's the library. Skip
        // 0x802:  Mult materials per chunk. Can't get names except in certain types (null separated
        //         strings at end of mtlname chunk, but can't identify these). create dummy names.
        // Only use first mtlname chunks from first model.  2nd model has duplicates
        foreach (ChunkMtlName mtlChunk in Models[0].ChunkMap.Values
            .Where(c => c.ChunkType == ChunkType.MtlName || c.ChunkType == ChunkType.MtlNameIvo))
        {
            switch (mtlChunk.Version)
            {
                case 0x744:
                    break;
                case 0x800:
                case 0x80000800:
                    if (mtlChunk.MatType != MtlNameType.Library)
                        Materials.Add(Material.CreateDefaultMaterial(mtlChunk.Name));
                    break;
                case 0x802:
                    for (int i = 0; i < mtlChunk.NumChildren; i++)
                    {
                        Materials.Add(Material.CreateDefaultMaterial(mtlChunk.Name + i.ToString()));
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private static void AutoDetectMFile(string filename, FileInfo inputFile, List<FileInfo> inputFiles)
    {
        FileInfo mFile = new(Path.ChangeExtension(filename, string.Format("{0}m", inputFile.Extension)));

        if (mFile.Exists)
        {
            Utils.Log(LogLevelEnum.Debug, "Found geometry file {0}", mFile.Name);
            inputFiles.Add(mFile);
        }
    }

    private static SkinningInfo ConsolidateSkinningInfo(List<Model> models)
    {
        SkinningInfo skin = new()
        {
            HasSkinningInfo = models.Any(a => a.SkinningInfo.HasSkinningInfo == true),
            HasBoneMapDatastream = models.Any(a => a.SkinningInfo.HasBoneMapDatastream == true)
        };

        foreach (Model model in models)
        {
            if (model.SkinningInfo.IntFaces != null)
                skin.IntFaces = model.SkinningInfo.IntFaces;

            if (model.SkinningInfo.IntVertices != null)
                skin.IntVertices = model.SkinningInfo.IntVertices;

            if (model.SkinningInfo.LookDirectionBlends != null)
                skin.LookDirectionBlends = model.SkinningInfo.LookDirectionBlends;

            if (model.SkinningInfo.MorphTargets != null)
                skin.MorphTargets = model.SkinningInfo.MorphTargets;

            if (model.SkinningInfo.PhysicalBoneMeshes != null)
                skin.PhysicalBoneMeshes = model.SkinningInfo.PhysicalBoneMeshes;

            if (model.SkinningInfo.BoneEntities != null)
                skin.BoneEntities = model.SkinningInfo.BoneEntities;

            if (model.SkinningInfo.BoneMapping != null)
                skin.BoneMapping = model.SkinningInfo.BoneMapping;

            if (model.SkinningInfo.Collisions != null)
                skin.Collisions = model.SkinningInfo.Collisions;

            if (model.SkinningInfo.CompiledBones != null)
                skin.CompiledBones = model.SkinningInfo.CompiledBones;

            if (model.SkinningInfo.Ext2IntMap != null)
                skin.Ext2IntMap = model.SkinningInfo.Ext2IntMap;
        }

        return skin;
    }

    private void CreateMaterials()
    {
        // Do this by Node chunk.  Won't have to process materials that aren't used in the model.
        foreach (ChunkNode nodeChunk in Chunks.Where(c => c.ChunkType == ChunkType.Node))
        {
            if (Chunks.FirstOrDefault(c => c.ID == nodeChunk.MatID) is not ChunkMtlName matChunk)
            {
                Utils.Log(LogLevelEnum.Debug, $"Unable to find material chunk {nodeChunk.MatID} for node {nodeChunk.ID}");
                continue;
            }

            if (matChunk.MatType != MtlNameType.Child)
            {
                // The material has just a single mat with no submaterials.  Mat file is in same directory?
                FileInfo matfile = GetMaterialFile(matChunk.Name);
            }

        }
    }

    // Gets the material file for Basic, Single and Library types.  Child materials are created from the library.
    private FileInfo? GetMaterialFile(string name)
    {
        if (name.Contains(':'))  // Need an example and test for this case
            name = name.Split(':')[1];

        FileInfo materialFile;

        if (name.Contains("mechDefault.mtl"))
        {
            // For MWO models with a material called "Material #0", which is a default mat used on lots of mwo mechs.
            // TODO: Figure out what the default material actually is and manually create that material.
            materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(InputFile), name));
        }

        if (name.Contains('/') || name.Contains('\\'))
        {
            // The mtlname has a path.  Most likely starts at the Objects directory.
            var stringSeparators = new string[] { @"\", @"/" };
            string[] result;

            var path = Path.Combine(DataDir, name);
            
            // if objectdir is provided, check objectdir + mtlchunk.name
            if (DataDir != null)
                materialFile = new FileInfo(Path.Combine(DataDir, name));
            else
            {
                // object dir not provided, but we have a path. Append to current model directory
                result = name.Split(stringSeparators, StringSplitOptions.None);
                materialFile = new FileInfo(Path.Combine(InputFile, name));
            }
        }
        else
        {
            var charsToClean = name.ToCharArray().Intersect(Path.GetInvalidFileNameChars()).ToArray();
            if (charsToClean.Length > 0)
            {
                foreach (char character in charsToClean)
                {
                    name = name.Replace(character.ToString(), "");
                }
            }
            materialFile = new FileInfo(Path.Combine(Path.GetDirectoryName(InputFile), name));
        }

        materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

        return materialFile;
        //Material material = Material.FromFile(materialFile);

        //if (material != null)
        //{
        //    Utils.Log(LogLevelEnum.Debug, "Located material file {0}", materialFile.Name);

        //    if (Materials.Count == 1 && Materials[0].Name is null)
        //        Materials[0].Name = RootNode.Name;

        //    // Early return - we have the material map
        //    return null;
        //}
        //else
        //{
        //    Utils.Log(LogLevelEnum.Debug, "Unable to locate material file {0}.mtl", name);
        //    return null;
        //}
    }
}