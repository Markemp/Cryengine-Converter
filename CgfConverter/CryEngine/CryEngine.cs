using CgfConverter.CryEngineCore;
using CgfConverter.CryXmlB;
using CgfConverter.Models;
using CgfConverter.PackFileSystem;
using CgfConverter.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Material = CgfConverter.Models.Materials.Material;

namespace CgfConverter;

public partial class CryEngine
{
    private const string invalidExtensionErrorMessage = "Warning: Unsupported file extension - please use a cga, cgf, chr or skin file.";

    private static readonly HashSet<string> validExtensions =
    [
        ".cgf",
        ".cga",
        ".chr",
        ".skin",
        ".anim",
        ".soc",
        ".caf",
        ".dba"
    ];

    public readonly TaggedLogger Log;

    public string Name => Path.GetFileNameWithoutExtension(InputFile).ToLower();
    public List<Model> Models { get; internal set; } = []; // All the model files associated with this game object
    public List<Model> Animations { get; internal set; } = [];  // Animation files for this object
    public List<ChunkNode> Nodes { get; internal set; } = []; // node hierarchy.
    public ChunkNode RootNode { get; internal set; } // can get node hierarchy from here
    public ChunkCompiledBones Bones { get; internal set; }  // move to skinning info
    public SkinningInfo? SkinningInfo { get; set; }
    public string InputFile { get; internal set; }
    public IPackFileSystem PackFileSystem { get; internal set; }
    public List<string> MaterialFiles { get; set; } = [];
    public string? ObjectDir { get; set; }

    /// <summary>Dictionary of Materials.  Key is the mtlName chunk Name property (stripped of path and
    /// extension info), or the material file if provided.</summary>
    public Dictionary<string, Material> Materials { get; internal set; } = [];

    private List<Chunk>? _chunks;
    public List<Chunk> Chunks {
        get {
            _chunks ??= Models.SelectMany(m => m.ChunkMap.Values).ToList();
            return _chunks;
        }
    }

    public CryEngine(string filename, IPackFileSystem packFileSystem, TaggedLogger? parentLogger = null, string? materialFiles = null, string? objectDir = null)
    {
        Log = new TaggedLogger(Path.GetFileName(filename), parentLogger);
        InputFile = filename;
        PackFileSystem = packFileSystem;
        MaterialFiles = string.IsNullOrEmpty(materialFiles) ? [] : materialFiles.Split(',').ToList();
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

        CreateMaterials();
        BuildNodeStructure();

        CreateAnimations();

        AssignMaterialsToNodes(false);
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

    private void BuildNodeStructure()
    {
        if (IsIvoFile)
        {
            // Create node chunks from the first model.  If there is a nodemeshcombo chunk, use
            // that for the nodes.  If not (skin and chr files), create a dummy root node.
            // Can be zero or multiple nodes, but all reference the same geometry.
            bool hasValidNodeMeshCombo = false;
            var comboChunk = (ChunkNodeMeshCombo?)Models[index: 0].ChunkMap.Values.FirstOrDefault(c => c.ChunkType == ChunkType.NodeMeshCombo);

            if (comboChunk is not null && comboChunk.NumberOfNodes != 0)
                hasValidNodeMeshCombo = true;

            if (hasValidNodeMeshCombo)
            {
                // SkinMesh has the mesh and meshsubset info, as well as all the datastreams
                var skinMesh = Models.Count > 1
                    ? Models[1].ChunkMap.Values.FirstOrDefault(x => x.ChunkType == ChunkType.IvoSkin || x.ChunkType == ChunkType.IvoSkin2) as ChunkIvoSkinMesh
                    : null;

                var geometryMeshDetails = skinMesh?.MeshDetails;

                var stringTable = comboChunk?.NodeNames ?? [];
                var materialTable = comboChunk?.MaterialIndices ?? [];
                var materialFileName = Materials.Keys.First();

                // create node chunks
                foreach (var node in comboChunk.NodeMeshCombos)
                {
                    var index = comboChunk.NodeMeshCombos.IndexOf(node);

                    // Create meshsubsets for this node.  This is all meshSubsets where the meshParent equals the node index
                    var subsets = skinMesh?.MeshSubsets.Where(x => x.NodeParentIndex == index).ToList() ?? [];

                    ChunkMesh chunkMesh = new ChunkMesh_802();

                    var hasGeometry = subsets.Count != 0;

                    // Create a meshchunk for nodes with geometry
                    if (hasGeometry)
                    {
                        chunkMesh.ScalingVectors = geometryMeshDetails.ScalingBoundingBox;
                        chunkMesh.MaxBound = geometryMeshDetails.BoundingBox.Max;
                        chunkMesh.MinBound = geometryMeshDetails.BoundingBox.Min;
                        chunkMesh.NumVertices = (int)skinMesh.MeshDetails.NumberOfVertices;
                        chunkMesh.NumIndices = (int)skinMesh.MeshDetails.NumberOfIndices;
                        chunkMesh.NumVertSubsets = skinMesh.MeshDetails.NumberOfSubmeshes;
                        chunkMesh.GeometryInfo = BuildNodeGeometryInfo(skinMesh, subsets);
                    }

                    var newNode = new ChunkNode_823
                    {
                        Name = stringTable[index],
                        ObjectNodeID = -1,
                        ParentNodeIndex = node.ParentIndex,
                        ParentNodeID = node.ParentIndex == 0xffff ? -1 : node.ParentIndex,
                        NumChildren = node.NumberOfChildren,
                        MaterialID = node.GeometryType == IvoGeometryType.Geometry ? materialTable[index] : 0,
                        Transform = node.BoneToWorld.ConvertToLocalTransformMatrix(),
                        ChunkType = ChunkType.Node,
                        ID = (int)node.Id,
                        MeshData = hasGeometry ? chunkMesh : null,
                        MaterialFileName = materialFileName
                    };

                    if (hasGeometry)
                        newNode.Materials = Materials.Values.First();

                    Nodes.Add(newNode);
                }

                // build node hierarchy
                foreach (var node in Nodes)
                {
                    var index = Nodes.IndexOf(node);
                    if (node.ParentNodeIndex != 0xFFFF)
                        node.ParentNode = Nodes[node.ParentNodeIndex];
                    else
                        RootNode = node; // hopefully there is just one

                    // Add all child nodes to Children.  A child is where the parent index is current index
                    var childNodes = Nodes.Where(x => x.ParentNodeIndex == index);
                    foreach (var child in childNodes)
                    {
                        node.Children.Add(child);
                    }
                }
            }
            else  // Skin version.  Manually create mesh chunk and submeshes.
            {
                ChunkMesh? chunkMesh = Models.Count == 1 ? null : CreateMeshData();

                var rootNode = new ChunkNode_823
                {
                    Name = Path.GetFileNameWithoutExtension(InputFile),
                    ObjectNodeID = 2,
                    ParentNodeIndex = -1,     // No parent
                    ParentNodeID = -1,
                    NumChildren = 0,     // Single object
                    MaterialID = 11,
                    Transform = Matrix4x4.Identity,
                    ChunkType = ChunkType.Node,
                    ID = 1,
                    MeshData = chunkMesh
                };
                Nodes.Add(rootNode);
                RootNode = rootNode;
            }
        }
        else // Traditional Crydata.  Build geometry info from the models.
        {
            // Separate datastream for each node.
            // For each ChunkNode in model[0], add it to the Nodes list.
            foreach (var node in Models[0].NodeMap.Values)
            {
                // Add helper or mesh data to the node
                var objectChunk = Models[0].ChunkMap[node.ObjectNodeID];
                node.Children = Models[0].NodeMap.Values.Where(x => x.ParentNodeID == node.ID).ToList();

                if (objectChunk is ChunkHelper helper)
                {
                    node.ChunkHelper = helper;
                    Nodes.Add(node);
                    continue;
                }
                else if (objectChunk is ChunkMesh mesh)
                {
                    // For models with separate geometry files, the MESH_IS_EMPTY flag will be set
                    // on the first model.  You have to find the equivalent mesh chunk in the geometry
                    // file to find out if it's a mesh physics chunk or not.
                    bool isSplitFile = Models.Count > 1;
                    if (isSplitFile)
                    {
                        // Have mesh point to 2nd model's mesh chunk.
                        var model1Node = Models[1].NodeMap.FirstOrDefault(x => x.Value.Name == node.Name).Value;
                        if (model1Node is null)  // physics node.  Continue.
                        {
                            Nodes.Add(node);
                            continue;
                        }
                            
                        mesh = Models[1].ChunkMap[model1Node.ObjectNodeID] as ChunkMesh;
                    }

                    node.MeshData = mesh;
                    // If it's mesh physics data, there won't be any geometry info.
                    if (!mesh.Flags1.HasFlag(MeshChunkFlag.MESH_IS_EMPTY))
                    {
                        var submeshData = (Models[^1].ChunkMap[mesh.MeshSubsetsData] as ChunkMeshSubsets)!.MeshSubsets;
                        mesh.GeometryInfo = new()
                        {
                            GeometrySubsets = submeshData,
                            Indices = GetRequiredDatastream<uint>(mesh.IndicesData),
                            UVs = GetRequiredDatastream<UV>(mesh.UVsData),
                            Vertices = GetRequiredDatastream<Vector3>(mesh.VerticesData),
                            Colors = GetRequiredDatastream<IRGBA>(mesh.ColorsData),
                            VertUVs = GetRequiredDatastream<VertUV>(mesh.VertsUVsData),
                            Normals = GetRequiredDatastream<Vector3>(mesh.NormalsData),
                            BoneMappings = GetRequiredDatastream<MeshBoneMapping>(mesh.BoneMapData),
                            BoundingBox = new BoundingBox(mesh.MinBound, mesh.MaxBound)
                        };
                    }
                }
                
                Nodes.Add(node);
            }
        }
    }

    private ChunkMesh CreateMeshData()
    {
        var skinMesh = Models[1].ChunkMap.Values.FirstOrDefault(x => x.ChunkType == ChunkType.IvoSkin || x.ChunkType == ChunkType.IvoSkin2) as ChunkIvoSkinMesh;
        var geometryMeshDetails = skinMesh.MeshDetails;
        var subsets = skinMesh.MeshSubsets;

        ChunkMesh chunkMesh = new ChunkMesh_802
        {
            ScalingVectors = geometryMeshDetails.ScalingBoundingBox,
            MaxBound = geometryMeshDetails.BoundingBox.Max,
            MinBound = geometryMeshDetails.BoundingBox.Min,
            NumVertices = (int)skinMesh.MeshDetails.NumberOfVertices,
            NumIndices = (int)skinMesh.MeshDetails.NumberOfIndices,
            NumVertSubsets = skinMesh.MeshDetails.NumberOfSubmeshes,
            GeometryInfo = BuildNodeGeometryInfo(skinMesh, subsets)
        };
        return chunkMesh;
    }

    private static SkinningInfo? ConsolidateSkinningInfo(List<Model> models)
    {
        if (!models.Any(model => model.ChunkMap.Values.Any(chunk => chunk is ChunkCompiledBones)))
            return null;

        SkinningInfo skin = new();

        foreach (var chunk in models.SelectMany(x => x.ChunkMap.Values))
        {
            switch (chunk)
            {
                case ChunkCompiledExtToIntMap extToIntMap:
                    skin.Ext2IntMap = extToIntMap.Source?.ToList();
                    break;

                case ChunkCompiledBones compiledBones:
                    skin.CompiledBones = compiledBones.BoneList;
                    break;

                case ChunkCompiledIntFaces intFaces:
                    skin.IntFaces = intFaces.Faces?.ToList();
                    break;

                case ChunkCompiledIntSkinVertices intVertices:
                    skin.IntVertices = intVertices.IntSkinVertices?.ToList();
                    break;

                case ChunkIvoSkinMesh ivoSkinMesh:
                    skin.BoneMappings = ivoSkinMesh.BoneMappings?.Data.ToList();
                    break;

                case ChunkDataStream dataStream:
                    if (dataStream.Data is Datastream<MeshBoneMapping> boneMaps)
                        skin.BoneMappings = boneMaps.Data.ToList();
                    break;
            }
        }

        return skin;
    }


    private void CreateMaterials()
    {
        var materialStrategies = new List<Func<bool>>
        {
            TryLoadExplicitMaterialFiles,
            TryLoadMaterialLibraryFiles,
            CreateDefaultMaterials
        };

        foreach (var strategy in materialStrategies)
        {
            if (strategy()) return;
        }
    }

    private bool TryLoadExplicitMaterialFiles()
    {
        if (MaterialFiles.Count == 0) return false;

        Log.I("Loading materials from explicitly provided files");

        var loadedAny = false;
        foreach (var materialFile in MaterialFiles.ToList()) // ToList to avoid modification during iteration
        {
            if (TryLoadSingleMaterialFile(materialFile))
                loadedAny = true;
            else
                Log.W("Unable to find provided material file {0}. Will check material library chunks.", materialFile);
        }

        return loadedAny;
    }

    private bool TryLoadMaterialLibraryFiles()
    {
        var libraryFiles = GetMaterialLibraryFileNames();
        if (!libraryFiles.Any())
        {
            Log.I("No material library files found in model chunks");
            return false;
        }

        Log.I("Loading materials from library chunks: {0}", string.Join(", ", libraryFiles));

        var loadedAny = false;
        foreach (var libraryFile in libraryFiles)
        {
            if (TryLoadSingleMaterialFile(libraryFile))
            {
                loadedAny = true;
                // Add to MaterialFiles list for consistency
                if (!MaterialFiles.Contains(libraryFile))
                    MaterialFiles.Add(libraryFile);
            }
        }

        return loadedAny;
    }

    private bool CreateDefaultMaterials()
    {
        Log.W("Unable to find any material files for this model. Creating dummy materials.");

        var libraryFiles = GetMaterialLibraryFileNames();
        if (!libraryFiles.Any())
        {
            // Create a single default material with unknown key
            var defaultKey = "default";
            var maxMats = CalculateMaxMaterialCount();
            var defaultMaterial = CreateDefaultMaterialSet(maxMats);
            Materials.Add(defaultKey, defaultMaterial);
            MaterialFiles.Add(defaultKey);
            return true;
        }

        // Create default materials for each library file found
        var maxMaterials = CalculateMaxMaterialCount();
        foreach (var libraryFile in libraryFiles)
        {
            var key = Path.GetFileNameWithoutExtension(libraryFile) ?? "unknown";
            if (!Materials.ContainsKey(key))
            {
                var defaultMaterial = CreateDefaultMaterialSet(maxMaterials);
                Materials.Add(key, defaultMaterial);
                MaterialFiles.Add(libraryFile);
            }
        }

        return Materials.Count > 0;
    }

    private bool TryLoadSingleMaterialFile(string materialFile)
    {
        var key = Path.GetFileNameWithoutExtension(materialFile);
        var fullyQualifiedPath = GetFullMaterialFilePath(materialFile);

        if (fullyQualifiedPath is null)
            return false;

        try
        {
            var materials = MaterialUtilities.FromStream(
                PackFileSystem.GetStream(fullyQualifiedPath),
                materialFile,
                ObjectDir,
                closeAfter: true);

            Materials.Add(key, materials);
            Log.D("Successfully loaded material file: {0}", materialFile);
            return true;
        }
        catch (Exception ex)
        {
            Log.W("Failed to load material file {0}: {1}", materialFile, ex.Message);
            return false;
        }
    }

    private IEnumerable<string> GetMaterialLibraryFileNames()
    {
        return Models[0].ChunkMap.Values
            .OfType<ChunkMtlName>()
            .Where(x => x.MatType == MtlNameType.Library ||
                       x.MatType == MtlNameType.Basic ||
                       x.MatType == MtlNameType.Single)
            .Select(x => x.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct();
    }

    private uint CalculateMaxMaterialCount()
    {
        // Get max from mesh subsets
        var meshSubsets = Models.Last().ChunkMap.Values
            .OfType<ChunkMeshSubsets>()
            .SelectMany(c => c.MeshSubsets);
        var maxFromMeshSubsets = meshSubsets.Any()
            ? (uint)(meshSubsets.Max(x => x.MatID) + 1)
            : 0u;

        // Get max from material name chunks
        var mtlNameCounts = Models
            .SelectMany(x => x.ChunkMap.Values.OfType<ChunkMtlName>())
            .Select(x => x.NumChildren);
        var maxFromMtlNames = mtlNameCounts.Any() ? mtlNameCounts.Max() : 0u;

        // Get max from IVO subsets if applicable
        var maxFromIvoSubsets = 0u;
        if (Models.Count > 1)
        {
            var ivoSubsets = Models[1].ChunkMap.Values
                .OfType<ChunkIvoSkinMesh>()
                .SelectMany(x => x.MeshSubsets);

            if (ivoSubsets.Any())
                maxFromIvoSubsets = (uint)(ivoSubsets.Max(x => x.MatID) + 1);
        }

        var result = Math.Max(Math.Max(maxFromMeshSubsets, maxFromMtlNames), maxFromIvoSubsets);
        return Math.Max(result, 1u); // Ensure at least 1 material
    }

    private Material CreateDefaultMaterialSet(uint maxNumberOfMaterials)
    {
        var materials = new Material
        {
            SubMaterials = new Material[maxNumberOfMaterials],
            Name = Name
        };

        var random = new Random();
        for (var i = 0; i < maxNumberOfMaterials; i++)
        {
            // Generate random RGB values between 0.0 and 1.0
            var r = random.NextSingle();
            var g = random.NextSingle();
            var b = random.NextSingle();

            materials.SubMaterials[i] = MaterialUtilities.CreateDefaultMaterial(
                $"material{i}",
                $"{r:F3},{g:F3},{b:F3}");
        }

        return materials;
    }

    private void CreateAnimations()
    {
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
            Log.I("Unable to find associated animation track database file.");
        }
    }

    private GeometryInfo BuildNodeGeometryInfo(ChunkIvoSkinMesh skinMesh, IEnumerable<MeshSubset> subsets)
    {
        var geometryMeshDetails = skinMesh.MeshDetails;

        return new()
        {
            BoundingBox = new(geometryMeshDetails.BoundingBox.Min, geometryMeshDetails.BoundingBox.Max),
            GeometrySubsets = subsets.ToList(),
            Indices = skinMesh.Indices,
            Colors = skinMesh.Colors,
            VertUVs = skinMesh.VertsUvs,
            Normals = skinMesh.Normals,
            BoneMappings = skinMesh.BoneMappings
        };
    }

    private void AssignMaterialsToNodes(bool mtlFilesProvided = true)
    {
        //foreach (var node in Nodes.Where(x => x.MaterialID != 0))
        foreach (var node in Nodes)
        {
            if (mtlFilesProvided && MaterialFiles.Count == 1)
            {
                // Special case: single material file provided
                node.MaterialFileName = Path.GetFileNameWithoutExtension(MaterialFiles[0]);
                node.Materials = Materials.Values.First();
                continue;
            }

            // General case: Resolve material based on MaterialID
            var mtlNameChunk = Chunks.OfType<ChunkMtlName>().Where(x => x.ID == node.MaterialID).FirstOrDefault();
            var mtlNameKey = Path.GetFileNameWithoutExtension(mtlNameChunk?.Name) ?? "default";

            if (Materials.ContainsKey(mtlNameKey))
            {
                node.MaterialFileName = mtlNameKey;
                node.Materials = Materials[mtlNameKey];
            }
            else
            {
                node.MaterialFileName = Materials.FirstOrDefault().Key;
                node.Materials = Materials.FirstOrDefault().Value;
            }
        }
    }

    // Cache for material file paths to avoid repeated lookups
    private readonly Dictionary<string, string?> _materialPathCache = new(StringComparer.InvariantCultureIgnoreCase);
    
    private string? GetFullMaterialFilePath(string materialFile)
    {
        // Check cache first
        if (_materialPathCache.TryGetValue(materialFile, out var cachedPath))
            return cachedPath;
            
        if (!materialFile.EndsWith(".mtl", StringComparison.InvariantCultureIgnoreCase))
            materialFile += ".mtl";

        string? result = null;
        
        // 1. Check if file exists as provided (direct path)
        if (PackFileSystem.Exists(materialFile))
            result = materialFile;
            
        // 2. Check if it's in the same directory as the model file
        if (result == null)
        {
            var modelPath = Path.GetDirectoryName(InputFile);
            if (modelPath is not null)
            {
                var modelDirPath = Path.Combine(modelPath, materialFile);
                if (PackFileSystem.Exists(modelDirPath))
                    result = modelDirPath;
            }
        }
            
        // 3. Check if relative to objectdir if provided
        if (result == null && ObjectDir is not null)
        {
            var objectDirPath = Path.Combine(ObjectDir, materialFile);
            if (PackFileSystem.Exists(objectDirPath))
                result = objectDirPath;
        }
        
        // Cache the result (even if null) to avoid repeated lookups
        _materialPathCache[materialFile] = result;
        return result;
    }

    public bool IsIvoFile => Models.First().FileSignature?.Equals("#ivo") ?? false;

    public static bool SupportsFile(string name) => validExtensions.Contains(Path.GetExtension(name).ToLowerInvariant());

    private Datastream<T>? GetDatastream<T>(int chunkId)
    {
        var chunk = Models[^1].ChunkMap[chunkId] as ChunkDataStream;
        return chunk?.Data as Datastream<T>;
    }

    private Datastream<T>? GetRequiredDatastream<T>(int chunkId)
    {
        if (chunkId != 0)
            return GetDatastream<T>(chunkId);

        return null;
    }
}
