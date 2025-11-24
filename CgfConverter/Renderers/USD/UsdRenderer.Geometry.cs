using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Models.Structs;
using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CgfConverter.Renderers.USD;

/// <summary>
/// UsdRenderer partial class - Geometry and node hierarchy creation
/// </summary>
public partial class UsdRenderer
{
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
                meshPrim.Attributes.Add(new UsdColorsList($"{CleanPathString(nodeChunk.Name)}_color", [.. colors.Data]));
            if (hasUVs)
                meshPrim.Attributes.Add(new UsdTexCoordsList($"{CleanPathString(nodeChunk.Name)}_UV", [.. uvs.Data]));
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
}
