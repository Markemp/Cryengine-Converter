using CgfConverter.CryEngineCore;
using CgfConverter.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace CgfConverter.Renderers.Wavefront;

public class WavefrontModelRenderer : IRenderer
{
    protected readonly ArgsHandler Args;
    protected readonly CryEngine CryData;
    public readonly FileInfo OutputFile_Model;
    public readonly FileInfo OutputFile_Material;

    public WavefrontModelRenderer(ArgsHandler argsHandler, CryEngine cryEngine)
    {
        Args = argsHandler;
        CryData = cryEngine;
        OutputFile_Model = Args.FormatOutputFileName(".obj", cryEngine.InputFile);
        OutputFile_Material = Args.FormatOutputFileName(".mtl", cryEngine.InputFile);
    }

    public int CurrentVertexPosition { get; internal set; }
    public int TempIndicesPosition { get; internal set; }
    public int TempVertexPosition { get; internal set; }
    public int CurrentIndicesPosition { get; internal set; }
    public string GroupOverride { get; internal set; }
    public int FaceIndex { get; internal set; }

    /// <summary>
    /// Renders an .obj file, and matching .mat file for the current model
    /// </summary>
    public int Render()
    {
        if (Args.GroupMeshes)
            GroupOverride = Path.GetFileNameWithoutExtension(OutputFile_Model.Name);

        HelperMethods.Log(LogLevelEnum.Info, @"Output file is {0}", OutputFile_Model);

        using StreamWriter file = new StreamWriter(OutputFile_Model.FullName);
        file.WriteLine("# cgf-converter .obj export version {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
        file.WriteLine("#");

        if (OutputFile_Material.Exists)
            file.WriteLine("mtllib {0}", OutputFile_Material.Name);

        FaceIndex = 1;

        var nullParents = CryData.Nodes.Where(p => p.ParentNode is null).ToArray();

        if (nullParents.Length > 1)
        {
            foreach (var node in nullParents)
            {
                HelperMethods.Log(LogLevelEnum.Warning, "Rendering node with null parent {0}", node.Name);
            }
        }

        foreach (ChunkNode node in CryData.Nodes)
        {
            if (Args.IsNodeNameExcluded(node.Name))
            {
                HelperMethods.Log(LogLevelEnum.Debug, $"Excluding node {node.Name}");
                continue;
            }

            if (node.MeshData is null)
            {
                HelperMethods.Log(LogLevelEnum.Warning, "Skipped node with missing Object {0}", node.Name);
                continue;
            }

            if (node.MeshData is not null)
                WriteObjNode(file, node);

            //switch (node.ObjectChunk.ChunkType)
            //{
            //    case ChunkType.Mesh:
            //        if ((node.ParentNode is not null) && (node.ParentNode.ChunkType != ChunkType.Node))
            //            HelperMethods.Log(LogLevelEnum.Debug, "Rendering {0} to parent {1}", node.Name, node.ParentNode.Name);

            //        // Grab the mesh and process that.
            //        WriteObjNode(file, node);
            //        break;

            //    case ChunkType.Helper: // Ignore Helpers nodes
            //        break;

            //    default:
            //        // Warn us if we're skipping other nodes of interest
            //        HelperMethods.Log(LogLevelEnum.Debug, "Skipped a {0} chunk", node.ObjectChunk.ChunkType);
            //        break;
            //}
        }

        // If this is a .chr file, just write out the hitbox info.  OBJ files can't do armatures.
        foreach (CryEngineCore.ChunkCompiledPhysicalProxies tmpProxy in CryData.Chunks.Where(a => a.ChunkType == ChunkType.CompiledPhysicalProxies))
        {
            // TODO: align these properly
            WriteObjHitBox(file, tmpProxy);
        }

        return 1;
    }

    public static float Safe(float value)
    {
        if (value == float.NegativeInfinity)
            return float.MinValue;

        if (value == float.PositiveInfinity)
            return float.MaxValue;

        if (value == float.NaN)
            return 0;

        return value;
    }

    private Matrix4x4 GetNestedTransformations(CryEngineCore.ChunkNode node)
    {
        if (node.ParentNode is not null)
            return node.Transform * GetNestedTransformations(node.ParentNode);
        else
            return node.Transform; // Is this right?
    }

    public void WriteObjNode(StreamWriter f, ChunkNode chunkNode)
    {
        if (chunkNode.MeshData is not ChunkMesh meshChunk)
            return;

        if (meshChunk.MeshSubsetsData == 0)   // This is probably wrong.  These may be parents with no geometry, but still have an offset
        {
            HelperMethods.Log(LogLevelEnum.Debug, "*******Found a Mesh chunk with no Submesh ID (ID: {0:X}, Name: {1}).  Skipping...", meshChunk.ID, chunkNode.Name);
            return;
        }
        if (meshChunk.VerticesData == 0 && meshChunk.VertsUVsData == 0)  // Mesh physics node, or geometry in second file
        {
            HelperMethods.Log(LogLevelEnum.Debug, "*******Found a Mesh chunk with no Vertex info (ID: {0:X}, Name: {1}).  Skipping...", meshChunk.ID, chunkNode.Name);
            return;
        }
        //var meshChunk = chunkNode.MeshData;
        var subsets = meshChunk.GeometryInfo.GeometrySubsets;
        var verts = meshChunk.GeometryInfo.Vertices;
        var vertsUvs = meshChunk.GeometryInfo.VertUVs;
        var uvs = meshChunk.GeometryInfo.UVs;
        var indices = meshChunk.GeometryInfo.Indices;
        var colors = meshChunk.GeometryInfo.Colors;
        var normals = meshChunk.GeometryInfo.Normals;

        // Going to assume that there is only one VerticesData datastream for now.  Need to watch for    
        // Some 801 types have vertices and not VertsUVs.
        ChunkMtlName mtlName = chunkNode._model.ChunkMap.GetValue(chunkNode.MaterialID, null) as ChunkMtlName;

        int numChildren = chunkNode.NumChildren;

        var tempVertexPosition = CurrentVertexPosition;
        var tempIndicesPosition = CurrentIndicesPosition;
        var transformSoFar = GetNestedTransformations(chunkNode);

        if (subsets is null) return;

        foreach (var subset in subsets)
        {
            if (meshChunk.VerticesData == 0)
            {
                // Dymek's code.  Scales the object by the bounding box.
                var multiplerVector = Vector3.Abs((meshChunk.MinBound - meshChunk.MaxBound) / 2f);
                if (multiplerVector.X < 1) { multiplerVector.X = 1; }
                if (multiplerVector.Y < 1) { multiplerVector.Y = 1; }
                if (multiplerVector.Z < 1) { multiplerVector.Z = 1; }
                var boundaryBoxCenter = (meshChunk.MinBound + meshChunk.MaxBound) / 2f;

                for (int j = subset.FirstVertex; j < subset.NumVertices + subset.FirstVertex; j++)
                {
                    Vector3 vertex = (vertsUvs.Data[j].Vertex * multiplerVector) + boundaryBoxCenter;

                    // Use matrix operations for the maximum performance
                    vertex = Vector3.Transform(vertex, transformSoFar);

                    f.WriteLine("v {0:F7} {1:F7} {2:F7}", Safe(vertex.X), Safe(vertex.Y), Safe(vertex.Z));
                }

                f.WriteLine();

                for (int j = subset.FirstVertex; j < subset.NumVertices + subset.FirstVertex; j++)
                {
                    f.WriteLine("vt {0:F7} {1:F7} 0", Safe(vertsUvs.Data[j].UV.U), Safe(1 - vertsUvs.Data[j].UV.V));
                }
            }
            else
            {
                for (int j = subset.FirstVertex; j < subset.NumVertices + subset.FirstVertex; j++)
                {
                    if (verts is not null)
                    {
                        // Rotate/translate the vertex
                        Vector3 vertex = Vector3.Transform(verts.Data[j], transformSoFar);

                        f.WriteLine("v {0:F7} {1:F7} {2:F7}", Safe(vertex.X), Safe(vertex.Y), Safe(vertex.Z));
                    }
                    else
                        HelperMethods.Log(LogLevelEnum.Debug, "Error rendering vertices for {0:X}", chunkNode.Name);
                }

                f.WriteLine();

                for (var j = subset.FirstVertex; j < subset.NumVertices + subset.FirstVertex; j++)
                {
                    f.WriteLine("vt {0:F7} {1:F7} 0", Safe(uvs.Data[j].U), Safe(1 - uvs.Data[j].V));
                }
            }

            f.WriteLine();

            if (meshChunk.NormalsData != 0)
            {
                for (var j = subset.FirstVertex; j < subset.NumVertices + subset.FirstVertex; j++)
                {
                    f.WriteLine("vn {0:F7} {1:F7} {2:F7}",
                        normals.Data[j].X,
                        normals.Data[j].Y,
                        normals.Data[j].Z);
                }
            }

            if (Args.Smooth)
                f.WriteLine("s {0}", FaceIndex++);

            // Now write out the faces info based on the MtlName
            for (int j = subset.FirstIndex; j < subset.NumIndices + subset.FirstIndex; j++)
            {
                f.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",    // Vertices, UVs, Normals
                    indices.Data[j] + 1 + CurrentVertexPosition,
                    indices.Data[j + 1] + 1 + CurrentVertexPosition,
                    indices.Data[j + 2] + 1 + CurrentVertexPosition);

                j += 2;
            }

            tempVertexPosition += subset.NumVertices;  // add the number of vertices so future objects can start at the right place
            tempIndicesPosition += subset.NumIndices;  // Not really used...
        }

        // Extend the current vertex, uv and normal positions by the length of those arrays.
        CurrentVertexPosition = tempVertexPosition;
        CurrentIndicesPosition = tempIndicesPosition;
    }

    public void WriteObjHitBox(StreamWriter f, CryEngineCore.ChunkCompiledPhysicalProxies chunkProx)  // Pass a bone proxy to write to the stream.  For .chr files (armatures)
    {
        // The chunkProx has the vertex and index info, so much like WriteObjNode just need to write it out.  Much simpler than WriteObjNode though in theory
        // Assume only one CompiledPhysicalProxies per .chr file (or any file for that matter).  May not be a safe bet.
        // Need the materials
        for (int i = 0; i < chunkProx.NumPhysicalProxies; i++)        // Write out all the bones
        {
            // write out this bones vertex info.
            // Need to find a way to get the material name associated with the bone, so we can link the hitbox to the body part.
            f.WriteLine("g");
            // Utils.Log(LogLevelEnum.Debug, "Num Vertices: {0} ", chunkProx.HitBoxes[i].NumVertices);
            for (int j = 0; j < chunkProx.PhysicalProxies[i].NumVertices; j++)
            {
                string s1 = string.Format("v {0:F7} {1:F7} {2:F7}",
                    chunkProx.PhysicalProxies[i].Vertices[j].X,
                    chunkProx.PhysicalProxies[i].Vertices[j].Y,
                    chunkProx.PhysicalProxies[i].Vertices[j].Z);
                f.WriteLine(s1);
            }
            f.WriteLine();
            string s7 = string.Format("g {0}", i);
            f.WriteLine(s7);

            // The material file doesn't have any elements with the Name of the material.  Use i
            string s_material = string.Format("usemtl {0}", i);
            f.WriteLine(s_material);

            for (int j = 0; j < chunkProx.PhysicalProxies[i].NumIndices; j++)
            {
                string s2 = string.Format("f {0} {1} {2}",
                    chunkProx.PhysicalProxies[i].Indices[j] + 1 + CurrentVertexPosition,
                    chunkProx.PhysicalProxies[i].Indices[j + 1] + 1 + CurrentVertexPosition,
                    chunkProx.PhysicalProxies[i].Indices[j + 2] + 1 + CurrentVertexPosition);
                f.WriteLine(s2);
                j = j + 2;
            }
            CurrentVertexPosition += (int)chunkProx.PhysicalProxies[i].NumVertices;
            CurrentIndicesPosition += (int)chunkProx.PhysicalProxies[i].NumIndices;
            f.WriteLine();
        }
        f.WriteLine();
    }
}
