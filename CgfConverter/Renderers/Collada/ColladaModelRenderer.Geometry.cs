using CgfConverter.Collada;
using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Models.Structs;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Collada.Collada.Types;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using static CgfConverter.Utilities.HelperMethods;

namespace CgfConverter.Renderers.Collada;

/// <summary>
/// ColladaModelRenderer partial class - Geometry creation and mesh processing
/// </summary>
public partial class ColladaModelRenderer
{
    public void WriteLibrary_Geometries()
    {
        WriteGeometries();
    }

    public void WriteGeometries()
    {
        ColladaLibraryGeometries libraryGeometries = new();

        // Make a list for all the geometries objects we will need. Will convert to array at end.  Define the array here as well
        // We have to define a Geometry for EACH meshsubset in the meshsubsets, since the mesh can contain multiple materials
        List<ColladaGeometry> geometryList = [];

        // For each of the nodes, we need to write the geometry.
        foreach (ChunkNode nodeChunk in _cryData.Nodes)
        {
            if (_args.IsNodeNameExcluded(nodeChunk.Name))
            {
                Log.D($"Excluding node {nodeChunk.Name}");
                continue;
            }

            if (nodeChunk.MeshData is not ChunkMesh meshChunk)
                continue;

            if (meshChunk.GeometryInfo is null)  // $physics node
                continue;

            // Create a geometry object.  Use the chunk ID for the geometry ID
            // Create all the materials used by this chunk.
            // Make the mesh object.  This will have 3 or 4 sources, 1 vertices, and 1 or more Triangles (with material ID)
            // If the Object ID of Node chunk points to a Helper or a Controller, place an empty.
            var subsets = meshChunk.GeometryInfo.GeometrySubsets;
            Datastream<uint>? indices = meshChunk.GeometryInfo.Indices;
            Datastream<UV>? uvs = meshChunk.GeometryInfo.UVs;
            Datastream<UV>? uvs2 = meshChunk.GeometryInfo.UVs2;
            Datastream<Vector3>? verts = meshChunk.GeometryInfo.Vertices;
            Datastream<VertUV>? vertsUvs = meshChunk.GeometryInfo.VertUVs;
            Datastream<Vector3>? normals = meshChunk.GeometryInfo.Normals;
            Datastream<IRGBA>? colors = meshChunk.GeometryInfo.Colors;

            if (verts is null && vertsUvs is null) // There is no vertex data for this node.  Skip.
                continue;

            // geometry is a Geometry object for each meshsubset.
            ColladaGeometry geometry = new()
            {
                Name = nodeChunk.Name,
                ID = nodeChunk.Name + "-mesh"
            };
            ColladaMesh colladaMesh = new();
            geometry.Mesh = colladaMesh;

            bool hasUV2 = uvs2 is not null && verts is not null;  // UV2 only supported in traditional format
            ColladaSource[] source = new ColladaSource[hasUV2 ? 5 : 4];
            ColladaSource posSource = new();
            ColladaSource normSource = new();
            ColladaSource uvSource = new();
            ColladaSource colorSource = new();
            ColladaSource uv2Source = new();
            source[0] = posSource;
            source[1] = normSource;
            source[2] = uvSource;
            source[3] = colorSource;
            if (hasUV2)
                source[4] = uv2Source;
            posSource.ID = nodeChunk.Name + "-mesh-pos";
            posSource.Name = nodeChunk.Name + "-pos";
            normSource.ID = nodeChunk.Name + "-mesh-norm";
            normSource.Name = nodeChunk.Name + "-norm";
            uvSource.ID = nodeChunk.Name + "-mesh-UV";
            uvSource.Name = "UV";
            colorSource.ID = nodeChunk.Name + "-mesh-color";
            colorSource.Name = "Col";
            uv2Source.ID = nodeChunk.Name + "-mesh-UV2";
            uv2Source.Name = "UV2";

            ColladaVertices vertices = new() { ID = nodeChunk.Name + "-vertices" };
            geometry.Mesh.Vertices = vertices;
            ColladaInputUnshared[] inputshared = new ColladaInputUnshared[4];
            vertices.Input = inputshared;

            ColladaInputUnshared posInput = new() { Semantic = ColladaInputSemantic.POSITION };
            ColladaInputUnshared normInput = new() { Semantic = ColladaInputSemantic.NORMAL };
            ColladaInputUnshared uvInput = new() { Semantic = ColladaInputSemantic.TEXCOORD };
            ColladaInputUnshared colorInput = new() { Semantic = ColladaInputSemantic.COLOR };

            posInput.source = "#" + posSource.ID;
            normInput.source = "#" + normSource.ID;
            uvInput.source = "#" + uvSource.ID;
            colorInput.source = "#" + colorSource.ID;
            inputshared[0] = posInput;

            ColladaFloatArray floatArrayVerts = new();
            ColladaFloatArray floatArrayNormals = new();
            ColladaFloatArray floatArrayUVs = new();
            ColladaFloatArray floatArrayColors = new();
            ColladaFloatArray floatArrayUVs2 = new();

            StringBuilder vertString = new();
            StringBuilder normString = new();
            StringBuilder uvString = new();
            StringBuilder colorString = new();
            StringBuilder uv2String = new();

            var numberOfElements = nodeChunk.MeshData.GeometryInfo.GeometrySubsets.Sum(x => x.NumVertices);

            if (verts is not null)  // Will be null if it's using VertsUVs.
            {
                int numVerts = (int)verts.NumElements;

                floatArrayVerts.ID = posSource.ID + "-array";
                floatArrayVerts.Digits = 6;
                floatArrayVerts.Magnitude = 38;
                floatArrayVerts.Count = numVerts * 3;
                floatArrayUVs.ID = uvSource.ID + "-array";
                floatArrayUVs.Digits = 6;
                floatArrayUVs.Magnitude = 38;
                floatArrayUVs.Count = numVerts * 2;
                floatArrayNormals.ID = normSource.ID + "-array";
                floatArrayNormals.Digits = 6;
                floatArrayNormals.Magnitude = 38;
                floatArrayNormals.Count = numVerts * 3;
                floatArrayColors.ID = colorSource.ID + "-array";
                floatArrayColors.Digits = 6;
                floatArrayColors.Magnitude = 38;
                floatArrayColors.Count = numVerts * 4;
                floatArrayUVs2.ID = uv2Source.ID + "-array";
                floatArrayUVs2.Digits = 6;
                floatArrayUVs2.Magnitude = 38;
                floatArrayUVs2.Count = numVerts * 2;

                var hasNormals = normals is not null;
                var hasUVs = uvs is not null;
                var hasColors = colors is not null;
                for (uint j = 0; j < numVerts; j++)
                {
                    var normal = hasNormals ? normals.Data[j] : DefaultNormal;
                    var uv = hasUVs ? uvs.Data[j] : DefaultUV;
                    var color = hasColors ? colors.Data[j] : DefaultColor;
                    var uv2 = hasUV2 ? uvs2!.Data[j] : DefaultUV;
                    vertString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} ", verts.Data[j].X, verts.Data[j].Y, verts.Data[j].Z);
                    normString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} ", Safe(normal.X), Safe(normal.Y), Safe(normal.Z));
                    colorString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} {3:F6} ", color.R, color.G, color.B, color.A);
                    uvString.AppendFormat(culture, "{0:F6} {1:F6} ", Safe(uv.U), 1 - Safe(uv.V));
                    if (hasUV2)
                        uv2String.AppendFormat(culture, "{0:F6} {1:F6} ", Safe(uv2.U), 1 - Safe(uv2.V));
                }
            }
            else    // VertsUV structure.  Pull out verts, colors and UVs from vertsUvs.
            {
                floatArrayVerts.ID = posSource.ID + "-array";
                floatArrayVerts.Digits = 6;
                floatArrayVerts.Magnitude = 38;
                floatArrayVerts.Count = numberOfElements * 3;
                floatArrayUVs.ID = uvSource.ID + "-array";
                floatArrayUVs.Digits = 6;
                floatArrayUVs.Magnitude = 38;
                floatArrayUVs.Count = numberOfElements * 2;
                floatArrayNormals.ID = normSource.ID + "-array";
                floatArrayNormals.Digits = 6;
                floatArrayNormals.Magnitude = 38;
                floatArrayNormals.Count = numberOfElements * 3;
                floatArrayColors.ID = colorSource.ID + "-array";
                floatArrayColors.Digits = 6;
                floatArrayColors.Magnitude = 38;
                floatArrayColors.Count = numberOfElements * 4;

                var multiplerVector = _cryData.IsIvoFile
                    ? Vector3.Abs((meshChunk.MinBound - meshChunk.MaxBound) / 2f)
                    : Vector3.One;

                if (multiplerVector.X < 1) multiplerVector.X = 1;
                if (multiplerVector.Y < 1) multiplerVector.Y = 1;
                if (multiplerVector.Z < 1) multiplerVector.Z = 1;
                Vector3 scalingVector = Vector3.One;

                if (meshChunk.ScalingVectors is not null)
                {
                    scalingVector = Vector3.Abs((meshChunk.ScalingVectors.Max - meshChunk.ScalingVectors.Min) / 2f);
                    if (scalingVector.X < 1) scalingVector.X = 1;
                    if (scalingVector.Y < 1) scalingVector.Y = 1;
                    if (scalingVector.Z < 1) scalingVector.Z = 1;
                }

                var boundaryBoxCenter = _cryData.IsIvoFile
                    ? (meshChunk.MinBound + meshChunk.MaxBound) / 2f
                    : Vector3.Zero;

                var scalingBoxCenter = meshChunk.ScalingVectors is not null ? (meshChunk.ScalingVectors.Max + meshChunk.ScalingVectors.Min) / 2f : Vector3.Zero;
                var hasNormals = normals is not null;
                var useScalingBox = _cryData.InputFile
                    .EndsWith("cga") || _cryData.InputFile.EndsWith("cgf")
                    && meshChunk.ScalingVectors is not null;

                // Create Vertices, UV, normals and colors string
                foreach (var subset in meshChunk.GeometryInfo.GeometrySubsets ?? [])
                {
                    for (int i = subset.FirstVertex; i < subset.NumVertices + subset.FirstVertex; i++)
                    {
                        Vector3 vert = vertsUvs.Data[i].Vertex;

                        if (!_cryData.InputFile.EndsWith("skin") && !_cryData.InputFile.EndsWith("chr"))
                        {
                            if (meshChunk.ScalingVectors is null)
                                vert = (vert * multiplerVector) + boundaryBoxCenter;
                            else
                                vert = (vert * scalingVector) + scalingBoxCenter;
                        }

                        vertString.AppendFormat("{0:F6} {1:F6} {2:F6} ", Safe(vert.X), Safe(vert.Y), Safe(vert.Z));
                        colorString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} {3:F6} ", vertsUvs.Data[i].Color.R, vertsUvs.Data[i].Color.G, vertsUvs.Data[i].Color.B, vertsUvs.Data[i].Color.A);
                        uvString.AppendFormat("{0:F6} {1:F6} ", Safe(vertsUvs.Data[i].UV.U), Safe(1 - vertsUvs.Data[i].UV.V));

                        var normal = hasNormals ? normals.Data[i] : DefaultNormal;
                        normString.AppendFormat("{0:F6} {1:F6} {2:F6} ", Safe(normal.X), Safe(normal.Y), Safe(normal.Z));
                    }
                }
            }

            CleanNumbers(vertString);
            CleanNumbers(normString);
            CleanNumbers(uvString);
            CleanNumbers(colorString);

            #region Create the triangles node.
            var numberOfMeshSubsets = subsets.Count;
            var triangles = new ColladaTriangles[numberOfMeshSubsets];
            geometry.Mesh.Triangles = triangles;

            for (int j = 0; j < numberOfMeshSubsets; j++) // Need to make a new Triangles entry for each submesh.
            {
                var submatName = GetSafeSubmaterialName(nodeChunk, subsets[j].MatID);
                triangles[j] = new ColladaTriangles
                {
                    Count = subsets[j].NumIndices / 3,
                    Material = GetMaterialName(nodeChunk.MaterialFileName, submatName) + "-material"
                };

                // Create the inputs.  vertex, normal, texcoord, [color], [texcoord2]
                bool triHasColors = colors is not null || vertsUvs is not null;
                int inputCount = 3 + (triHasColors ? 1 : 0) + (hasUV2 ? 1 : 0);

                triangles[j].Input = new ColladaInputShared[inputCount];

                triangles[j].Input[0] = new ColladaInputShared
                {
                    Semantic = ColladaInputSemantic.VERTEX,
                    Offset = 0,
                    source = "#" + vertices.ID
                };
                triangles[j].Input[1] = new ColladaInputShared
                {
                    Semantic = ColladaInputSemantic.NORMAL,
                    Offset = 1,
                    source = "#" + normSource.ID
                };
                triangles[j].Input[2] = new ColladaInputShared
                {
                    Semantic = ColladaInputSemantic.TEXCOORD,
                    Offset = 2,
                    Set = 0,
                    source = "#" + uvSource.ID
                };

                int nextInputID = 3;
                if (triHasColors)
                {
                    triangles[j].Input[nextInputID] = new ColladaInputShared
                    {
                        Semantic = ColladaInputSemantic.COLOR,
                        Offset = nextInputID,
                        source = "#" + colorSource.ID
                    };
                    nextInputID++;
                }
                if (hasUV2)
                {
                    triangles[j].Input[nextInputID] = new ColladaInputShared
                    {
                        Semantic = ColladaInputSemantic.TEXCOORD,
                        Offset = nextInputID,
                        Set = 1,
                        source = "#" + uv2Source.ID
                    };
                    nextInputID++;
                }

                // Create the P node for the Triangles.
                // Each triangle vertex emits its index once per active input (all inputs share the same index).
                StringBuilder p = new();
                string formatString =
                    string.Join(" ", Enumerable.Repeat("{0}", inputCount)) + " " +
                    string.Join(" ", Enumerable.Repeat("{1}", inputCount)) + " " +
                    string.Join(" ", Enumerable.Repeat("{2}", inputCount)) + " ";

                var offsetStart = 0;
                for (int q = 0; q < meshChunk.GeometryInfo.GeometrySubsets.IndexOf(subsets[j]); q++)
                {
                    offsetStart += meshChunk.GeometryInfo.GeometrySubsets[q].NumVertices;
                }

                for (var k = subsets[j].FirstIndex; k < (subsets[j].FirstIndex + subsets[j].NumIndices); k += 3)
                {
                    var firstGlobalIndex = indices.Data[subsets[j].FirstIndex];
                    uint localIndex0 = (uint)((indices.Data[k] - firstGlobalIndex) + offsetStart);
                    uint localIndex1 = (uint)((indices.Data[k + 1] - firstGlobalIndex) + offsetStart);
                    uint localIndex2 = (uint)((indices.Data[k + 2] - firstGlobalIndex) + offsetStart);

                    p.AppendFormat(formatString, localIndex0, localIndex1, localIndex2);
                }
                triangles[j].P = new ColladaIntArrayString
                {
                    Value_As_String = p.ToString().TrimEnd()
                };
            }

            #endregion

            #region Create the source float_array nodes.  Vertex, normal, UV.  May need color as well.

            floatArrayVerts.Value_As_String = vertString.ToString().TrimEnd();
            floatArrayNormals.Value_As_String = normString.ToString().TrimEnd();
            floatArrayUVs.Value_As_String = uvString.ToString().TrimEnd();
            floatArrayColors.Value_As_String = colorString.ToString();
            floatArrayUVs2.Value_As_String = uv2String.ToString().TrimEnd();

            source[0].Float_Array = floatArrayVerts;
            source[1].Float_Array = floatArrayNormals;
            source[2].Float_Array = floatArrayUVs;
            source[3].Float_Array = floatArrayColors;
            if (hasUV2)
                source[4].Float_Array = floatArrayUVs2;
            geometry.Mesh.Source = source;

            // create the technique_common for each of these
            posSource.Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor()
            };
            posSource.Technique_Common.Accessor.Source = "#" + floatArrayVerts.ID;
            posSource.Technique_Common.Accessor.Stride = 3;
            posSource.Technique_Common.Accessor.Count = (uint)numberOfElements;
            ColladaParam[] paramPos = new ColladaParam[3];
            paramPos[0] = new ColladaParam();
            paramPos[1] = new ColladaParam();
            paramPos[2] = new ColladaParam();
            paramPos[0].Name = "X";
            paramPos[0].Type = "float";
            paramPos[1].Name = "Y";
            paramPos[1].Type = "float";
            paramPos[2].Name = "Z";
            paramPos[2].Type = "float";
            posSource.Technique_Common.Accessor.Param = paramPos;

            normSource.Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = "#" + floatArrayNormals.ID,
                    Stride = 3,
                    Count = (uint)numberOfElements
                }
            };
            ColladaParam[] paramNorm = new ColladaParam[3];
            paramNorm[0] = new ColladaParam();
            paramNorm[1] = new ColladaParam();
            paramNorm[2] = new ColladaParam();
            paramNorm[0].Name = "X";
            paramNorm[0].Type = "float";
            paramNorm[1].Name = "Y";
            paramNorm[1].Type = "float";
            paramNorm[2].Name = "Z";
            paramNorm[2].Type = "float";
            normSource.Technique_Common.Accessor.Param = paramNorm;

            uvSource.Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = "#" + floatArrayUVs.ID,
                    Stride = 2
                }
            };

            uvSource.Technique_Common.Accessor.Count = (uint)numberOfElements;

            ColladaParam[] paramUV = new ColladaParam[2];
            paramUV[0] = new ColladaParam();
            paramUV[1] = new ColladaParam();
            paramUV[0].Name = "S";
            paramUV[0].Type = "float";
            paramUV[1].Name = "T";
            paramUV[1].Type = "float";
            uvSource.Technique_Common.Accessor.Param = paramUV;

            if (colors is not null || vertsUvs is not null)
            {
                colorSource.Technique_Common = new ColladaTechniqueCommonSource
                {
                    Accessor = new ColladaAccessor()
                };
                colorSource.Technique_Common.Accessor.Source = "#" + floatArrayColors.ID;
                colorSource.Technique_Common.Accessor.Stride = 4;
                colorSource.Technique_Common.Accessor.Count = (uint)numberOfElements;
                ColladaParam[] paramColor = new ColladaParam[4];
                paramColor[0] = new ColladaParam();
                paramColor[1] = new ColladaParam();
                paramColor[2] = new ColladaParam();
                paramColor[3] = new ColladaParam();
                paramColor[0].Name = "R";
                paramColor[0].Type = "float";
                paramColor[1].Name = "G";
                paramColor[1].Type = "float";
                paramColor[2].Name = "B";
                paramColor[2].Type = "float";
                paramColor[3].Name = "A";
                paramColor[3].Type = "float";
                colorSource.Technique_Common.Accessor.Param = paramColor;
            }

            if (hasUV2)
            {
                uv2Source.Technique_Common = new ColladaTechniqueCommonSource
                {
                    Accessor = new ColladaAccessor
                    {
                        Source = "#" + floatArrayUVs2.ID,
                        Stride = 2,
                        Count = (uint)numberOfElements
                    }
                };
                uv2Source.Technique_Common.Accessor.Param =
                [
                    new ColladaParam { Name = "S", Type = "float" },
                    new ColladaParam { Name = "T", Type = "float" }
                ];
            }

            geometryList.Add(geometry);

            #endregion

            // There is no geometry for a helper or controller node.  Can skip the rest.
            // Sanity checks
            var vertcheck = vertString.ToString().TrimEnd().Split(' ');
            var normcheck = normString.ToString().TrimEnd().Split(' ');
            var colorcheck = colorString.ToString().TrimEnd().Split(' ');
            var uvcheck = uvString.ToString().TrimEnd().Split(' ');

        }
        libraryGeometries.Geometry = geometryList.ToArray();
        DaeObject.Library_Geometries = libraryGeometries;
    }
}
