using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Models.Materials;
using CgfConverter.Models.Structs;
using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
using CgfConverter.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using static Extensions.FileHandlingExtensions;

namespace CgfConverter.Renderers.USD;
public class UsdRenderer : IRenderer
{
    protected readonly ArgsHandler _args;
    protected readonly CryEngine _cryData;

    private readonly FileInfo usdOutputFile;
    private UsdSerializer usdSerializer;
    private readonly TaggedLogger Log;

    public UsdRenderer(ArgsHandler argsHandler, CryEngine cryEngine)
    {
        _args = argsHandler;
        _cryData = cryEngine;
        usdOutputFile = _args.FormatOutputFileName(".usda", _cryData.InputFile);
        usdSerializer = new UsdSerializer();
        Log = _cryData.Log;
    }

    public int Render()
    {
        var usdDoc = GenerateUsdObject();

        Log.D();
        Log.D("*** Starting Write USD ***");
        Log.D();

        WriteUsdToFile(usdDoc);

        return 0;
    }

    public void WriteUsdToFile(UsdDoc usdDoc)
    {
        TextWriter writer = new StreamWriter(usdOutputFile.FullName);
        usdSerializer.Serialize(usdDoc, writer);
        writer.Close();
    }

    public UsdDoc GenerateUsdObject()
    {
        Log.D("Number of models: {0}", _cryData.Models.Count);
        for (int i = 0; i < _cryData.Models.Count; i++)
        {
            Log.D("\tNumber of nodes in model: {0}", _cryData.Models[i].NodeMap.Count);
        }

        // Create the usd doc
        var usdDoc = new UsdDoc { Header = new UsdHeader() };
        usdDoc.Prims.Add(new UsdXform("root", "/"));
        var rootPrim = usdDoc.Prims[0];

        // Check if this model has skeletal animation
        bool hasSkeleton = _cryData.SkinningInfo?.HasSkinningInfo ?? false;

        if (hasSkeleton)
        {
            // Create skeleton hierarchy
            Log.D("Model has skeleton with {0} bones", _cryData.SkinningInfo.CompiledBones.Count);
            var skelRoot = CreateSkeleton();
            rootPrim.Children.Add(skelRoot);

            // Add skinned node hierarchy under the skeleton root
            skelRoot.Children.AddRange(CreateNodeHierarchy());
        }
        else
        {
            // Create the node hierarchy as Xforms
            rootPrim.Children = CreateNodeHierarchy();
        }

        rootPrim.Children.Add(CreateMaterials());

        return usdDoc;
    }

    private UsdPrim CreateMaterials()
    {
        var scope = new UsdScope("_materials");
        var matList = new List<UsdMaterial>();

        foreach (var matKey in _cryData.Materials.Keys) // Each mtl file is a Key
        {
            foreach (var submat in _cryData.Materials[matKey].SubMaterials)
            {
                var matName = GetMaterialName(matKey, submat.Name);
                var cleanMatName = CleanPathString(matName);
                var usdMat = new UsdMaterial(cleanMatName);
                usdMat.Attributes.Add(new UsdToken<string>(
                    "outputs:surface.connect",
                    $"</root/_materials/{cleanMatName}/Principled_BSDF.outputs:surface>"));
                usdMat.Children.AddRange(CreateShaders(submat, matKey, cleanMatName));
                matList.Add(usdMat);
            }
        }
        scope.Children.AddRange(matList);
        return scope;
    }

    private IEnumerable<UsdShader> CreateShaders(Material submat, string matKey, string matName)
    {
        List<UsdShader> shaders = new();
        // Add the PrincipleBSDF shader
        var principleBSDF = new UsdShader($"Principled_BSDF");
        principleBSDF.Attributes.Add(new UsdToken<string>("info:id", "UsdPreviewSurface", true));

        // Material color properties
        if (submat.DiffuseValue is not null)
        {
            var diffuse = $"{submat.DiffuseValue.Red}, {submat.DiffuseValue.Green}, {submat.DiffuseValue.Blue}";
            principleBSDF.Attributes.Add(new UsdColor3f("inputs:diffuseColor", diffuse));
        }

        if (submat.SpecularValue is not null)
        {
            var specular = $"{submat.SpecularValue.Red}, {submat.SpecularValue.Green}, {submat.SpecularValue.Blue}";
            principleBSDF.Attributes.Add(new UsdColor3f("inputs:specularColor", specular));
        }

        if (submat.EmissiveValue is not null)
        {
            var emissive = $"{submat.EmissiveValue.Red}, {submat.EmissiveValue.Green}, {submat.EmissiveValue.Blue}";
            principleBSDF.Attributes.Add(new UsdColor3f("inputs:emissiveColor", emissive));
        }

        // Opacity
        if (submat.OpacityValue.HasValue)
        {
            principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:opacity", submat.OpacityValue.Value));
        }

        // Roughness - convert from Shininess
        // Shininess typically ranges from 0-128, where higher = more shiny (less rough)
        if (submat.Shininess > 0)
        {
            float roughness = 1.0f - Math.Clamp((float)(submat.Shininess / 128.0), 0.0f, 1.0f);
            principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:roughness", roughness));
        }

        // Metallic - default to 0 (non-metallic)
        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:metallic", 0.0f));

        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:clearcoat", 0));
        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:clearcoatRoughness", 0.03f));
        principleBSDF.Attributes.Add(new UsdToken<string?>("outputs:surface", null, false));
        shaders.Add(principleBSDF);

        if (submat.Textures == null)
            return shaders;

        // Track which textures we've already created to avoid duplicates
        var createdTextures = new HashSet<string>();

        foreach (var texture in submat.Textures)
        {
            if (texture.Map == Texture.MapTypeEnum.Env)
                continue; // Don't add cubemaps as it causes blender to crash

            // Get the shader name we would create
            if (string.IsNullOrEmpty(texture.File))
                continue;

            var shaderName = CleanPathString(Path.GetFileNameWithoutExtension(texture.File));

            // Skip if we've already created a shader for this texture
            if (createdTextures.Contains(shaderName))
                continue;

            var imageTexture = CreateUsdImageTextureShader(texture, matName);
            if (imageTexture is not null)
            {
                createdTextures.Add(shaderName);
                shaders.Add(imageTexture);
                // connect image texture to color input of PrincipledBSDF
                if (texture.Map == Texture.MapTypeEnum.Diffuse)
                {
                    imageTexture.Attributes.Add(new UsdFloat3f("outputs:rgb"));
                    imageTexture.Attributes.Add(new UsdFloat("outputs:a"));

                    // Connection paths use angle bracket syntax in USD
                    principleBSDF.Attributes.Add(new UsdColor3f(
                        $"inputs:diffuseColor.connect",
                        $"</root/_materials/{matName}/{imageTexture.Name}.outputs:rgb>"));

                    // Connect alpha channel to opacity
                    principleBSDF.Attributes.Add(new UsdFloat(
                        $"inputs:opacity.connect",
                        $"</root/_materials/{matName}/{imageTexture.Name}.outputs:a>"));
                }
                else if (texture.Map == Texture.MapTypeEnum.Normals)
                {
                    imageTexture.Attributes.Add(new UsdFloat3f("outputs:rgb"));
                }
                //else if (texture.Map == Texture.MapTypeEnum.Env)
                //{
                //    imageTexture.Attributes.Add(new UsdToken<string>("inputs:type", "cube"));
                //    imageTexture.Attributes.Add(new UsdColor3f("outputs:rgb", null));
                //}
            }
        }

        return shaders;
    }

    private UsdShader? CreateUsdImageTextureShader(Texture texture, string matName)
    {
        if (string.IsNullOrEmpty(texture.File))
        {
            Log.D("Texture has no file path specified");
            return null;
        }

        // Filter out null/empty data directories
        var dataDirs = new List<string>();
        if (!string.IsNullOrEmpty(_args.DataDir))
            dataDirs.Add(_args.DataDir);

        var textureFile = ResolveTextureFile(texture.File, _args.PackFileSystem, dataDirs);
        if (File.Exists(textureFile) == false)
        {
            Log.D("Texture file not found: {0}", texture.File);
            return null;
        }
        var usdImageTexture = new UsdShader(CleanPathString(Path.GetFileNameWithoutExtension(texture.File)));

        usdImageTexture.Attributes.Add(new UsdToken<string>("info:id", "UsdUVTexture", true));
        usdImageTexture.Attributes.Add(new UsdToken<string>("inputs:wrapS", "repeat"));
        usdImageTexture.Attributes.Add(new UsdToken<string>("inputs:wrapT", "repeat"));

        // Use forward slashes for USD asset paths (cross-platform compatible)
        var normalizedPath = textureFile.Replace('\\', '/');
        usdImageTexture.Attributes.Add(new UsdAsset("file", normalizedPath));

        var isBumpmap = texture.Map == Texture.MapTypeEnum.Normals ? "raw" : "sRGB";
        usdImageTexture.Attributes.Add(new UsdToken<string>("inputs:sourceColorSpace", isBumpmap));

        return usdImageTexture;
    }

    private List<UsdPrim> CreateNodeHierarchy()
    {
        List<ChunkNode> rootNodes = _cryData.Models[0].NodeMap.Values.Where(a => a.ParentNodeID == ~0).ToList();
        List<UsdPrim> nodes = [];

        foreach (ChunkNode root in rootNodes)
        {
            Log.D("Root node: {0}", root.Name);
            nodes.Add(CreateNode(root, $"/root/{root.Name}"));
        }

        return nodes;
    }

    private UsdXform CreateNode(ChunkNode node, string parentPath)
    {
        string cleanNodeName = CleanPathString(node.Name);
        var xform = new UsdXform(cleanNodeName, parentPath);

        xform.Attributes.Add(new UsdMatrix4d("xformOp:transform", node.Transform));
        xform.Attributes.Add(new UsdToken<List<string>>("xformOpOrder", ["xformOp:transform"], true));
        // If it's a geometry node, add a UsdMesh
        //var modelIndex = node._model.IsIvoFile ? 1 : 0;
        //ChunkNode geometryNode = _cryData.Models.Last().NodeMap.Values.Where(a => a.Name == node.Name).FirstOrDefault();

        var meshPrim = CreateMeshPrim(node);
        if (meshPrim is not null)
            xform.Children.Add(meshPrim);

        // Get all the children of the node
        var children = node.Children;
        if (children is not null)
        {
            foreach (var childNode in children)
            {
                xform.Children.Add(CreateNode(childNode, xform.Path));
            }
        }

        return xform;
    }

    private UsdPrim? CreateMeshPrim(ChunkNode nodeChunk)
    {
        if (_args.IsNodeNameExcluded(nodeChunk.Name))
        {
            Log.D($"Excluding node {nodeChunk.Name}");
            return null;
        }

        if (nodeChunk.MeshData is not ChunkMesh meshChunk)
            return null;

        if (meshChunk.GeometryInfo is null)  // $physics node
            return null;

        string nodeName = nodeChunk.Name;

        var subsets = meshChunk.GeometryInfo.GeometrySubsets;
        Datastream<uint>? indices = meshChunk.GeometryInfo.Indices;
        Datastream<UV>? uvs = meshChunk.GeometryInfo.UVs;
        Datastream<Vector3>? verts = meshChunk.GeometryInfo.Vertices;
        Datastream<VertUV>? vertsUvs = meshChunk.GeometryInfo.VertUVs;
        Datastream<Vector3>? normals = meshChunk.GeometryInfo.Normals;
        Datastream<IRGBA>? colors = meshChunk.GeometryInfo.Colors;

        if (verts is null && vertsUvs is null) // There is no vertex data for this node.  Skip.
            return null;

        List<UsdMesh> usdMeshes = [];

        if (meshChunk.MeshSubsetsData == 0
            || meshChunk.NumVertices == 0
            || nodeChunk._model.ChunkMap[meshChunk.MeshSubsetsData].ID == 0)
            return null;

        var numberOfElements = nodeChunk.MeshData.GeometryInfo?.GeometrySubsets?.Sum(x => x.NumVertices) ?? 0;

        UsdMesh meshPrim = new(CleanPathString(nodeChunk.Name));

        if (verts is not null)
        {
            int numVerts = (int)verts.NumElements;
            var hasNormals = normals is not null;
            var hasUVs = uvs is not null;
            var hasColors = colors is not null;

            meshPrim.Attributes.Add(new UsdBool("doubleSided", true, true));
            meshPrim.Attributes.Add(new UsdVector3dList("extent", [meshChunk.MinBound, meshChunk.MaxBound]));
            meshPrim.Attributes.Add(new UsdIntList("faceVertexCounts", [.. Enumerable.Repeat(3, (int)(indices.NumElements / 3))]));
            meshPrim.Attributes.Add(new UsdIntList("faceVertexIndices", [.. indices.Data.Select(x => (int)x)]));
            meshPrim.Attributes.Add(new UsdPointsList("points", [.. verts.Data]));

            if (hasColors)
                meshPrim.Attributes.Add(new UsdColorsList($"{nodeChunk.Name}_color", [.. colors.Data]));
            if (hasUVs)
                meshPrim.Attributes.Add(new UsdTexCoordsList($"{nodeChunk.Name}_UV", [.. uvs.Data]));
            if (hasNormals)
            {
                // For faceVarying normals, expand the normals array to match faceVertexIndices
                // by indexing into the normals using the same indices as vertices
                var faceVaryingNormals = indices.Data.Select(idx => normals.Data[(int)idx]).ToList();
                meshPrim.Attributes.Add(new UsdNormalsList("normals", faceVaryingNormals));
            }

            meshPrim.Attributes.Add(new UsdToken<string>("subdivisionScheme", "none", true));
            Dictionary<string, object> matBindingApi = new() { ["apiSchemas"] = "[\"MaterialBindingAPI\"]" };
            meshPrim.Properties = [new UsdProperty(matBindingApi, true)];

            foreach (var subset in meshChunk.GeometryInfo.GeometrySubsets ?? [])
            {
                var index = subset.MatID;
                var matName = GetMaterialName(
                    Path.GetFileNameWithoutExtension(nodeChunk.MaterialFileName),
                    _cryData.Materials[nodeChunk.MaterialFileName].SubMaterials[index].Name);
                var cleanMatName = CleanPathString(matName);

                var submeshPrim = new UsdGeomSubset(cleanMatName);

                // Convert vertex index range to face index range
                // subset.FirstIndex and subset.NumIndices refer to vertex indices
                // For elementType="face", we need face indices (triangle numbers)
                // Each face has 3 vertices, so face_index = vertex_index / 3
                int firstFace = (int)subset.FirstIndex / 3;
                int numFaces = (int)subset.NumIndices / 3;
                var faceIndices = Enumerable.Range(firstFace, numFaces).Select(i => (uint)i).ToList();

                submeshPrim.Attributes.Add(new UsdUIntList("indices", faceIndices));
                //submeshPrim.Attributes.Add(new UsdToken<string>("familyType", "face", true));
                submeshPrim.Attributes.Add(new UsdToken<string>("elementType", "face", true));
                submeshPrim.Attributes.Add(new UsdToken<string>("familyName", "materialBind", true));
                meshPrim.Children.Add(submeshPrim);

                // Assign material to submesh
                submeshPrim.Properties = [new UsdProperty(matBindingApi, true)];
                submeshPrim.Attributes.Add(
                    new UsdRelativePath(
                        "material:binding",
                        $"/root/_materials/{cleanMatName}"));
            }

            // Add skinning data if present
            if (_cryData.SkinningInfo?.HasSkinningInfo ?? false)
            {
                AddSkinningAttributes(meshPrim, nodeChunk);
            }
        }
        else if (vertsUvs is not null)
            return null;

        return meshPrim;
    }

    private string GetMaterialName(string matKey, string submatName)
    {
        // material name is <mtlChunkName>_mtl_<submatName>
        var matfileName = Path.GetFileNameWithoutExtension(submatName);

        return $"{matKey}_mtl_{matfileName}".Replace(' ', '_');
    }

    #region Skeleton Methods

    /// <summary>Creates a USD skeleton hierarchy for skinned meshes.</summary>
    private UsdSkelRoot CreateSkeleton()
    {
        var skelRoot = new UsdSkelRoot("Armature");
        var skeleton = new UsdSkeleton("Skeleton");

        // Build joint paths (hierarchical bone names)
        var jointPaths = new List<string>();
        var bonePathMap = new Dictionary<CompiledBone, string>();
        BuildJointPaths(_cryData.SkinningInfo.RootBone, "", jointPaths, bonePathMap);

        // Add joint names array
        skeleton.Attributes.Add(new UsdTokenArray("joints", jointPaths, isUniform: true));

        // Add bind transforms (inverse bind pose matrices)
        var bindTransforms = GetBindTransforms(jointPaths, bonePathMap);
        skeleton.Attributes.Add(new UsdMatrix4dArray("bindTransforms", bindTransforms, isUniform: true));

        // Add rest transforms (local transform matrices)
        var restTransforms = GetRestTransforms(jointPaths, bonePathMap);
        skeleton.Attributes.Add(new UsdMatrix4dArray("restTransforms", restTransforms, isUniform: true));

        skelRoot.Children.Add(skeleton);
        return skelRoot;
    }

    /// <summary>Recursively builds joint path strings in USD format (e.g., "Bip01/bip_01_Pelvis/bip_01_Spine").</summary>
    private void BuildJointPaths(CompiledBone bone, string parentPath, List<string> jointPaths, Dictionary<CompiledBone, string> bonePathMap)
    {
        if (bone == null)
            return;

        // Clean bone name for USD compliance
        string cleanName = CleanPathString(bone.BoneName ?? "bone");

        // Build the full path for this bone
        string bonePath = string.IsNullOrEmpty(parentPath)
            ? cleanName
            : $"{parentPath}/{cleanName}";

        jointPaths.Add(bonePath);
        bonePathMap[bone] = bonePath;

        // Recursively process children
        var childBones = _cryData.SkinningInfo.GetChildBones(bone);
        foreach (var childBone in childBones)
        {
            BuildJointPaths(childBone, bonePath, jointPaths, bonePathMap);
        }
    }

    /// <summary>Gets bind transforms (inverse bind matrices) in joint order.</summary>
    private List<Matrix4x4> GetBindTransforms(List<string> jointPaths, Dictionary<CompiledBone, string> bonePathMap)
    {
        var bindTransforms = new List<Matrix4x4>();

        // Build reverse lookup from path to bone
        var pathToBone = bonePathMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        foreach (var jointPath in jointPaths)
        {
            if (pathToBone.TryGetValue(jointPath, out var bone))
            {
                // USD bindTransforms are inverse bind matrices (world-to-bone)
                // BindPoseMatrix has translation in M14/M24/M34 (column 4)
                // But USD needs translation in M41/M42/M43 (row 4)
                // Convert the matrix format
                bindTransforms.Add(MoveTranslationToRow4(bone.BindPoseMatrix));
            }
            else
            {
                // Fallback to identity matrix if bone not found
                bindTransforms.Add(Matrix4x4.Identity);
            }
        }

        return bindTransforms;
    }

    /// <summary>
    /// Converts a Matrix4x4 with translation in column 4 (M14/M24/M34)
    /// to USD format: move translation to row 4 (M41/M42/M43), keep rotation as-is.
    /// </summary>
    private static Matrix4x4 MoveTranslationToRow4(Matrix4x4 source)
    {
        return new Matrix4x4
        {
            M11 = source.M11,
            M12 = source.M12,
            M13 = source.M13,
            M14 = 0,
            M21 = source.M21,
            M22 = source.M22,
            M23 = source.M23,
            M24 = 0,
            M31 = source.M31,
            M32 = source.M32,
            M33 = source.M33,
            M34 = 0,
            M41 = source.M14,  // Move translation from column 4 to row 4
            M42 = source.M24,
            M43 = source.M34,
            M44 = source.M44
        };
    }

    /// <summary>Gets rest transforms (local-space bone transforms) in joint order.</summary>
    private List<Matrix4x4> GetRestTransforms(List<string> jointPaths, Dictionary<CompiledBone, string> bonePathMap)
    {
        var restTransforms = new List<Matrix4x4>();

        // Build reverse lookup from path to bone
        var pathToBone = bonePathMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        foreach (var jointPath in jointPaths)
        {
            if (pathToBone.TryGetValue(jointPath, out var bone))
            {
                // USD restTransforms are LOCAL transforms (bone-to-parent)
                // Calculate from world transforms: LocalTransform = inverse(ParentWorld) * ChildWorld
                Matrix4x4 localTransform;

                if (bone.ParentBone == null)
                {
                    // Root bone - use world transform directly
                    // Just move translation to row 4, don't transpose
                    localTransform = bone.WorldTransformMatrix.ConvertToUsdTransformMatrix();
                }
                else
                {
                    // Child bone - compute local transform
                    // Just move translation to row 4, don't transpose
                    var parentWorld = bone.ParentBone.WorldTransformMatrix.ConvertToUsdTransformMatrix();
                    var childWorld = bone.WorldTransformMatrix.ConvertToUsdTransformMatrix();

                    if (Matrix4x4.Invert(parentWorld, out var parentWorldInv))
                    {
                        localTransform = parentWorldInv * childWorld;
                    }
                    else
                    {
                        // Fallback if parent can't be inverted
                        localTransform = childWorld;
                    }
                }

                restTransforms.Add(localTransform);
            }
            else
            {
                // Fallback to identity matrix if bone not found
                restTransforms.Add(Matrix4x4.Identity);
            }
        }

        return restTransforms;
    }

    /// <summary>Adds skinning attributes to a mesh prim for skeletal animation.</summary>
    private void AddSkinningAttributes(UsdMesh meshPrim, ChunkNode nodeChunk)
    {
        // Add SkelBindingAPI schema
        var skelBindingApi = new Dictionary<string, object> { ["apiSchemas"] = "[\"SkelBindingAPI\"]" };

        // Merge with existing properties or create new
        if (meshPrim.Properties != null && meshPrim.Properties.Count > 0)
        {
            // Update existing properties to include SkelBindingAPI
            meshPrim.Properties[0].Properties["apiSchemas"] = "[\"MaterialBindingAPI\", \"SkelBindingAPI\"]";
        }
        else
            meshPrim.Properties = [new UsdProperty(skelBindingApi, true)];

        // Add geomBindTransform (usually identity matrix)
        meshPrim.Attributes.Add(new UsdMatrix4d("primvars:skel:geomBindTransform", Matrix4x4.Identity));

        // Get skinning data from IntVertices
        var skinningInfo = _cryData.SkinningInfo;
        if (skinningInfo.IntVertices == null || skinningInfo.IntVertices.Count == 0)
            return;

        // Build joint indices and weights arrays
        var jointIndices = new List<int>();
        var jointWeights = new List<float>();
        int maxInfluences = 0;

        foreach (var intVertex in skinningInfo.IntVertices)
        {
            // Count non-zero weights to determine influences per vertex
            int influences = 0;
            for (int i = 0; i < 4; i++)
            {
                if (intVertex.BoneMapping.Weight[i] > 0)
                    influences++;
            }
            maxInfluences = Math.Max(maxInfluences, influences);

            // Add bone indices and weights (up to 4 influences per vertex)
            for (int i = 0; i < 4; i++)
            {
                jointIndices.Add(intVertex.BoneMapping.BoneIndex[i]);
                jointWeights.Add(intVertex.BoneMapping.Weight[i]); // Weights are already 0-1 range in MeshBoneMapping
            }
        }

        // Add skinning arrays with elementSize (influences per vertex)
        int elementSize = 4; // CryEngine uses up to 4 bone influences per vertex
        meshPrim.Attributes.Add(new UsdIntArray("primvars:skel:jointIndices", jointIndices, elementSize, "vertex"));
        meshPrim.Attributes.Add(new UsdFloatArray("primvars:skel:jointWeights", jointWeights, elementSize, "vertex"));

        // Add relationship to skeleton
        meshPrim.Attributes.Add(new UsdRelationship("skel:skeleton", "</root/Armature/Skeleton>"));
    }

    #endregion

    /// <summary>Clean a string to be valid USD prim name.
    /// USD prim names must start with letter/underscore and contain only letters, digits, and underscores.</summary>
    private string CleanPathString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "_";

        // Replace invalid characters with underscore
        var cleaned = new StringBuilder();
        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                cleaned.Append(c);
            else
                cleaned.Append('_');
        }

        // Ensure it starts with letter or underscore
        var result = cleaned.ToString();
        if (char.IsDigit(result[0]))
            result = "_" + result;

        return result;
    }
}
