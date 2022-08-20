using CgfConverter.CryEngineCore;
using CgfConverter.Materials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Material = CgfConverter.Materials.Material;

namespace CgfConverter;

public partial class CryEngine
{
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

    public List<Model> Models { get; internal set; } = new();
    //public List<Material> Materials { get; internal set; } = new();
    public ChunkNode RootNode { get; internal set; }
    public ChunkCompiledBones Bones { get; internal set; }
    public SkinningInfo SkinningInfo { get; set; }
    public string InputFile { get; internal set; }
    public string DataDir { get; internal set; }

    public List<Chunk> Chunks
    {
        get
        {
            if (_chunks is null)
                _chunks = Models.SelectMany(m => m.ChunkMap.Values).ToList();

            return _chunks;
        }
    }

    public Dictionary<string, ChunkNode> NodeMap  // Cannot use the Node name for the key.  Across a couple files, you may have multiple nodes with same name.
    {
        get
        {
            if (_nodeMap is null)
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

                        _nodeMap[node.Name] = node;    // TODO:  fix this.  The node name can conflict. (example?)
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

        CreateMaterials();

        //Utils.Log(LogLevelEnum.Debug, "Unable to locate any material file.  Creating Default materials.");

        //// Create dummy materials
        //// 0x800:  1 material per mtlname chunk.  If NFlags1 = 1, it's the library. Skip
        //// 0x802:  Mult materials per chunk. Can't get names except in certain types (null separated
        ////         strings at end of mtlname chunk, but can't identify these). create dummy names.
        //// Only use first mtlname chunks from first model.  2nd model has duplicates
        //foreach (ChunkMtlName mtlChunk in Models[0].ChunkMap.Values
        //    .Where(c => c.ChunkType == ChunkType.MtlName || c.ChunkType == ChunkType.MtlNameIvo))
        //{
        //    switch (mtlChunk.Version)
        //    {
        //        case 0x744:
        //            break;
        //        case 0x800:
        //        case 0x80000800:
        //            if (mtlChunk.MatType != MtlNameType.Library)
        //                Materials.Add(MaterialUtilities.CreateDefaultMaterial(mtlChunk.Name));
        //            break;
        //        case 0x802:
        //            for (int i = 0; i < mtlChunk.NumChildren; i++)
        //            {
        //                Materials.Add(MaterialUtilities.CreateDefaultMaterial(mtlChunk.Name + i.ToString()));
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //}
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

            var matfile = GetMaterialFile(matChunk.Name);
            if (matfile is not null)
            {
                var mats = CreateMaterialsFromMatFile(matfile);
                nodeChunk.Materials = mats;
            } // Default mats if material file cannot be found.
        }
    }

    private static Material? CreateMaterialsFromMatFile(FileInfo matfile)
    {
        var materials = MaterialUtilities.FromFile(matfile.FullName);
        return materials;
    }

    // Gets the material file for Basic, Single and Library types.  Child materials are created from the library.
    private FileInfo? GetMaterialFile(string name)
    {
        if (name.Contains(':'))  // Need an example and test for this case.  Probably a child material?
            name = name.Split(':')[1];

        if (!name.EndsWith(".mtl"))
            name += ".mtl";

        FileInfo materialFile;
        var inputFileInfo = new FileInfo(InputFile);

        if (name.Contains("mechDefault.mtl"))
        {
            // For MWO models with a material called "Material #0", which is a default mat used on lots of mwo mechs.
            // The actual material file is in objects\mechs\generic\body\generic_body.mtl
            name = "objects\\mechs\\generic\\body\\generic_body.mtl";
            materialFile = new FileInfo(Path.Combine(inputFileInfo.Directory.FullName, name));
            if (materialFile.Exists)
                return materialFile;
        }
        else
        {
            // Check if material file is in or relative to current directory
            materialFile = new FileInfo(Path.Combine(inputFileInfo.Directory.FullName, name));
            if (materialFile.Exists)
                return materialFile;

            // Check if material file relative to object directory
            materialFile = new FileInfo(Path.Combine(DataDir, name));
            if (materialFile.Exists)
                return materialFile;
        }

        Utils.Log(LogLevelEnum.Info, $"Unable to find material file for {name}");
        return null;
    }
}