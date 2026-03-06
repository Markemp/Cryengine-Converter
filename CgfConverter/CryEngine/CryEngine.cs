using CgfConverter.Cal;
using CgfConverter.CryEngineCore;
using CgfConverter.CryXmlB;
using CgfConverter.Models;
using CgfConverter.Models.Structs;
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
    public List<Model> Animations { get; internal set; } = [];  // Animation files for this object (DBA format)
    public List<CafAnimation> CafAnimations { get; internal set; } = [];  // CAF animation files
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

    private readonly bool _includeAnimations;

    public CryEngine(string filename, IPackFileSystem packFileSystem,
        CryEngineOptions? options = null, TaggedLogger? parentLogger = null)
    {
        Log = new TaggedLogger(Path.GetFileName(filename), parentLogger);
        InputFile = filename;
        PackFileSystem = packFileSystem;
        MaterialFiles = string.IsNullOrEmpty(options?.MaterialFiles) ? [] : options.MaterialFiles.Split(',').ToList();
        ObjectDir = options?.ObjectDir;
        _includeAnimations = options?.IncludeAnimations ?? false;
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

        if (_includeAnimations)
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
            if (Models.Count == 0)
            {
                Log.W("No models loaded - cannot build node structure");
                return;
            }

            bool hasValidNodeMeshCombo = false;
            var comboChunk = (ChunkNodeMeshCombo?)Models[0].ChunkMap.Values.FirstOrDefault(c => c.ChunkType == ChunkType.NodeMeshCombo);

            if (comboChunk is not null && comboChunk.NumberOfNodes != 0)
                hasValidNodeMeshCombo = true;

            if (hasValidNodeMeshCombo)
            {
                // SkinMesh has the mesh and meshsubset info, as well as all the datastreams
                // First try to find IvoSkin in the second model (companion .cgam file)
                // If not found, also check the first model in case of combined files
                ChunkIvoSkinMesh? skinMesh = null;
                if (Models.Count > 1)
                {
                    skinMesh = Models[1].ChunkMap.Values.FirstOrDefault(x => x.ChunkType == ChunkType.IvoSkin || x.ChunkType == ChunkType.IvoSkin2) as ChunkIvoSkinMesh;
                }
                if (skinMesh is null)
                {
                    skinMesh = Models[0].ChunkMap.Values.FirstOrDefault(x => x.ChunkType == ChunkType.IvoSkin || x.ChunkType == ChunkType.IvoSkin2) as ChunkIvoSkinMesh;
                }

                var geometryMeshDetails = skinMesh?.MeshDetails;

                var stringTable = comboChunk?.NodeNames ?? [];
                var materialTable = comboChunk?.MaterialIndices ?? [];
                var materialFileName = Materials.Keys.FirstOrDefault() ?? "default";

                // Check if stringTable has correct number of entries
                if (stringTable.Count < comboChunk.NumberOfNodes)
                {
                    Log.W($"NodeMeshCombo has {comboChunk.NumberOfNodes} nodes but only {stringTable.Count} names in string table");
                }

                // create node chunks
                if (comboChunk.NodeMeshCombos is null || comboChunk.NodeMeshCombos.Count == 0)
                {
                    Log.W("NodeMeshCombo has no node data");
                    return;
                }

                // Pre-group mesh subsets by NodeParentIndex to avoid O(n*m) lookups in the loop
                var subsetsByNodeIndex = new Dictionary<int, List<MeshSubset>>();
                if (skinMesh?.MeshSubsets != null)
                {
                    foreach (var subset in skinMesh.MeshSubsets)
                    {
                        int nodeIndex = (int)(subset.NodeParentIndex ?? 0);
                        if (!subsetsByNodeIndex.TryGetValue(nodeIndex, out var list))
                        {
                            list = new List<MeshSubset>();
                            subsetsByNodeIndex[nodeIndex] = list;
                        }
                        list.Add(subset);
                    }
                }

                for (int index = 0; index < comboChunk.NodeMeshCombos.Count; index++)
                {
                    var node = comboChunk.NodeMeshCombos[index];

                    // Get meshsubsets for this node from pre-grouped dictionary (O(1) lookup)
                    var subsets = subsetsByNodeIndex.TryGetValue(index, out var nodeSubsets) ? nodeSubsets : [];

                    ChunkMesh chunkMesh = new ChunkMesh_802();

                    var hasGeometry = subsets.Count != 0 && geometryMeshDetails is not null && skinMesh is not null;

                    // Create a meshchunk for nodes with geometry
                    if (hasGeometry)
                    {
                        chunkMesh.ScalingVectors = geometryMeshDetails!.ScalingBoundingBox;
                        chunkMesh.MaxBound = node.BoundingBoxMax;
                        chunkMesh.MinBound = node.BoundingBoxMin;
                        chunkMesh.NumVertices = (int)skinMesh!.MeshDetails.NumberOfVertices;
                        chunkMesh.NumIndices = (int)skinMesh.MeshDetails.NumberOfIndices;
                        chunkMesh.NumVertSubsets = skinMesh.MeshDetails.NumberOfSubmeshes;
                        chunkMesh.GeometryInfo = BuildNodeGeometryInfo(skinMesh, subsets);
                    }

                    // Use node name from string table if available, otherwise generate a name
                    var nodeName = index < stringTable.Count ? stringTable[index] : $"node_{index}";

                    var newNode = new ChunkNode_823
                    {
                        Name = nodeName,
                        ObjectNodeID = -1,
                        ParentNodeIndex = node.ParentIndex,
                        ParentNodeID = node.ParentIndex == 0xffff ? -1 : node.ParentIndex,
                        NumChildren = node.NumberOfChildren,
                        // Get material ID from mesh subsets (not materialTable which uses mesh subset indices, not node indices)
                        MaterialID = hasGeometry ? subsets[0].MatID : 0,
                        Transform = node.BoneToWorld.ConvertToLocalTransformMatrix(),
                        ChunkType = ChunkType.Node,
                        ID = (int)node.Id,
                        MeshData = hasGeometry ? chunkMesh : null,
                        MaterialFileName = materialFileName
                    };

                    if (hasGeometry && Materials.Count > 0)
                        newNode.Materials = Materials.Values.First();

                    Nodes.Add(newNode);
                }

                // build node hierarchy
                for (int index = 0; index < Nodes.Count; index++)
                {
                    var node = Nodes[index];
                    if (node.ParentNodeIndex != 0xFFFF && node.ParentNodeIndex >= 0 && node.ParentNodeIndex < Nodes.Count)
                        node.ParentNode = Nodes[node.ParentNodeIndex];
                    else if (node.ParentNodeIndex == 0xFFFF || node.ParentNodeIndex == -1)
                        RootNode = node; // hopefully there is just one
                    else
                        Log.W($"Node {node.Name} has invalid parent index {node.ParentNodeIndex}");

                    // Add all child nodes to Children.  A child is where the parent index is current index
                    // Optimize: iterate once instead of using LINQ Where which creates intermediate collections
                    foreach (var childNode in Nodes)
                    {
                        if (childNode.ParentNodeIndex == index)
                            node.Children.Add(childNode);
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
            // Pre-group children by parent ID to avoid O(n²) lookups
            var childrenByParentId = new Dictionary<int, List<ChunkNode>>();
            foreach (var childNode in Models[0].NodeMap.Values)
            {
                if (childNode.ParentNodeID != -1 && childNode.ParentNodeID != ~0)
                {
                    if (!childrenByParentId.TryGetValue(childNode.ParentNodeID, out var children))
                    {
                        children = new List<ChunkNode>();
                        childrenByParentId[childNode.ParentNodeID] = children;
                    }
                    children.Add(childNode);
                }
            }

            // Separate datastream for each node.
            // For each ChunkNode in model[0], add it to the Nodes list.
            foreach (var node in Models[0].NodeMap.Values)
            {
                // Add helper or mesh data to the node
                var objectChunk = Models[0].ChunkMap[node.ObjectNodeID];
                // Get children from pre-grouped dictionary (O(1) lookup)
                node.Children = childrenByParentId.TryGetValue(node.ID, out var nodeChildren) ? nodeChildren : new List<ChunkNode>();

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

        // For Ivo format files, the bone indices in BoneMappings refer to NodeMeshCombo indices,
        // not CompiledBone indices. We need to remap using ObjectNodeIndex.
        // Build a map: ObjectNodeIndex (NodeMeshCombo index) → bone index (position in CompiledBones)
        if (skin.BoneMappings is not null && skin.CompiledBones is not null)
        {
            var nodeMeshComboToBoneIndex = new Dictionary<int, int>();
            for (int boneIndex = 0; boneIndex < skin.CompiledBones.Count; boneIndex++)
            {
                var bone = skin.CompiledBones[boneIndex];
                // ObjectNodeIndex maps bone to its corresponding NodeMeshCombo
                // Only add if ObjectNodeIndex is valid (some bones may not have mesh associations)
                if (bone.ObjectNodeIndex >= 0)
                {
                    nodeMeshComboToBoneIndex[bone.ObjectNodeIndex] = boneIndex;
                }
            }

            // Only remap if we found any ObjectNodeIndex mappings (indicates Ivo format)
            if (nodeMeshComboToBoneIndex.Count > 0)
            {
                for (int i = 0; i < skin.BoneMappings.Count; i++)
                {
                    var mapping = skin.BoneMappings[i];
                    var remappedBoneIndex = new ushort[4];
                    for (int j = 0; j < 4; j++)
                    {
                        int nodeMeshComboIndex = mapping.BoneIndex[j];
                        if (nodeMeshComboToBoneIndex.TryGetValue(nodeMeshComboIndex, out int actualBoneIndex))
                        {
                            remappedBoneIndex[j] = (ushort)actualBoneIndex;
                        }
                        else
                        {
                            // Keep original index if no mapping found (fallback)
                            remappedBoneIndex[j] = (ushort)nodeMeshComboIndex;
                        }
                    }
                    skin.BoneMappings[i] = new MeshBoneMapping
                    {
                        BoneInfluenceCount = mapping.BoneInfluenceCount,
                        BoneIndex = remappedBoneIndex,
                        Weight = mapping.Weight
                    };
                }
            }
        }

        return skin;
    }


    private void CreateMaterials()
    {
        if (TryLoadExplicitMaterialFiles())
            return;

        var libraryFiles = GetMaterialLibraryFileNames().ToList();

        if (libraryFiles.Any())
        {
            var loadedAny = false;
            foreach (var libraryFile in libraryFiles)
            {
                var key = Path.GetFileNameWithoutExtension(libraryFile) ?? "unknown";

                if (TryLoadSingleMaterialFile(libraryFile))
                {
                    if (!MaterialFiles.Contains(libraryFile))
                        MaterialFiles.Add(libraryFile);
                    loadedAny = true;
                    continue;
                }

                if (TryLoadModelNameMaterialFile(key))
                {
                    loadedAny = true;
                    continue;
                }

                Log.W("Could not find material file for library: {0}, skipping", key);
            }

            // Only create dummy materials if nothing at all could be loaded
            if (!loadedAny)
            {
                foreach (var libraryFile in libraryFiles)
                {
                    var key = Path.GetFileNameWithoutExtension(libraryFile) ?? "unknown";
                    CreateDefaultMaterialsForLibrary(key);
                }
            }
        }
        else
        {
            if (!TryLoadModelNameMaterialFile(null))
                CreateDefaultMaterials();
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

    private bool TryLoadModelNameMaterialFile(string? libraryKey)
    {
        var modelName = Name;
        var key = libraryKey ?? modelName;

        if (Materials.ContainsKey(key))
            return true;

        var fullyQualifiedPath = GetFullMaterialFilePath(modelName);
        if (fullyQualifiedPath is null)
            return false;

        try
        {
            var material = MaterialUtilities.FromStream(
                PackFileSystem.GetStream(fullyQualifiedPath),
                modelName,
                ObjectDir,
                closeAfter: true);

            Materials.Add(key, material);
            if (!MaterialFiles.Contains(key))
                MaterialFiles.Add(key);

            Log.I("Loaded model-name material file: {0} (stored as '{1}')", modelName, key);
            return true;
        }
        catch (Exception ex)
        {
            Log.W("Failed to load model-name material file {0}: {1}", modelName, ex.Message);
            return false;
        }
    }

    private void CreateDefaultMaterialsForLibrary(string key)
    {
        if (Materials.ContainsKey(key))
            return;

        Log.W("Creating dummy materials for library: {0}", key);
        var maxMaterials = CalculateMaxMaterialCount();
        Materials.Add(key, CreateDefaultMaterialSet(maxMaterials));
        if (!MaterialFiles.Contains(key))
            MaterialFiles.Add(key);
    }

    private void CreateDefaultMaterials()
    {
        Log.W("Unable to find any material files for this model. Creating dummy materials.");

        var defaultKey = "default";
        var maxMats = CalculateMaxMaterialCount();
        var defaultMaterial = CreateDefaultMaterialSet(maxMats);
        Materials.Add(defaultKey, defaultMaterial);
        MaterialFiles.Add(defaultKey);
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
        var modelDir = Path.GetDirectoryName(InputFile);

        // Try chrparams first (XML format used by MWO, Star Citizen, etc.)
        if (TryLoadChrParamsAnimations(modelDir))
            return;

        // Fall back to .cal files (ArcheAge format)
        if (TryLoadCalAnimations(modelDir))
            return;

        Log.D("No animation configuration files found (.chrparams or .cal)");
    }

    /// <summary>
    /// Attempts to load animations from a .chrparams file.
    /// </summary>
    private bool TryLoadChrParamsAnimations(string? modelDir)
    {
        try
        {
            var chrparamsFileName = Path.ChangeExtension(Path.GetFileName(InputFile), ".chrparams");
            var chrparamsPath = string.IsNullOrEmpty(modelDir)
                ? chrparamsFileName
                : Path.Combine(modelDir, chrparamsFileName);

            Log.D("Looking for chrparams at: {0}", chrparamsPath);

            var chrparams = CryXmlSerializer.Deserialize<ChrParams.ChrParams>(
                PackFileSystem.GetStream(chrparamsPath));

            Log.D("Successfully loaded chrparams, animations count: {0}", chrparams.Animations?.Length ?? 0);

            // Try to load DBA database first (preferred for batch animations)
            var trackFilePath = chrparams.Animations?.FirstOrDefault(x => x.Name == "$TracksDatabase")?.Path;
            if (trackFilePath is not null)
            {
                if (Path.GetExtension(trackFilePath) != ".dba")
                    trackFilePath = Path.ChangeExtension(trackFilePath, ".dba");

                Log.D("Attempting to load animation database from: {0}", trackFilePath);

                // Check if this is a wildcard pattern
                if (trackFilePath.Contains('*') || trackFilePath.Contains('?'))
                {
                    var dbaFiles = ExpandDbaWildcard(trackFilePath);
                    Log.D("Wildcard pattern '{0}' matched {1} DBA files", trackFilePath, dbaFiles.Count);

                    foreach (var dbaPath in dbaFiles)
                    {
                        try
                        {
                            Animations.Add(Model.FromStream(dbaPath, PackFileSystem.GetStream(dbaPath), true));
                            Log.D("Successfully loaded animation database: {0}", dbaPath);
                        }
                        catch (Exception ex)
                        {
                            Log.D("Error loading DBA file {0}: {1}", dbaPath, ex.Message);
                        }
                    }
                }
                else
                {
                    // Resolve relative paths against ObjectDir
                    var fullDbaPath = trackFilePath;
                    if (!Path.IsPathRooted(fullDbaPath) && !string.IsNullOrEmpty(ObjectDir))
                    {
                        fullDbaPath = Path.Combine(ObjectDir, fullDbaPath);
                    }

                    try
                    {
                        Animations.Add(Model.FromStream(fullDbaPath, PackFileSystem.GetStream(fullDbaPath), true));
                        Log.D("Successfully loaded animation database: {0}", fullDbaPath);
                    }
                    catch (FileNotFoundException)
                    {
                        Log.D("DBA file not found: {0}", fullDbaPath);
                    }
                }
            }

            // Also load individual CAF files from animation entries
            LoadCafAnimations(chrparams);
            return true;
        }
        catch (FileNotFoundException)
        {
            Log.D("No chrparams file found");
            return false;
        }
    }

    /// <summary>
    /// Attempts to load animations from a .cal file (ArcheAge format).
    /// </summary>
    private bool TryLoadCalAnimations(string? modelDir)
    {
        try
        {
            var calFileName = Path.ChangeExtension(Path.GetFileName(InputFile), ".cal");
            var calPath = string.IsNullOrEmpty(modelDir)
                ? calFileName
                : Path.Combine(modelDir, calFileName);

            Log.D("Looking for cal file at: {0}", calPath);

            var calFile = CalFile.ParseWithIncludes(calPath, PackFileSystem);

            Log.D("Successfully loaded cal file, filepath: {0}, animations count: {1}",
                calFile.FilePath ?? "(none)", calFile.Animations.Count);

            if (calFile.Animations.Count == 0)
            {
                Log.D("No animations found in cal file");
                return false;
            }

            // Load CAF files from cal entries
            LoadCafAnimationsFromCal(calFile);
            return true;
        }
        catch (FileNotFoundException)
        {
            Log.D("No cal file found");
            return false;
        }
    }

    /// <summary>
    /// Loads CAF animation files from a parsed .cal file.
    /// </summary>
    private void LoadCafAnimationsFromCal(CalFile calFile)
    {
        var basePath = calFile.FilePath ?? "";
        Log.D("CAF base path from cal: {0}", basePath);

        foreach (var (name, relativePath) in calFile.Animations)
        {
            try
            {
                // Build full path
                var cafPath = relativePath;
                if (!Path.IsPathRooted(cafPath) && !string.IsNullOrEmpty(basePath))
                {
                    cafPath = Path.Combine(basePath, cafPath);
                }

                // Ensure .caf extension
                if (!cafPath.EndsWith(".caf", StringComparison.OrdinalIgnoreCase))
                    cafPath = Path.ChangeExtension(cafPath, ".caf");

                // Try loading with multiple path resolutions
                if (!TryLoadCafWithPathVariants(cafPath, name))
                {
                    Log.D("CAF file not found: {0}", cafPath);
                }
            }
            catch (Exception ex)
            {
                Log.D("Error loading CAF {0}: {1}", name, ex.Message);
            }
        }

        if (CafAnimations.Count > 0)
            Log.I("Loaded {0} CAF animation(s) from cal file", CafAnimations.Count);
    }

    /// <summary>
    /// Tries to load a CAF file with multiple path variants (for ArcheAge's "game" subdirectory).
    /// </summary>
    private bool TryLoadCafWithPathVariants(string cafPath, string animationName)
    {
        // Try path as-is first
        if (TryLoadSingleCafFile(cafPath, animationName))
            return true;

        // Try with "game\" prefix (ArcheAge stores files under game/ subdirectory)
        var gamePathPrefixed = Path.Combine("game", cafPath);
        if (TryLoadSingleCafFile(gamePathPrefixed, animationName))
            return true;

        return false;
    }

    /// <summary>
    /// Attempts to load a single CAF file. Returns true on success, false if file not found.
    /// </summary>
    private bool TryLoadSingleCafFile(string cafPath, string animationName)
    {
        // Resolve relative paths against ObjectDir
        var fullPath = cafPath;
        if (!Path.IsPathRooted(fullPath) && !string.IsNullOrEmpty(ObjectDir))
        {
            fullPath = Path.Combine(ObjectDir, fullPath);
        }

        // Check if file exists via PackFileSystem
        try
        {
            var cafModel = Model.FromStream(fullPath, PackFileSystem.GetStream(fullPath), true);
            var cafAnimation = ParseCafModel(cafModel, animationName, fullPath);

            if (cafAnimation is not null)
            {
                CafAnimations.Add(cafAnimation);
                Log.D("Successfully loaded CAF animation: {0} from {1}", animationName, fullPath);
                return true;
            }
            return false;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Log.D("Error loading CAF {0}: {1}", fullPath, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Loads individual CAF animation files listed in chrparams.
    /// </summary>
    private void LoadCafAnimations(ChrParams.ChrParams chrparams)
    {
        if (chrparams.Animations is null)
            return;

        // Get base path for CAF files (from #filepath entry)
        var basePath = chrparams.Animations.FirstOrDefault(x => x.Name == "#filepath")?.Path ?? "";
        Log.D("CAF base path: {0}", basePath);

        // Find all animation entries that aren't special directives
        var cafEntries = chrparams.Animations
            .Where(a => !string.IsNullOrEmpty(a.Name)
                     && !a.Name.StartsWith("$")
                     && !a.Name.StartsWith("#")
                     && !string.IsNullOrEmpty(a.Path))
            .ToList();

        Log.D("Found {0} potential CAF animation entries", cafEntries.Count);

        foreach (var entry in cafEntries)
        {
            try
            {
                // Build full path pattern
                var cafPattern = entry.Path!;
                if (!Path.IsPathRooted(cafPattern) && !string.IsNullOrEmpty(basePath))
                {
                    cafPattern = Path.Combine(basePath, cafPattern);
                }

                // Check if this is a wildcard pattern
                if (cafPattern.Contains('*') || cafPattern.Contains('?'))
                {
                    // Expand wildcard pattern to find matching files
                    var cafFiles = ExpandCafWildcard(cafPattern);
                    Log.D("Wildcard pattern '{0}' matched {1} files", cafPattern, cafFiles.Count);

                    foreach (var cafPath in cafFiles)
                    {
                        LoadSingleCafFile(cafPath);
                    }
                }
                else
                {
                    // Single file path
                    if (Path.GetExtension(cafPattern).ToLowerInvariant() != ".caf")
                        cafPattern = Path.ChangeExtension(cafPattern, ".caf");

                    LoadSingleCafFile(cafPattern, entry.Name!);
                }
            }
            catch (Exception ex)
            {
                Log.D("Error processing CAF entry {0}: {1}", entry.Path, ex.Message);
            }
        }

        if (CafAnimations.Count > 0)
            Log.I("Loaded {0} CAF animation(s)", CafAnimations.Count);
    }

    /// <summary>
    /// Expands a wildcard pattern to find matching CAF files.
    /// Handles wildcards in both directory path (e.g., "path/*/*.caf") and filename (e.g., "path/*.caf").
    /// </summary>
    private List<string> ExpandCafWildcard(string pattern)
    {
        var results = new List<string>();

        try
        {
            // Normalize path separators
            pattern = pattern.Replace('\\', '/');

            // Check if the directory portion contains wildcards
            var lastSlash = pattern.LastIndexOf('/');
            var filePattern = lastSlash >= 0 ? pattern[(lastSlash + 1)..] : pattern;
            var directoryPattern = lastSlash >= 0 ? pattern[..lastSlash] : "";

            // Find the first wildcard in the directory path
            var firstWildcardIndex = directoryPattern.IndexOfAny(['*', '?']);

            string baseDirectory;
            bool searchRecursively = false;

            if (firstWildcardIndex >= 0)
            {
                // There's a wildcard in the directory path - need to search recursively
                // Find the last slash before the wildcard to get the base directory
                var lastSlashBeforeWildcard = directoryPattern.LastIndexOf('/', firstWildcardIndex);
                baseDirectory = lastSlashBeforeWildcard >= 0
                    ? directoryPattern[..lastSlashBeforeWildcard]
                    : "";
                searchRecursively = true;
            }
            else
            {
                baseDirectory = directoryPattern;
            }

            // Resolve relative paths against ObjectDir
            if (!Path.IsPathRooted(baseDirectory) && !string.IsNullOrEmpty(ObjectDir))
            {
                baseDirectory = Path.Combine(ObjectDir, baseDirectory);
            }

            // Normalize again after Path.Combine
            baseDirectory = baseDirectory.Replace('\\', '/');

            if (Directory.Exists(baseDirectory))
            {
                var searchOption = searchRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var matchingFiles = Directory.GetFiles(baseDirectory, filePattern, searchOption);
                results.AddRange(matchingFiles.Where(f =>
                    f.EndsWith(".caf", StringComparison.OrdinalIgnoreCase)));

                Log.D("Searched '{0}' with pattern '{1}', recursive={2}, found {3} files",
                    baseDirectory, filePattern, searchRecursively, results.Count);
            }
            else
            {
                Log.D("Directory not found for wildcard expansion: {0}", baseDirectory);
            }
        }
        catch (Exception ex)
        {
            Log.D("Error expanding wildcard pattern {0}: {1}", pattern, ex.Message);
        }

        return results;
    }

    /// <summary>
    /// Expands a wildcard pattern to find matching DBA files.
    /// </summary>
    private List<string> ExpandDbaWildcard(string pattern)
    {
        var results = new List<string>();

        try
        {
            // Normalize path separators
            pattern = pattern.Replace('\\', '/');

            // Check if the directory portion contains wildcards
            var lastSlash = pattern.LastIndexOf('/');
            var filePattern = lastSlash >= 0 ? pattern[(lastSlash + 1)..] : pattern;
            var directoryPattern = lastSlash >= 0 ? pattern[..lastSlash] : "";

            // Find the first wildcard in the directory path
            var firstWildcardIndex = directoryPattern.IndexOfAny(['*', '?']);

            string baseDirectory;
            bool searchRecursively = false;

            if (firstWildcardIndex >= 0)
            {
                // There's a wildcard in the directory path - need to search recursively
                var lastSlashBeforeWildcard = directoryPattern.LastIndexOf('/', firstWildcardIndex);
                baseDirectory = lastSlashBeforeWildcard >= 0
                    ? directoryPattern[..lastSlashBeforeWildcard]
                    : "";
                searchRecursively = true;
            }
            else
            {
                baseDirectory = directoryPattern;
            }

            // Resolve relative paths against ObjectDir
            if (!Path.IsPathRooted(baseDirectory) && !string.IsNullOrEmpty(ObjectDir))
            {
                baseDirectory = Path.Combine(ObjectDir, baseDirectory);
            }

            // Normalize again after Path.Combine
            baseDirectory = baseDirectory.Replace('\\', '/');

            if (Directory.Exists(baseDirectory))
            {
                var searchOption = searchRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var matchingFiles = Directory.GetFiles(baseDirectory, filePattern, searchOption);
                results.AddRange(matchingFiles.Where(f =>
                    f.EndsWith(".dba", StringComparison.OrdinalIgnoreCase)));

                Log.D("Searched '{0}' with pattern '{1}', recursive={2}, found {3} DBA files",
                    baseDirectory, filePattern, searchRecursively, results.Count);
            }
            else
            {
                Log.D("Directory not found for DBA wildcard expansion: {0}", baseDirectory);
            }
        }
        catch (Exception ex)
        {
            Log.D("Error expanding DBA wildcard pattern {0}: {1}", pattern, ex.Message);
        }

        return results;
    }

    /// <summary>
    /// Loads a single CAF file.
    /// </summary>
    private void LoadSingleCafFile(string cafPath, string? animationName = null)
    {
        try
        {
            // Resolve relative paths against ObjectDir
            var fullPath = cafPath;
            if (!Path.IsPathRooted(fullPath) && !string.IsNullOrEmpty(ObjectDir))
            {
                fullPath = Path.Combine(ObjectDir, fullPath);
            }

            // Use filename without extension as animation name if not specified
            animationName ??= Path.GetFileNameWithoutExtension(fullPath);

            Log.D("Loading CAF: {0} (name: {1})", fullPath, animationName);

            var cafModel = Model.FromStream(fullPath, PackFileSystem.GetStream(fullPath), true);
            var cafAnimation = ParseCafModel(cafModel, animationName, fullPath);

            if (cafAnimation is not null)
            {
                CafAnimations.Add(cafAnimation);
                Log.D("Successfully loaded CAF animation: {0}", animationName);
            }
        }
        catch (FileNotFoundException)
        {
            Log.D("CAF file not found: {0}", cafPath);
        }
        catch (Exception ex)
        {
            Log.D("Error loading CAF {0}: {1}", cafPath, ex.Message);
        }
    }

    /// <summary>
    /// Parses a CAF Model into a CafAnimation structure.
    /// </summary>
    private CafAnimation? ParseCafModel(Model cafModel, string animationName, string filePath)
    {
        var animation = new CafAnimation
        {
            Name = animationName,
            FilePath = filePath
        };

        // Get timing info from timing chunk
        var timingChunk = cafModel.ChunkMap.Values.OfType<ChunkTimingFormat>().FirstOrDefault();
        if (timingChunk is not null)
        {
            animation.SecsPerTick = timingChunk.SecsPerTick;
            animation.TicksPerFrame = timingChunk.TicksPerFrame;
            animation.StartFrame = timingChunk.GlobalRange.Start;
            animation.EndFrame = timingChunk.GlobalRange.End;
        }

        // Check for additive animation flag from GlobalAnimationHeaderCAF chunk
        var animHeaderChunk = cafModel.ChunkMap.Values.OfType<ChunkGlobalAnimationHeaderCAF>().FirstOrDefault();
        if (animHeaderChunk is not null)
        {
            // AssetFlags.Additive = 0x001
            animation.IsAdditive = (animHeaderChunk.Flags & 0x001) != 0;
            if (animation.IsAdditive)
                Log.D($"CAF animation '{animationName}' is additive (flags=0x{animHeaderChunk.Flags:X})");
        }

        // Get bone name mapping from BoneNameList chunk (if present)
        var boneNameList = cafModel.ChunkMap.Values.OfType<ChunkBoneNameList>().FirstOrDefault();
        if (boneNameList is not null)
        {
            // Build CRC32 -> bone name mapping
            foreach (var boneName in boneNameList.BoneNames)
            {
                var crc = ComputeBoneNameCrc32(boneName);
                animation.ControllerIdToBoneName[crc] = boneName;
            }
        }

        // Process controller chunks (829/831 = compressed, 827/830 = uncompressed CryKeyPQLog)
        var controllers827 = cafModel.ChunkMap.Values.OfType<ChunkController_827>().ToList();
        var controllers829 = cafModel.ChunkMap.Values.OfType<CryEngineCore.Chunks.ChunkController_829>().ToList();
        var controllers830 = cafModel.ChunkMap.Values.OfType<ChunkController_830>().ToList();
        var controllers831 = cafModel.ChunkMap.Values.OfType<ChunkController_831>().ToList();

        // 827 and 830 use unified key times for both rotation and position
        foreach (var ctrl in controllers827)
        {
            var keyTimes = ctrl.KeyTimes.Select(t => (float)t).ToList();
            var track = new BoneTrack
            {
                ControllerId = ctrl.ControllerId,
                RotationKeyTimes = keyTimes,
                PositionKeyTimes = keyTimes,
                Positions = ctrl.KeyPositions.ToList(),
                Rotations = ctrl.KeyRotations.ToList()
            };
            animation.BoneTracks[ctrl.ControllerId] = track;
        }

        foreach (var ctrl in controllers830)
        {
            var keyTimes = ctrl.KeyTimes.Select(t => (float)t).ToList();
            var track = new BoneTrack
            {
                ControllerId = ctrl.ControllerId,
                RotationKeyTimes = keyTimes,
                PositionKeyTimes = keyTimes,
                Positions = ctrl.KeyPositions.ToList(),
                Rotations = ctrl.KeyRotations.ToList()
            };
            animation.BoneTracks[ctrl.ControllerId] = track;
        }

        // 829 and 831 have separate rotation/position key times
        // Key times are stored as actual frame numbers (byte/uint16/float format just determines storage size)
        foreach (var ctrl in controllers829)
        {
            var track = new BoneTrack
            {
                ControllerId = ctrl.ControllerId,
                RotationKeyTimes = ctrl.RotationKeyTimes.ToList(),
                PositionKeyTimes = ctrl.PositionKeyTimes.ToList(),
                Positions = ctrl.KeyPositions.ToList(),
                Rotations = ctrl.KeyRotations.ToList()
            };
            animation.BoneTracks[ctrl.ControllerId] = track;
        }

        foreach (var ctrl in controllers831)
        {
            var track = new BoneTrack
            {
                ControllerId = ctrl.ControllerId,
                RotationKeyTimes = ctrl.RotationKeyTimes.ToList(),
                PositionKeyTimes = ctrl.PositionKeyTimes.ToList(),
                Positions = ctrl.KeyPositions.ToList(),
                Rotations = ctrl.KeyRotations.ToList()
            };
            animation.BoneTracks[ctrl.ControllerId] = track;
        }

        // Process Star Citizen #ivo CAF chunks
        var ivoCAFs = cafModel.ChunkMap.Values.OfType<ChunkIvoCAF>().ToList();
        foreach (var ivoCaf in ivoCAFs)
        {
            // Each bone hash maps to rotation/position data
            foreach (var boneHash in ivoCaf.BoneHashes)
            {
                // Get rotation data
                ivoCaf.Rotations.TryGetValue(boneHash, out var rotations);
                ivoCaf.RotationTimes.TryGetValue(boneHash, out var rotTimes);
                ivoCaf.Positions.TryGetValue(boneHash, out var positions);
                ivoCaf.PositionTimes.TryGetValue(boneHash, out var posTimes);

                var track = new BoneTrack
                {
                    ControllerId = boneHash,
                    Rotations = rotations ?? [],
                    Positions = positions ?? [],
                    RotationKeyTimes = rotTimes ?? [],
                    PositionKeyTimes = posTimes ?? []
                };

                if (track.Rotations.Count > 0 || track.Positions.Count > 0)
                {
                    animation.BoneTracks[boneHash] = track;
                }
            }
        }

        if (animation.BoneTracks.Count == 0)
        {
            Log.D("No controller chunks found in CAF file: {0}", filePath);
            return null;
        }

        Log.D("Parsed CAF with {0} bone tracks, frames {1}-{2}",
            animation.BoneTracks.Count, animation.StartFrame, animation.EndFrame);

        return animation;
    }

    /// <summary>
    /// Computes CRC32 of a bone name (lowercase) for controller ID matching.
    /// </summary>
    private static uint ComputeBoneNameCrc32(string boneName)
    {
        // CryEngine uses lowercase CRC32 for bone names
        var bytes = System.Text.Encoding.ASCII.GetBytes(boneName.ToLowerInvariant());
        uint crc = 0xFFFFFFFF;

        foreach (byte b in bytes)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                crc = (crc >> 1) ^ (0xEDB88320 & ~((crc & 1) - 1));
            }
        }

        return ~crc;
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

    public bool IsIvoFile => Models.Count > 0 && (Models[0].FileSignature?.Equals("#ivo") ?? false);

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
