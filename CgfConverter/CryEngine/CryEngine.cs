using CgfConverter.CryEngineCore;
using CgfConverter.Materials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CgfConverter.CryXmlB;
using CgfConverter.PackFileSystem;
using CgfConverter.Utils;
using Extensions;
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
        ".soc",
        ".caf",
        ".dba"
    };

    protected readonly TaggedLogger Log;

    public List<Model> Models { get; internal set; } = new();
    public List<Model> Animations { get; internal set; } = new();
    public ChunkNode RootNode { get; internal set; }
    public ChunkCompiledBones Bones { get; internal set; }
    public SkinningInfo? SkinningInfo { get; set; }
    public string InputFile { get; internal set; }
    public IPackFileSystem PackFileSystem { get; internal set; }
    public string? MaterialFile { get; set; }

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

                Log.D("Mapping Nodes");

                foreach (Model model in Models)
                {
                    model.RootNode = rootNode = (rootNode ?? model.RootNode);

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

    private List<Chunk>? _chunks;
    private Dictionary<string, ChunkNode>? _nodeMap;

    public CryEngine(string filename, IPackFileSystem packFileSystem, TaggedLogger? parentLogger = null)
    {
        Log = new TaggedLogger(Path.GetFileName(filename), parentLogger);
        InputFile = filename;
        PackFileSystem = packFileSystem;
    }

    public void ProcessCryengineFiles()
    {
        var inputFiles = new List<string> { InputFile };

        if (!validExtensions.Contains(Path.GetExtension(InputFile).ToLowerInvariant()))
        {
            Log.D(invalidExtensionErrorMessage);
            throw new FileLoadException(invalidExtensionErrorMessage, InputFile);
        }

        AutoDetectMFile(InputFile, InputFile, inputFiles);

        foreach (var file in inputFiles)
        {
            // Each file (.cga and .cgam if applicable) will have its own RootNode.  
            // This can cause problems.  .cga files with a .cgam files won't have geometry for the one root node.
            var model = Model.FromStream(file, PackFileSystem.GetStream(file), true);

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

        try
        {
            var chrparams = CryXmlSerializer.Deserialize<ChrParams.ChrParams>(
                PackFileSystem.GetStream(Path.ChangeExtension(InputFile, ".chrparams")));
            var trackFilePath = chrparams.Animations?.FirstOrDefault(x => x.Name == "$TracksDatabase" || x.Name == "#filepath")?.Path;
            if (trackFilePath is null)
                throw new FileNotFoundException();
            if (Path.GetExtension(trackFilePath) != "dba")
                trackFilePath = Path.ChangeExtension(trackFilePath, "dba");
            Log.D("Associated animation track database file found at {0}", trackFilePath);
            Animations.Add(Model.FromStream(trackFilePath, PackFileSystem.GetStream(trackFilePath), true));
        }
        catch (FileNotFoundException)
        {
            // pass
        }
    }

    private void AutoDetectMFile(string filename, string inputFile, List<string> inputFiles)
    {
        var mFile = Path.ChangeExtension(filename, $"{Path.GetExtension(inputFile)}m");
        if (PackFileSystem.Exists(mFile))
        {
            Log.D("Found geometry file {0}", mFile);
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
        var loadedMaterialMap = new Dictionary<Chunk, Material?>();
        foreach (var nodeChunk in model.ChunkMap.Values.OfType<ChunkNode>())
        {
            Material? material;
            
            // If Ivo file, just a single material node chunk with a library.
            if (nodeChunk._model.IsIvoFile)
            {
                if (Chunks.FirstOrDefault(c => c.ChunkType == ChunkType.MtlNameIvo || c.ChunkType == ChunkType.MtlNameIvo320) is not ChunkMtlName ivoMatChunk)
                {
                    Log.D($"Unable to find material chunk {nodeChunk.MatID} for node {nodeChunk.ID}");
                    continue;
                }

                if (loadedMaterialMap.TryGetValue(ivoMatChunk, out material))
                    nodeChunk.Materials = material;
                else if (GetMaterialFile(ivoMatChunk.Name) is { } ivoMatFile)
                    nodeChunk.Materials =
                        MaterialUtilities.FromStream(PackFileSystem.GetStream(ivoMatFile), nodeChunk.Name, true);
                
                loadedMaterialMap[ivoMatChunk] = nodeChunk.Materials ??= CreateDefaultMaterials(nodeChunk);
                continue;
            }

            if (model.ChunkMap.Values.FirstOrDefault(c => c.ID == nodeChunk.MatID) is not ChunkMtlName matChunk)
            {
                Log.D($"Unable to find material chunk {nodeChunk.MatID} for node {nodeChunk.ID}");
                continue;
            }

            if (loadedMaterialMap.TryGetValue(matChunk, out material))
            {
                nodeChunk.Materials = material;
                continue;
            }

            var name = matChunk.Name;
            
            if (name.Contains("mechDefault.mtl") && (matChunk.NumChildren == 5 || matChunk.NumChildren == 4))  // Edge case for MWO models
                name = "Objects/mechs/generic/body/generic_body.mtl";

            if (name.Contains("mechDefault.mtl") && matChunk.NumChildren == 11)  // Edge case for MWO models
                name = "Objects/mechs/_mech_templates/body/mecha.mtl";

            if (name.Contains("05 - Default") && matChunk.NumChildren == 5)  // Edge case for MWO models
                name = "Objects/mechs/generic/body/generic_body.mtl";

            if (GetMaterialFile(name) is { } matfile)
                nodeChunk.Materials = MaterialUtilities.FromStream(PackFileSystem.GetStream(matfile), nodeChunk.Name, true);

            loadedMaterialMap[matChunk] = nodeChunk.Materials ??= CreateDefaultMaterials(nodeChunk);

            // create dummy 5th material (generic_variant) for MWO mechDefault.mtls
            if (matChunk.Name.Equals("Objects/mechs/generic/body/generic_body.mtl", StringComparison.OrdinalIgnoreCase))
            {
                var source = nodeChunk.Materials!.SubMaterials;
                var newMats = new Material[5];

                Array.Copy(source, newMats, source.Length);
                newMats[4] = nodeChunk.Materials!.SubMaterials[2];
                nodeChunk.Materials.SubMaterials = newMats;
            }
        }
    }

    private Material? CreateDefaultMaterials(ChunkNode nodeChunk)
    {
        // For each child of this node chunk's material file, create a default material.
        if (Chunks.OfType<ChunkMtlName>().FirstOrDefault(c => c.ID == nodeChunk.MatID) is not { } mtlNameChunk)
            return null;

        switch (mtlNameChunk.MatType)
        {
            case MtlNameType.Basic:
            case MtlNameType.Single:
                return new Material
                {
                    Name = nodeChunk.Name,
                    SubMaterials = new[] {MaterialUtilities.CreateDefaultMaterial(nodeChunk.Name)}
                };
            
            case MtlNameType.Library:
            {
                var material = new Material {SubMaterials = new Material[mtlNameChunk.NumChildren]};
                for (var i = 0; i < mtlNameChunk.NumChildren; i++)
                {
                    if (mtlNameChunk.ChildIDs is not null)
                    {
                        var childMaterial =
                            (ChunkMtlName?) Chunks.FirstOrDefault(c => c.ID == mtlNameChunk.ChildIDs[i]);
                        var mat = MaterialUtilities.CreateDefaultMaterial(childMaterial?.Name ?? $"Material-{i}",
                            $"{i / (float) mtlNameChunk.NumChildren},0.5,0.5");
                        material.SubMaterials[i] = mat;
                    }
                    else // TODO: For SC, there are no more mtlname chunks. Use node name?
                        material.SubMaterials[i] = MaterialUtilities.CreateDefaultMaterial($"Material-{i}",
                            $"{i / (float) mtlNameChunk.NumChildren},0.5,0.5");
                }

                return material;
            }
            default:
                Log.I($"Found MtlName chunk {mtlNameChunk.ID} with unhandled MtlNameType {mtlNameChunk.MatType}.");
                return null;
        }
    }

    // Gets the material file for Basic, Single and Library types.  Child materials are created from the library.
    private string? GetMaterialFile(string name)
    {
        // If a MaterialFile is provided, use that.
        if (MaterialFile is not null)
        {
            // Check if absolute material path is given
            if (File.Exists(MaterialFile))
                return MaterialFile;

            var mtlName = FileHandlingExtensions.CombineAndNormalizePath(Path.GetDirectoryName(InputFile), MaterialFile);
            if (PackFileSystem.Exists(mtlName))
                return mtlName;

            // Check if material file relative to object directory
            if (PackFileSystem.Exists(MaterialFile))
                return MaterialFile;
        }

        if (name.Contains(':'))  // Need an example and test for this case.  Probably a child material?
            name = name.Split(':')[1];

        if (!name.EndsWith(".mtl"))
            name += ".mtl";

        if (name.Contains("mechDefault.mtl"))
        {
            // For MWO models with a material called "Material #0", which is a default mat used on lots of mwo mechs.
            // The actual material file is in objects\mechs\generic\body\generic_body.mtl
            name = "objects\\mechs\\generic\\body\\generic_body.mtl";

            if (PackFileSystem.Exists(name))
                return name;
        }

        var dirStrings = InputFile.Split("\\");
        for (int i = 0; i < dirStrings.Length; i++)
        {
            if (string.Equals(dirStrings[i], "Objects", StringComparison.OrdinalIgnoreCase))
            {
                var path = string.Join("\\", dirStrings.Take(i));
                var mtlFileName = FileHandlingExtensions.CombineAndNormalizePath(path, name);
                if (PackFileSystem.Exists(mtlFileName))
                    return mtlFileName;
            }
        }

        // Check if material file is in or relative to current directory
        var fullName = FileHandlingExtensions.CombineAndNormalizePath(Path.GetDirectoryName(InputFile), name);
        if (PackFileSystem.Exists(fullName))
            return fullName;

        // Check if material file relative to object directory
        if (PackFileSystem.Exists(name))
            return name;

        // See if there is any .mtl file in the current directory. If so, use that.
        var currentDir = Path.GetDirectoryName(fullName);
        if (Directory.Exists(currentDir!))
        {
            var mtlFiles = Directory.GetFiles(currentDir!, "*.mtl");
            if (mtlFiles.Length > 0)
            {
                Log.I("Found one ore more multiple material files in {0}. Using {1}", currentDir, mtlFiles[0]);
                return mtlFiles[0];
            }
        }

        Log.W("Unable to find material file for {0}", name);
        return null;
    }

    public static bool SupportsFile(string name) => validExtensions.Contains(Path.GetExtension(name).ToLowerInvariant());
}
