using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Models.Materials;
using CgfConverter.Models.Structs;
using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
using CgfConverter.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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

        // Create the node hierarchy as Xforms
        rootPrim.Children = CreateNodeHierarchy();

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
                var usdMat = new UsdMaterial(CleanPathString(matName));
                usdMat.Attributes.Add(new UsdToken<string>(
                    "outputs:surface.connect",
                    $"</root/_materials/{matName}/Principled_BSDF.outputs:surface>"));
                usdMat.Children.AddRange(CreateShaders(submat, matKey, matName));
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
        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:clearcoat", 0));
        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:clearcoatRoughness", 0.03f));
        principleBSDF.Attributes.Add(new UsdToken<string?>("outputs:surface", null, false));
        shaders.Add(principleBSDF);

        foreach (var texture in submat.Textures)
        {
            if (texture.Map == Texture.MapTypeEnum.Env)
                continue; // Don't add cubemaps as it causes blender to crash
            var textureName = Path.ChangeExtension(texture.File, ".dds");
            var imageTexture = CreateUsdImageTextureShader(texture, matName);
            if (imageTexture is not null)
            {
                shaders.Add(imageTexture);
                // connect image texture to color input of PrincipledBSDF
                if (texture.Map == Texture.MapTypeEnum.Diffuse)
                {
                    imageTexture.Attributes.Add(new UsdFloat3f("outputs:rgb"));
                    principleBSDF.Attributes.Add(new UsdColor3f(
                        $"inputs:diffuseColor.connect",
                        CleanPathString($"/root/_materials/{matName}/{imageTexture.Name}.outputs:rgb")));
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
        var textureFile = ResolveTextureFile(texture.File, _args.PackFileSystem, [_args.DataDir]);
        if (File.Exists(textureFile) == false)
        {
            Log.D("Texture file not found: {0}", texture.File);
            return null;
        }
        var usdImageTexture = new UsdShader(CleanPathString(Path.GetFileNameWithoutExtension(texture.File)));

        usdImageTexture.Attributes.Add(new UsdToken<string>("info:id", "UsdUVTexture", true));
        usdImageTexture.Attributes.Add(new UsdToken<string>("inputs:wrapS", "repeat"));
        usdImageTexture.Attributes.Add(new UsdToken<string>("inputs:wrapT", "repeat"));
        var texturePath = ResolveTextureFile(texture.File, _args.PackFileSystem, [_args.DataDir]);
        usdImageTexture.Attributes.Add(new UsdAsset(
            Path.GetFileNameWithoutExtension(texture.File),
            CleanPathString(texturePath)));
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
        var modelIndex = node._model.IsIvoFile ? 1 : 0;
        ChunkNode geometryNode = _cryData.Models.Last().NodeMap.Values.Where(a => a.Name == node.Name).FirstOrDefault();

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
        // Find the object node that corresponds with this node chunk.  If it's
        // a mesh chunk, create a collection of UsdMesh prim for each submesh.
        // geometryNodeChunk may be the same as nodeChunk for single file models.  Otherwise
        // it's the matching node to nodeChunk in the second model.

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
            meshPrim.Attributes.Add(new UsdNormalsList("normals", [.. normals.Data]));
            meshPrim.Attributes.Add(new UsdPointsList("points", [.. verts.Data]));
            meshPrim.Attributes.Add(new UsdColorsList($"{nodeChunk.Name}_color", [.. colors.Data]));
            meshPrim.Attributes.Add(new UsdTexCoordsList($"{nodeChunk.Name}_UV", [.. uvs.Data]));
            meshPrim.Attributes.Add(new UsdToken<string>("subdivisionScheme", "none", true));
            Dictionary<string, object> matBindingApi = new() { ["apiSchemas"] = "[\"MaterialBindingAPI\"]" };
            meshPrim.Properties = [new UsdProperty(matBindingApi, true)];

            foreach (var subset in meshChunk.GeometryInfo.GeometrySubsets ?? [])
            {
                var index = subset.MatID;
                var matName = GetMaterialName(
                    Path.GetFileNameWithoutExtension(nodeChunk.MaterialFileName),
                    _cryData.Materials[nodeChunk.MaterialFileName].SubMaterials[index].Name);
                // Submesh name should be material name
                //var submeshName = 
                var submeshPrim = new UsdGeomSubset(CleanPathString(matName));
                submeshPrim.Attributes.Add(new UsdUIntList("indices", [.. indices.Data.Skip(subset.FirstIndex).Take(subset.NumIndices)]));
                //submeshPrim.Attributes.Add(new UsdToken<string>("familyType", "face", true));
                submeshPrim.Attributes.Add(new UsdToken<string>("elementType", "face", true));
                submeshPrim.Attributes.Add(new UsdToken<string>("familyName", "materialBind", true));
                meshPrim.Children.Add(submeshPrim);

                // Assign material to submesh
                
                submeshPrim.Properties = [new UsdProperty(matBindingApi, true)];
                submeshPrim.Attributes.Add(
                    new UsdRelativePath(
                        "material:binding",
                        $"/root/_materials/{matName}"));
            }
        }
        else if (vertsUvs is not null)
        {
            return null;
        }




        //var geometryNodeChunk = _cryData.Models.Last().NodeMap.Values.Where(x => x.Name == nodeName).FirstOrDefault();
        //if (geometryNodeChunk is null)
        //    return null;

        //var meshNodeId = geometryNodeChunk?.ObjectNodeID;

        //var objectNodeChunkType = _cryData.Models.Last().ChunkMap[nodeChunk.ObjectNodeID].GetType();
        //if (!objectNodeChunkType.Name.Contains("ChunkMesh"))
        //    return null;

        //List<UsdMesh> usdMeshes = new();
        //var meshChunk = (ChunkMesh)_cryData.Models.Last().ChunkMap[geometryNodeChunk.ObjectNodeID];
        //var meshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[meshChunk.MeshSubsetsData];
        //var mtlNameChunk = (ChunkMtlName)_cryData.Models.Last().ChunkMap[nodeChunk.MaterialID];
        //var mtlFileName = mtlNameChunk.Name;
        //var key = Path.GetFileNameWithoutExtension(mtlFileName);
        //var numberOfSubmeshes = meshSubsets.NumMeshSubset;

        // Get materials for this mesh chunk
        //Material[] submats;
        //if (_cryData.Materials.ContainsKey(key))
        //    submats = _cryData.Materials[key].SubMaterials;
        //else
        //{
        //    submats = _cryData.Materials.FirstOrDefault().Value.SubMaterials;
        //    mtlFileName = Path.GetFileNameWithoutExtension(_cryData.MaterialFiles.FirstOrDefault());
        //}
        //var matName = GetMaterialName(mtlFileName, submats[meshSubsets.MeshSubsets[j].MatID].Name);

        //if (meshChunk.MeshSubsetsData == 0
        //    || meshChunk.NumVertices == 0
        //    || nodeChunk._model.ChunkMap[meshChunk.MeshSubsetsData].ID == 0)
        //    return null;

        //// Get datastream chunks for vertices, normals, uvs, indices, colors and tangents
        //var vertexChunk = (ChunkDataStream)_cryData.Models.Last().ChunkMap[meshChunk.VerticesData];
        //var normalChunk = (ChunkDataStream)_cryData.Models.Last().ChunkMap[meshChunk.NormalsData];
        //var uvChunk = (ChunkDataStream)_cryData.Models.Last().ChunkMap[meshChunk.UVsData];
        //var indexChunk = (ChunkDataStream)_cryData.Models.Last().ChunkMap[meshChunk.IndicesData];
        //var colorChunk = (ChunkDataStream)_cryData.Models.Last().ChunkMap[meshChunk.ColorsData];
        //var tangentChunk = (ChunkDataStream)_cryData.Models.Last().ChunkMap[meshChunk.TangentsData];

        ////UsdMesh meshPrim = new(CleanPathString(nodeChunk.Name));
        //meshPrim.Attributes.Add(new UsdBool("doubleSided", true, true));
        //meshPrim.Attributes.Add(new UsdVector3dList("extent", [meshChunk.MinBound, meshChunk.MaxBound]));
        //meshPrim.Attributes.Add(new UsdIntList("faceVertexCounts", [.. Enumerable.Repeat(3, (int)indexChunk.NumElements / 3)]));
        //meshPrim.Attributes.Add(new UsdIntList("faceVertexIndices", indexChunk.Indices.Select(x => (int)x).ToList()));
        //meshPrim.Attributes.Add(new UsdNormalsList("normals", [.. normalChunk.Normals]));
        //meshPrim.Attributes.Add(new UsdPointsList("points", [.. vertexChunk.Vertices]));
        //meshPrim.Attributes.Add(new UsdColorsList($"{nodeChunk.Name}_color", [.. colorChunk.Colors]));
        //meshPrim.Attributes.Add(new UsdTexCoordsList($"{nodeChunk.Name}_UV", [.. uvChunk.UVs]));
        //meshPrim.Attributes.Add(new UsdToken<string>("subdivisionScheme", "none", true));
        //Dictionary<string, object> matBindingApi = new() { ["apiSchemas"] = "[\"MaterialBindingAPI\"]" };
        //meshPrim.Properties = [new UsdProperty(matBindingApi, true)];

        // Add GeomSubset for each submesh
        //for (int j = 0; j < numberOfSubmeshes; j++)
        //{
        //    var submesh = meshSubsets.MeshSubsets[j];
        //    var submeshName = (Path.GetFileNameWithoutExtension(submats[submesh.MatID].Name));
        //    var submeshPrim = new UsdGeomSubset(CleanPathString(submeshName));
        //    submeshPrim.Attributes.Add(new UsdUIntList("indices", indexChunk.Indices.Skip(submesh.FirstIndex).Take(submesh.NumIndices).ToList()));
        //    //submeshPrim.Attributes.Add(new UsdToken<string>("familyType", "face", true));
        //    submeshPrim.Attributes.Add(new UsdToken<string>("elementType", "vertex", true));
        //    submeshPrim.Attributes.Add(new UsdToken<string>("familyName", "materialBind", true));
        //    meshPrim.Children.Add(submeshPrim);

        //    // Assign material to submesh
        //    var submatName = _cryData.Materials[key].SubMaterials[submesh.MatID].Name;
        //    submeshPrim.Properties = [new UsdProperty(matBindingApi, true)];
        //    submeshPrim.Attributes.Add(
        //        new UsdRelativePath(
        //            "material:binding",
        //            $"/root/_materials/{GetMaterialName(key, submatName)}"));
        //}

        return meshPrim;
    }

    private string GetMaterialName(string matKey, string submatName)
    {
        // material name is <mtlChunkName>_mtl_<submatName>
        var matfileName = Path.GetFileNameWithoutExtension(submatName);

        return $"{matKey}_mtl_{matfileName}".Replace(' ', '_');
    }

    /// <summary>If a prim name or value has an @, or starts with a number, it's invalid.
    /// Replace @ with _, and if it starts with a digit, add an _</summary>
    private string CleanPathString(string value)
    {
        value = value.Replace('@', '_');
        if (char.IsDigit(value[0]))
            value = "_" + value;
        return value;
    }
}
