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
            _chunks ??= Models.SelectMany(m => m.ChunkMap.Values).ToList();
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

                ChunkNode? rootNode = null;

                Utils.Log(LogLevelEnum.Debug, "Mapping Nodes");

                foreach (Model model in Models)
                {
                    // TODO: Refactor. Not sure every model has just 1 root node. Find where parent id = ~0.
                    model.RootNode = rootNode = (rootNode ?? model.RootNode);  // ?? Each model will have it's own rootnode.

                    foreach (ChunkNode node in model.ChunkMap.Values.Where(c => c.ChunkType == ChunkType.Node).Select(c => c as ChunkNode))
                    {
                        // Preserve existing parents
                        if (_nodeMap.ContainsKey(node.Name))
                        {
                            ChunkNode parentNode = _nodeMap[node.Name].ParentNode;

                            if (parentNode is not null)
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

            if (RootNode is null)
                RootNode = model.RootNode;  // This makes the assumption that we read the .cga file before the .cgam file.

            Bones = Bones ?? model.Bones;
            Models.Add(model);
        }

        SkinningInfo = ConsolidateSkinningInfo(Models);

        foreach (var model in Models)
        {
            CreateMaterialsFor(model);
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
            if (model.SkinningInfo.IntFaces is not null)
                skin.IntFaces = model.SkinningInfo.IntFaces;

            if (model.SkinningInfo.IntVertices is not null)
                skin.IntVertices = model.SkinningInfo.IntVertices;

            if (model.SkinningInfo.LookDirectionBlends is not null)
                skin.LookDirectionBlends = model.SkinningInfo.LookDirectionBlends;

            if (model.SkinningInfo.MorphTargets is not null)
                skin.MorphTargets = model.SkinningInfo.MorphTargets;

            if (model.SkinningInfo.PhysicalBoneMeshes is not null)
                skin.PhysicalBoneMeshes = model.SkinningInfo.PhysicalBoneMeshes;

            if (model.SkinningInfo.BoneEntities is not null)
                skin.BoneEntities = model.SkinningInfo.BoneEntities;

            if (model.SkinningInfo.BoneMapping is not null)
                skin.BoneMapping = model.SkinningInfo.BoneMapping;

            if (model.SkinningInfo.Collisions is not null)
                skin.Collisions = model.SkinningInfo.Collisions;

            if (model.SkinningInfo.CompiledBones is not null)
                skin.CompiledBones = model.SkinningInfo.CompiledBones;

            if (model.SkinningInfo.Ext2IntMap is not null)
                skin.Ext2IntMap = model.SkinningInfo.Ext2IntMap;
        }

        return skin;
    }

    private void CreateMaterialsFor(Model model)
    {
        foreach (ChunkNode nodeChunk in model.ChunkMap.Values.Where(c => c.ChunkType == ChunkType.Node))
        {
            // If Ivo file, just a single material node chunk with a library.
            if (nodeChunk._model.IsIvoFile)
            {
                var ivoMatChunk = (ChunkMtlName)Chunks.Where(c => c.ChunkType == ChunkType.MtlNameIvo).FirstOrDefault();
                var ivoMatFile = GetMaterialFile(ivoMatChunk.Name);

                if (ivoMatFile is not null)
                    nodeChunk.Materials = GetMaterialFromMatFile(ivoMatFile, nodeChunk.Name);
                else
                    nodeChunk.Materials = CreateDefaultMaterials(nodeChunk);
                continue;
            }

            if (model.ChunkMap.Values.FirstOrDefault(c => c.ID == nodeChunk.MatID) is not ChunkMtlName matChunk)
            {
                Utils.Log(LogLevelEnum.Debug, $"Unable to find material chunk {nodeChunk.MatID} for node {nodeChunk.ID}");
                continue;
            }

            if (matChunk.Name.Contains("mechDefault.mtl") && (matChunk.NumChildren == 5 || matChunk.NumChildren == 4))  // Edge case for MWO models
                matChunk.Name = "Objects/mechs/generic/body/generic_body.mtl";

            if (matChunk.Name.Contains("mechDefault.mtl") && matChunk.NumChildren == 11)  // Edge case for MWO models
                matChunk.Name = "Objects/mechs/_mech_templates/body/mecha.mtl";

            if (matChunk.Name.Contains("05 - Default") && matChunk.NumChildren == 5)  // Edge case for MWO models
                matChunk.Name = "Objects/mechs/generic/body/generic_body.mtl";

            var matfile = GetMaterialFile(matChunk.Name);

            if (matfile is not null)
                nodeChunk.Materials = GetMaterialFromMatFile(matfile, nodeChunk.Name);
            else
                nodeChunk.Materials = CreateDefaultMaterials(nodeChunk);

            // create dummy 5th material (generic_variant) for MWO mechDefault.mtls
            if (matChunk.Name.Equals("Objects/mechs/generic/body/generic_body.mtl"))
            {
                var source = nodeChunk.Materials.SubMaterials;
                var newMats = new Material[5];

                Array.Copy(source, newMats, source.Length);
                newMats[4] = nodeChunk.Materials.SubMaterials[2];
                nodeChunk.Materials.SubMaterials = newMats;
            }
        }
    }

    private static Material? GetMaterialFromMatFile(FileInfo matfile, string matName) => MaterialUtilities.FromFile(matfile.FullName, matName);
    
    private Material? CreateDefaultMaterials(ChunkNode nodeChunk)
    {
        // For each child of this node chunk's material file, create a default material.
        var mtlNameChunk = (ChunkMtlName)Chunks.Where(c => c.ID == nodeChunk.MatID).FirstOrDefault();
        if (mtlNameChunk is not null)
        {
            if (mtlNameChunk.MatType == MtlNameType.Basic || mtlNameChunk.MatType == MtlNameType.Single)
            {
                var material = new Material() { Name = nodeChunk.Name };
                Material[] submats = new Material[1];
                submats[0] = MaterialUtilities.CreateDefaultMaterial(nodeChunk.Name);
                material.SubMaterials = submats;
                return material;
            }
            else if (mtlNameChunk.MatType == MtlNameType.Library)
            {
                var material = new Material { SubMaterials = new Material[mtlNameChunk.NumChildren] };
                for (int i = 0; i < mtlNameChunk.NumChildren; i++)
                {
                    if (mtlNameChunk.ChildIDs is not null)
                    {
                        var childMaterial = (ChunkMtlName)Chunks.Where(c => c.ID == mtlNameChunk.ChildIDs[i]).FirstOrDefault();
                        var mat = MaterialUtilities.CreateDefaultMaterial(childMaterial.Name, $"{i / (float)mtlNameChunk.NumChildren},0.5,0.5");
                        material.SubMaterials[i] = mat;
                    }
                    else   // TODO: For SC, there are no more mtlname chunks. Use node name?
                        material.SubMaterials[i] = MaterialUtilities.CreateDefaultMaterial($"Material-{i}", $"{i / (float)mtlNameChunk.NumChildren},0.5,0.5");
                }

                return material;
            }
            else
                Utils.Log(LogLevelEnum.Info, $"Found MtlName chunk {mtlNameChunk.ID} with unhandled MtlNameType {mtlNameChunk.MatType}.");
        }
        return null;
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
            materialFile = new FileInfo(Path.Combine(DataDir, name));

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