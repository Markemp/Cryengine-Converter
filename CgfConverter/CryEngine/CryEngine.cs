using CgfConverter.CryEngineCore;
using CgfConverter.CryXmlB;
using CgfConverter.Models;
using CgfConverter.PackFileSystem;
using CgfConverter.Utils;
using Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Linq;
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

    protected readonly TaggedLogger Log;

    public string Name => Path.GetFileNameWithoutExtension(InputFile).ToLower();
    public List<Model> Models { get; internal set; } = []; // All the model files associated with this game object
    public List<Model> Animations { get; internal set; } = [];  // Animation files for this object
    public List<ChunkNode> Nodes { get; internal set; } = []; // node hierarchy.
    public ChunkNode RootNode { get; internal set; } // can get node hierarchy from here
    public ChunkCompiledBones Bones { get; internal set; }  // move to skinning info
    public SkinningInfo? SkinningInfo { get; set; }
    public string InputFile { get; internal set; }
    public IPackFileSystem PackFileSystem { get; internal set; }
    public List<string>? MaterialFiles { get; set; } = [];
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
        MaterialFiles = materialFiles?.Split(',').ToList();
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
        BuildNodeStructure(); // new way to build the geometry to remove dependency on models

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
            if (Models[0].ChunkMap.Values.Any(c => c.ChunkType == ChunkType.NodeMeshCombo))
            {
                // SkinMesh has the mesh and meshsubset info, as well as all the datastreams
                var skinMesh = Models[1].ChunkMap.Values.FirstOrDefault(x => x.ChunkType == ChunkType.IvoSkin || x.ChunkType == ChunkType.IvoSkin2) as ChunkIvoSkinMesh;
                var geometryMeshDetails = skinMesh.MeshDetails;

                var comboChunk = Models[0].ChunkMap.Values
                    .Where(c => c.ChunkType == ChunkType.NodeMeshCombo)
                    .Select(x => x as ChunkNodeMeshCombo)
                    .First();  // only one nodemeshcombo chunk per file

                var stringTable = comboChunk.NodeNames;
                var materialTable = comboChunk.MaterialIndices;
                var materialFileName = Materials.Keys.First();

                // create node chunks
                foreach (var node in comboChunk.NodeMeshCombos)
                {
                    var index = comboChunk.NodeMeshCombos.IndexOf(node);

                    // Create meshsubsets for this node.  This is all meshSubsets where the meshParent equals
                    // the node index.
                    var subsets = skinMesh.MeshSubsets.Where(x => x.NodeParentIndex == index).ToList();

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

        foreach(var chunk in models.SelectMany(x => x.ChunkMap.Values))
        {
            if (chunk is ChunkCompiledExtToIntMap extToIntMap)
                skin.Ext2IntMap = extToIntMap.Source?.ToList();

            if (chunk is ChunkCompiledBones compiledBones)
                skin.CompiledBones = compiledBones.BoneList;

            if (chunk is ChunkCompiledIntFaces intFaces)
                skin.IntFaces = intFaces.Faces?.ToList();

            if (chunk is ChunkCompiledIntSkinVertices intVertices)
                skin.IntVertices = intVertices.IntSkinVertices?.ToList();

            //if (chunk is ChunkCompiledLookDirectionBlends lookDirBlends)
            //    skin.LookDirectionBlends = lookDirBlends.Blends;

            //if (chunk is ChunkCompiledMorphTargets morphTargets)
            //    skin.MorphTargets = morphTargets.MorphTargetVertices?.ToList();

            //if (chunk is ChunkCompiledPhysicalBones physicalBoneMeshes)
            //    skin.PhysicalBoneMeshes = physicalBoneMeshes.  .Meshes;

            //if (chunk is ChunkBoneEntities boneEntities)
            //    skin.BoneEntities = boneEntities.Entities;

            //if (chunk is ChunkBoneMappings boneMappings)
            //    skin.BoneMappings = boneMappings.Mappings;

            //if (chunk is ChunkCollisions collisions)
            //    skin.Collisions = collisions.Data;
        }

        return skin;
    }

    private void CreateMaterials()
    {
        // if -mtl arg is used, try to find the material files, set to the full path, and create materials from it.
        if (MaterialFiles is not null)
        {
            foreach (var materialFile in MaterialFiles)
            {
                var key = Path.GetFileNameWithoutExtension(materialFile);
                var fullyQualifiedMaterialFile = GetFullMaterialFilePath(materialFile);
                if (fullyQualifiedMaterialFile is not null)
                {
                    var materials = MaterialUtilities.FromStream(PackFileSystem.GetStream(fullyQualifiedMaterialFile), materialFile, true);
                    Materials.Add(key, materials);
                }
                else
                    Log.W("Unable to find provided material file {0}.  Checking material library chunks for materials.", materialFile);
            }
            AssignMaterialsToNodes();
            return;
        }

    // No material files provided.  Check the material library chunks for materials.
    var materialLibraryFiles = Models[0].ChunkMap.Values
        .OfType<ChunkMtlName>()
        .Where(x => x.MatType == MtlNameType.Library || x.MatType == MtlNameType.Basic || x.MatType == MtlNameType.Single)
        .Select(x => x.Name);

        Log.I("Found following potential material files.  If you are not specifying a material file and the materials don't" +
            " look right, trying one of the following files:");
        foreach (var file in materialLibraryFiles)
            Log.I($"   {file}");

        MaterialFiles = GetMaterialFilesFromMatLibraryChunks(materialLibraryFiles)?.ToList();

        if (MaterialFiles is not null)
        {
            foreach (var materialFile in MaterialFiles)
            {
                var key = Path.GetFileNameWithoutExtension(materialFile);
                var fullyQualifiedMaterialFile = GetFullMaterialFilePath(materialFile);
                if (fullyQualifiedMaterialFile is not null)
                {
                    var materials = MaterialUtilities.FromStream(PackFileSystem.GetStream(fullyQualifiedMaterialFile), materialFile, true);
                    Materials.Add(key, materials);
                }
            }
        }

        // Unable to find any materials.  Create default mats for each library file.
        if (Materials.Count == 0)
        {
            var meshSubsets = Models.Last().ChunkMap.Values.OfType<ChunkMeshSubsets>()
                .SelectMany(c => c.MeshSubsets);

            var maxChildren = Models
                .SelectMany(x => x.ChunkMap.Values.OfType<ChunkMtlName>())
                .Select(x => x.NumChildren)
                .Max();

            var maxMats = (uint)(meshSubsets.Select(x => x.MatID).DefaultIfEmpty(0).Max() + 1);
            // set maxMats to the max of maxMats and maxChildren
            maxMats = Math.Max(maxMats, maxChildren);

            foreach (var materialFile in materialLibraryFiles)
            {
                var key = Path.GetFileNameWithoutExtension(materialFile);

                if (!Materials.ContainsKey(key))
                {
                    MaterialFiles.Add(materialFile);
                    var materials = CreateDefaultMaterials(maxMats);
                    Materials.Add(key, materials);
                }
            }
        }
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
            Log.I("Unable to find associated animation track database file for {0}", InputFile);
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
        foreach (var node in Nodes.Where(x => x.MaterialID != 0))
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

    private string? GetFullMaterialFilePath(string materialFile)
    {
        if (!materialFile.EndsWith(".mtl"))
            materialFile += ".mtl";

        // check if file exists as provided
        if (PackFileSystem.Exists(materialFile))
            return materialFile;

        // Check if it's in the same directory as the model file
        var modelPath = Path.GetDirectoryName(InputFile);
        if (modelPath is not null && PackFileSystem.Exists(FileHandlingExtensions.CombineAndNormalizePath(modelPath, materialFile)))
            return FileHandlingExtensions.CombineAndNormalizePath(modelPath, materialFile);

        // check if relative to objectdir if provided
        if (ObjectDir is not null && PackFileSystem.Exists(FileHandlingExtensions.CombineAndNormalizePath(ObjectDir, materialFile)))
            return FileHandlingExtensions.CombineAndNormalizePath(ObjectDir, materialFile);

        // unable to find material file.
        return null;
    }

    private Material CreateDefaultMaterials(uint maxNumberOfMaterials)
    {
        var materials = new Material
        {
            SubMaterials = new Material[maxNumberOfMaterials],
            Name = Name
        };

        for (var i = 0; i < maxNumberOfMaterials; i++)
            materials.SubMaterials[i] = MaterialUtilities.CreateDefaultMaterial($"material{i}", $"{i / (float)maxNumberOfMaterials},0.5,0.5");

        return materials;
    }

    // Gets the Library material file if one can be found. File will be the name in the library as it's the dictionary key.
    private HashSet<string>? GetMaterialFilesFromMatLibraryChunks(IEnumerable<string> libraryFileNames)
    {
        HashSet<string> materialFiles = [];
        if (libraryFileNames is not null)
        {
            foreach (var libraryFile in libraryFileNames)
            {
                var testFile = libraryFile;
                if (!testFile.EndsWith(".mtl"))
                    testFile += ".mtl";

                if (PackFileSystem.Exists(testFile))
                {
                    materialFiles.Add(libraryFile);
                    continue;
                }

                // Check if testFile + object dir exists
                var fullObjectDirPath = FileHandlingExtensions.CombineAndNormalizePath(ObjectDir, testFile);
                if (PackFileSystem.Exists(fullObjectDirPath))
                {
                    materialFiles.Add(libraryFile);
                    continue;
                }

                // Check in fully qualified current dir
                var fullPath = FileHandlingExtensions.CombineAndNormalizePath(Path.GetDirectoryName(InputFile), $"{libraryFile}.mtl");
                if (PackFileSystem.Exists(fullPath))
                {
                    materialFiles.Add(libraryFile);
                    continue;
                }
            }
            return materialFiles;
        }

        Log.W("Unable to find material file for {0}", InputFile);
        return null;
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
