using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Models.Structs;
using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
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
        List<ChunkNode> rootNodes = _cryData.Nodes.Where(a => a.ParentNodeID == ~0).ToList();
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
        Datastream<UV>? uvs2 = meshChunk.GeometryInfo.UVs2;  // Second UV layer (if available)
        Datastream<Vector3>? verts = meshChunk.GeometryInfo.Vertices;
        Datastream<VertUV>? vertsUvs = meshChunk.GeometryInfo.VertUVs;
        Datastream<Vector3>? normals = meshChunk.GeometryInfo.Normals;
        Datastream<IRGBA>? colors = meshChunk.GeometryInfo.Colors;

        // Validation checks - same as glTF renderer
        if (indices is null)
        {
            Log.D($"Mesh[{nodeChunk.Name}]: IndicesData is empty.");
            return null;
        }
        if (subsets is null)
        {
            Log.D($"Mesh[{nodeChunk.Name}]: GeometrySubsets is empty.");
            return null;
        }
        if (verts is null && vertsUvs is null)
        {
            Log.D($"Mesh[{nodeChunk.Name}]: both VerticesData and VertsUVsData are empty.");
            return null;
        }

        var numberOfElements = nodeChunk.MeshData.GeometryInfo?.GeometrySubsets?.Sum(x => x.NumVertices) ?? 0;

        UsdMesh meshPrim = new(CleanPathString(nodeChunk.Name));

        if (verts is not null)
        {
            int numVerts = (int)verts.NumElements;
            var hasNormals = normals is not null;
            var hasUVs = uvs is not null;
            var hasUVs2 = uvs2 is not null;  // Second UV layer
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
            if (hasUVs2)
                meshPrim.Attributes.Add(new UsdTexCoordsList($"{CleanPathString(nodeChunk.Name)}_UV2", [.. uvs2.Data]));
            if (hasNormals)
            {
                // For faceVarying normals, expand the normals array to match faceVertexIndices
                // by indexing into the normals using the same indices as vertices
                var faceVaryingNormals = indices.Data
                    .Select(idx => (int)idx < normals.Data.Length ? normals.Data[(int)idx] : Vector3.UnitY)
                    .ToList();
                meshPrim.Attributes.Add(new UsdNormalsList("normals", faceVaryingNormals));
            }

            meshPrim.Attributes.Add(new UsdToken<string>("subdivisionScheme", "none", true));
            Dictionary<string, object> matBindingApi = new() { ["apiSchemas"] = "[\"MaterialBindingAPI\"]" };
            meshPrim.Properties = [new UsdProperty(matBindingApi, true)];

            // Collect face indices per material to avoid duplicate GeomSubset prims
            // Multiple mesh subsets may use the same material
            var materialFaceIndices = new Dictionary<string, List<uint>>();

            foreach (var subset in meshChunk.GeometryInfo.GeometrySubsets ?? [])
            {
                var index = subset.MatID;

                // Bounds check for material lookup
                if (!_cryData.Materials.TryGetValue(nodeChunk.MaterialFileName, out var material) ||
                    material?.SubMaterials is null ||
                    index < 0 || index >= material.SubMaterials.Length)
                {
                    Log.D($"Mesh[{nodeChunk.Name}]: Material index {index} out of bounds or material not found.");
                    continue;
                }

                var matName = GetMaterialName(
                    Path.GetFileNameWithoutExtension(nodeChunk.MaterialFileName),
                    material.SubMaterials[index].Name);
                var cleanMatName = CleanPathString(matName);

                // Convert vertex index range to face index range
                int firstFace = (int)subset.FirstIndex / 3;
                int numFaces = (int)subset.NumIndices / 3;
                var faceIndices = Enumerable.Range(firstFace, numFaces).Select(i => (uint)i);

                // Merge face indices for subsets using the same material
                if (!materialFaceIndices.ContainsKey(cleanMatName))
                    materialFaceIndices[cleanMatName] = new List<uint>();
                materialFaceIndices[cleanMatName].AddRange(faceIndices);
            }

            // Create one GeomSubset per unique material
            foreach (var kvp in materialFaceIndices)
            {
                var cleanMatName = kvp.Key;
                var faceIndices = kvp.Value;

                var submeshPrim = new UsdGeomSubset(cleanMatName);
                submeshPrim.Attributes.Add(new UsdUIntList("indices", faceIndices));
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
        {
            // Handle Ivo format (Star Citizen) - combined vertex/UV/color data
            // For Ivo format, we must extract only the vertices used by each subset
            // and remap indices from global to local (same approach as Collada/glTF renderers)
            var hasNormals = normals is not null;

            // Calculate bounding box scaling for Ivo format
            var multiplerVector = Vector3.Abs((meshChunk.MinBound - meshChunk.MaxBound) / 2f);
            if (multiplerVector.X < 1) multiplerVector.X = 1;
            if (multiplerVector.Y < 1) multiplerVector.Y = 1;
            if (multiplerVector.Z < 1) multiplerVector.Z = 1;
            var boundaryBoxCenter = (meshChunk.MinBound + meshChunk.MaxBound) / 2f;

            Vector3 scalingVector = Vector3.One;
            Vector3 scalingBoxCenter = Vector3.Zero;
            bool useScalingBox = false;

            if (meshChunk.ScalingVectors is not null)
            {
                scalingVector = Vector3.Abs((meshChunk.ScalingVectors.Max - meshChunk.ScalingVectors.Min) / 2f);
                if (scalingVector.X < 1) scalingVector.X = 1;
                if (scalingVector.Y < 1) scalingVector.Y = 1;
                if (scalingVector.Z < 1) scalingVector.Z = 1;
                scalingBoxCenter = (meshChunk.ScalingVectors.Max + meshChunk.ScalingVectors.Min) / 2f;
                useScalingBox = _cryData.InputFile.EndsWith("cga") || _cryData.InputFile.EndsWith("cgf");
            }

            // Extract vertices, UVs, colors, and normals PER-SUBSET (not all data)
            // This is critical for Ivo format where geometry is shared across nodes
            var vertices = new List<Vector3>();
            var uvList = new List<UV>();
            var colorList = new List<IRGBA>();
            var normalsList = new List<Vector3>();

            foreach (var subset in subsets ?? [])
            {
                for (int i = subset.FirstVertex; i < subset.FirstVertex + subset.NumVertices; i++)
                {
                    var vertUv = vertsUvs.Data[i];

                    // Apply bounding box scaling to vertices (skip for .skin and .chr files)
                    Vector3 vertex = vertUv.Vertex;
                    if (!_cryData.InputFile.EndsWith("skin") && !_cryData.InputFile.EndsWith("chr"))
                    {
                        if (useScalingBox)
                            vertex = (vertex * scalingVector) + scalingBoxCenter;
                        else
                            vertex = (vertex * multiplerVector) + boundaryBoxCenter;
                    }

                    vertices.Add(vertex);
                    uvList.Add(vertUv.UV);
                    colorList.Add(vertUv.Color);

                    if (hasNormals)
                        normalsList.Add(normals.Data[i]);
                }
            }

            // Remap indices from global to local
            // Each subset's indices point into the global vertex array, but we've extracted
            // only the subset vertices concatenated together, so we need to remap
            var remappedIndices = new List<int>();
            uint currentOffset = 0;

            foreach (var subset in subsets ?? [])
            {
                var firstGlobalIndex = indices.Data[subset.FirstIndex];

                for (int i = 0; i < subset.NumIndices; i++)
                {
                    uint globalIndex = indices.Data[subset.FirstIndex + i];
                    int localIndex = (int)((globalIndex - firstGlobalIndex) + currentOffset);
                    remappedIndices.Add(localIndex);
                }

                currentOffset += (uint)subset.NumVertices;
            }

            int numFaces = remappedIndices.Count / 3;

            meshPrim.Attributes.Add(new UsdBool("doubleSided", true, true));
            meshPrim.Attributes.Add(new UsdVector3dList("extent", [meshChunk.MinBound, meshChunk.MaxBound]));
            meshPrim.Attributes.Add(new UsdIntList("faceVertexCounts", [.. Enumerable.Repeat(3, numFaces)]));
            meshPrim.Attributes.Add(new UsdIntList("faceVertexIndices", remappedIndices));
            meshPrim.Attributes.Add(new UsdPointsList("points", vertices));

            // Add vertex colors from VertUV
            meshPrim.Attributes.Add(new UsdColorsList($"{CleanPathString(nodeChunk.Name)}_color", colorList));

            // Add UVs from VertUV
            meshPrim.Attributes.Add(new UsdTexCoordsList($"{CleanPathString(nodeChunk.Name)}_UV", uvList));

            if (hasNormals)
            {
                // For faceVarying normals, expand the normals array to match faceVertexIndices
                // by indexing into the extracted normals using the remapped indices
                var faceVaryingNormals = remappedIndices
                    .Select(idx => idx >= 0 && idx < normalsList.Count ? normalsList[idx] : Vector3.UnitY)
                    .ToList();
                meshPrim.Attributes.Add(new UsdNormalsList("normals", faceVaryingNormals));
            }

            meshPrim.Attributes.Add(new UsdToken<string>("subdivisionScheme", "none", true));
            Dictionary<string, object> matBindingApi = new() { ["apiSchemas"] = "[\"MaterialBindingAPI\"]" };
            meshPrim.Properties = [new UsdProperty(matBindingApi, true)];

            // Collect face indices per material to avoid duplicate GeomSubset prims
            // Multiple mesh subsets may use the same material
            var materialFaceIndices = new Dictionary<string, List<uint>>();

            // For Ivo format with remapped indices, GeomSubsets use sequential face ranges
            // since all indices are now contiguous after remapping
            int currentFaceOffset = 0;
            foreach (var subset in meshChunk.GeometryInfo.GeometrySubsets ?? [])
            {
                var index = subset.MatID;
                int subsetNumFaces = (int)subset.NumIndices / 3;

                // Bounds check for material lookup
                if (!_cryData.Materials.TryGetValue(nodeChunk.MaterialFileName, out var material) ||
                    material?.SubMaterials is null ||
                    index < 0 || index >= material.SubMaterials.Length)
                {
                    Log.D($"Mesh[{nodeChunk.Name}]: Material index {index} out of bounds or material not found.");
                    // Still need to update face offset even if we skip this subset
                    currentFaceOffset += subsetNumFaces;
                    continue;
                }

                var matName = GetMaterialName(
                    Path.GetFileNameWithoutExtension(nodeChunk.MaterialFileName),
                    material.SubMaterials[index].Name);
                var cleanMatName = CleanPathString(matName);

                // Face indices are now sequential after remapping
                var faceIndices = Enumerable.Range(currentFaceOffset, subsetNumFaces).Select(i => (uint)i);
                currentFaceOffset += subsetNumFaces;

                // Merge face indices for subsets using the same material
                if (!materialFaceIndices.ContainsKey(cleanMatName))
                    materialFaceIndices[cleanMatName] = new List<uint>();
                materialFaceIndices[cleanMatName].AddRange(faceIndices);
            }

            // Create one GeomSubset per unique material
            foreach (var kvp in materialFaceIndices)
            {
                var cleanMatName = kvp.Key;
                var faceIndices = kvp.Value;

                var submeshPrim = new UsdGeomSubset(cleanMatName);
                submeshPrim.Attributes.Add(new UsdUIntList("indices", faceIndices));
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

        return meshPrim;
    }
}
