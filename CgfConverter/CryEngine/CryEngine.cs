using CgfConverter.CryEngineCore;
using CgfConverter.CryXmlB;
using CgfConverter.Materials;
using CgfConverter.PackFileSystem;
using CgfConverter.Utils;
using Extensions;
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
        ".soc",
        ".caf",
        ".dba"
    };

    protected readonly TaggedLogger Log;

    public string Name => Path.GetFileNameWithoutExtension(InputFile).ToLower();
    public List<Model> Models { get; internal set; } = new();
    public List<Model> Animations { get; internal set; } = new();
    public ChunkNode RootNode { get; internal set; }
    public ChunkCompiledBones Bones { get; internal set; }
    public SkinningInfo? SkinningInfo { get; set; }
    public string InputFile { get; internal set; }
    public IPackFileSystem PackFileSystem { get; internal set; }
    public string? MaterialFile { get; set; }
    public string? ObjectDir { get; set; }
    public Material Materials { get; internal set; }

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

    public CryEngine(string filename, IPackFileSystem packFileSystem, TaggedLogger? parentLogger = null, string? materialFile = null, string? objectDir = null)
    {
        Log = new TaggedLogger(Path.GetFileName(filename), parentLogger);
        InputFile = filename;
        PackFileSystem = packFileSystem;
        MaterialFile = materialFile;
        ObjectDir = objectDir;
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

        // Create materials from the first model. First model contains a link to the material file if it isn't provided.
        CreateMaterialsFor(Models[0]);

        try
        {
            var chrparams = CryXmlSerializer.Deserialize<ChrParams.ChrParams>(
                PackFileSystem.GetStream(Path.ChangeExtension(InputFile, ".chrparams")));
            var trackFilePath = chrparams.Animations?.FirstOrDefault(x => x.Name == "$TracksDatabase")?.Path;
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
        // if -mtl arg is used, try to find the material file, set to the full path, and create materials from it.
        if (MaterialFile is not null)
        {
            if (!MaterialFile.EndsWith(".mtl"))
                MaterialFile += ".mtl";

            // check if file exists as provided
            if (PackFileSystem.Exists(MaterialFile))
            {
                Materials = MaterialUtilities.FromStream(PackFileSystem.GetStream(MaterialFile), Name, true);
                return;
            }

            // Check if it's in the same directory as the model file
            var modelPath = Path.GetDirectoryName(InputFile);
            if (modelPath is not null && PackFileSystem.Exists(FileHandlingExtensions.CombineAndNormalizePath(modelPath, MaterialFile)))
            {
                MaterialFile = FileHandlingExtensions.CombineAndNormalizePath(modelPath, MaterialFile);
                Materials = MaterialUtilities.FromStream(PackFileSystem.GetStream(MaterialFile), Name, true);
                return;
            }

            // check if relative to objectdir if provided
            if (ObjectDir is not null && PackFileSystem.Exists(FileHandlingExtensions.CombineAndNormalizePath(ObjectDir, MaterialFile)))
            {
                MaterialFile = FileHandlingExtensions.CombineAndNormalizePath(ObjectDir, MaterialFile);
                Materials = MaterialUtilities.FromStream(PackFileSystem.GetStream(MaterialFile), Name, true);
                return;
            }
            Log.W("Unable to find material file {0}.  Checking material library chunks for materials.", MaterialFile);
        }

        var materialLibraryFiles = model.ChunkMap.Values
            .OfType<ChunkMtlName>()
            .Where(x => x.MatType == MtlNameType.Library || x.MatType == MtlNameType.Basic)
            .Select(x => x.Name);

        MaterialFile = GetMaterialFileFromMatLibrary(materialLibraryFiles);
        if (MaterialFile is not null)
            Materials = MaterialUtilities.FromStream(PackFileSystem.GetStream(MaterialFile), Name, true);

        if (Materials is null)
        {
            var maxNumberOfMaterials = model.ChunkMap.Values.OfType<ChunkMtlName>().Max(c => c.NumChildren);
            Materials = CreateDefaultMaterials(maxNumberOfMaterials == 0 ? 1 : maxNumberOfMaterials);
        }
    }

    private Material CreateDefaultMaterials(uint maxNumberOfMaterials)
    {
        var materials = new Material
        {
            SubMaterials = new Material[maxNumberOfMaterials],
            Name = Name
        };

        for (var i = 0; i < maxNumberOfMaterials; i++)
            materials.SubMaterials[i] = MaterialUtilities.CreateDefaultMaterial($"{Name}-material-{i}", $"{i / (float) maxNumberOfMaterials},0.5,0.5");

        return materials;
    }

    // Gets the Library material file if one can be found. File will either be relative to same dir as model file or relative
    // to object dir.
    private string? GetMaterialFileFromMatLibrary(IEnumerable<string> libraryFileNames)
    {
        if (libraryFileNames is not null)
        {
            foreach (var libraryFile in libraryFileNames)
            {
                var testFile = libraryFile;
                if (!testFile.EndsWith(".mtl"))
                    testFile += ".mtl";

                if (PackFileSystem.Exists(testFile))
                {
                    MaterialFile = testFile;
                    return testFile;
                }

                // Check if testFile + object dir exists
                if (PackFileSystem.Exists(FileHandlingExtensions.CombineAndNormalizePath(ObjectDir, testFile)))
                    return testFile;

                // Check in current dir
                var fullPath = FileHandlingExtensions.CombineAndNormalizePath(Path.GetDirectoryName(InputFile), $"{libraryFile}.mtl");
                if (PackFileSystem.Exists(fullPath))
                    return fullPath;
            }
        }

        Log.W("Unable to find material file for {0}", InputFile);
        return null;
    }

    public static bool SupportsFile(string name) => validExtensions.Contains(Path.GetExtension(name).ToLowerInvariant());
}
