using CgfConverter.Utilities;
using System;
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
        // We need to create the obj header, then for each submesh write the vertex, UV and normal data.
        // First, let's figure out the name of the output file.  Should be <object name>.obj

        // Each Mesh will have a mesh subset and a series of datastream objects.  Need temporary pointers to these
        // so we can manipulate

        // Get object name.  This is the Root Node chunk Name
        // Get the objOutputFile name

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

        foreach (CryEngineCore.ChunkNode node in CryData.Nodes)
        {
            if (Args.IsNodeNameExcluded(node.Name))
            {
                HelperMethods.Log(LogLevelEnum.Debug, $"Excluding node {node.Name}");
                continue;
            }

            if (node.ObjectChunk is null)
            {
                HelperMethods.Log(LogLevelEnum.Warning, "Skipped node with missing Object {0}", node.Name);
                continue;
            }

            switch (node.ObjectChunk.ChunkType)
            {
                case ChunkType.Mesh:
                    // Render Meshes

                    if ((node.ParentNode is not null) && (node.ParentNode.ChunkType != ChunkType.Node))
                    {
                        HelperMethods.Log(LogLevelEnum.Debug, "Rendering {0} to parent {1}", node.Name, node.ParentNode.Name);
                    }

                    // Grab the mesh and process that.
                    WriteObjNode(file, node);
                    break;

                case ChunkType.Helper:
                    // Ignore Helpers nodes
                    // TODO: Investigate if there's something we should do here
                    break;

                default:
                    // Warn us if we're skipping other nodes of interest
                    HelperMethods.Log(LogLevelEnum.Debug, "Skipped a {0} chunk", node.ObjectChunk.ChunkType);
                    break;
            }
        }

        // If this is a .chr file, just write out the hitbox info.  OBJ files can't do armatures.
        foreach (CryEngineCore.ChunkCompiledPhysicalProxies tmpProxy in CryData.Chunks.Where(a => a.ChunkType == ChunkType.CompiledPhysicalProxies))
        {
            // TODO: align these properly
            WriteObjHitBox(file, tmpProxy);
        }
        // End of writing the output file

        return 1;
    }

    public float Safe(float value)
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

    public void WriteObjNode(StreamWriter f, CryEngineCore.ChunkNode chunkNode)  // Pass a node to this to have it write to the Stream
    {
        // Get the Transform here. It's the node chunk Transform.m(41/42/42) divided by 100, added to the parent transform.
        // The transform of a child has to add the transforms of ALL the parents.

        CryEngineCore.ChunkMesh tmpMesh = chunkNode.ObjectChunk as CryEngineCore.ChunkMesh;

        if (tmpMesh is null)
            return;

        if (tmpMesh.MeshSubsetsData == 0)   // This is probably wrong.  These may be parents with no geometry, but still have an offset
        {
            HelperMethods.Log(LogLevelEnum.Debug, "*******Found a Mesh chunk with no Submesh ID (ID: {0:X}, Name: {1}).  Skipping...", tmpMesh.ID, chunkNode.Name);
            // tmpMesh.WriteChunk();
            // Utils.Log(LogLevelEnum.Debug, "Node Chunk: {0}", chunkNode.Name);
            // transform = cgfData.GetTransform(chunkNode, transform);
            return;
        }
        if (tmpMesh.VerticesData == 0 && tmpMesh.VertsUVsData == 0)  // This is probably wrong.  These may be parents with no geometry, but still have an offset
        {
            HelperMethods.Log(LogLevelEnum.Debug, "*******Found a Mesh chunk with no Vertex info (ID: {0:X}, Name: {1}).  Skipping...", tmpMesh.ID, chunkNode.Name);
            //tmpMesh.WriteChunk();
            //Utils.Log(LogLevelEnum.Debug, "Node Chunk: {0}", chunkNode.Name);
            //transform = cgfData.GetTransform(chunkNode, transform);
            return;
        }

        // Going to assume that there is only one VerticesData datastream for now.  Need to watch for    
        // Some 801 types have vertices and not VertsUVs.
        CryEngineCore.ChunkMtlName tmpMtlName = chunkNode._model.ChunkMap.GetValue(chunkNode.MaterialID, null) as CryEngineCore.ChunkMtlName;
        CryEngineCore.ChunkMeshSubsets tmpMeshSubsets = tmpMesh._model.ChunkMap.GetValue(tmpMesh.MeshSubsetsData, null) as CryEngineCore.ChunkMeshSubsets; // Listed as Object ID for the Node
        CryEngineCore.ChunkDataStream tmpIndices = tmpMesh._model.ChunkMap.GetValue(tmpMesh.IndicesData, null) as CryEngineCore.ChunkDataStream;
        CryEngineCore.ChunkDataStream tmpVertices = tmpMesh._model.ChunkMap.GetValue(tmpMesh.VerticesData, null) as CryEngineCore.ChunkDataStream;
        CryEngineCore.ChunkDataStream tmpNormals = tmpMesh._model.ChunkMap.GetValue(tmpMesh.NormalsData, null) as CryEngineCore.ChunkDataStream;
        CryEngineCore.ChunkDataStream tmpUVs = tmpMesh._model.ChunkMap.GetValue(tmpMesh.UVsData, null) as CryEngineCore.ChunkDataStream;
        CryEngineCore.ChunkDataStream tmpVertsUVs = tmpMesh._model.ChunkMap.GetValue(tmpMesh.VertsUVsData, null) as CryEngineCore.ChunkDataStream;

        // We only use 3 things in obj files:  vertices, normals and UVs.  No need to process the Tangents.

        int numChildren = chunkNode.NumChildren;           // use in a for loop to print the mesh for each child

        var tempVertexPosition = CurrentVertexPosition;
        var tempIndicesPosition = CurrentIndicesPosition;
        var transformSoFar = GetNestedTransformations(chunkNode);

        foreach (var meshSubset in tmpMeshSubsets.MeshSubsets)
        {
            if (tmpMesh.VerticesData == 0)
            {
                // Dymek's code.  Scales the object by the bounding box.
                var multiplerVector = Vector3.Abs((tmpMesh.MinBound - tmpMesh.MaxBound) / 2f);
                if (multiplerVector.X < 1) { multiplerVector.X = 1; }
                if (multiplerVector.Y < 1) { multiplerVector.Y = 1; }
                if (multiplerVector.Z < 1) { multiplerVector.Z = 1; }
                var boundaryBoxCenter = (tmpMesh.MinBound + tmpMesh.MaxBound) / 2f;

                // Probably using VertsUVs (3.7+).  Write those vertices out. Do UVs at same time.
                for (int j = meshSubset.FirstVertex;
                    j < meshSubset.NumVertices + meshSubset.FirstVertex;
                    j++)
                {
                    // Let's try this using this node chunk's rotation matrix, and the transform is the sum of all the transforms.
                    // Get the transform.
                    Vector3 vertex = (tmpVertsUVs.Vertices[j] * multiplerVector) + boundaryBoxCenter;

                    // Use matrix operations for the maximum performance
                    vertex = Vector3.Transform(vertex, transformSoFar);

                    f.WriteLine("v {0:F7} {1:F7} {2:F7}", Safe(vertex.X), Safe(vertex.Y), Safe(vertex.Z));
                }

                f.WriteLine();

                for (int j = meshSubset.FirstVertex;
                    j < meshSubset.NumVertices + meshSubset.FirstVertex;
                    j++)
                {
                    f.WriteLine("vt {0:F7} {1:F7} 0", Safe(tmpVertsUVs.UVs[j].U), Safe(1 - tmpVertsUVs.UVs[j].V));
                }
            }
            else
            {
                for (int j = meshSubset.FirstVertex;
                    j < meshSubset.NumVertices + meshSubset.FirstVertex;
                    j++)
                {
                    if (tmpVertices is not null)
                    {
                        // Rotate/translate the vertex
                        // Use matrix operations for the maximum performance
                        Vector3 vertex = Vector3.Transform(tmpVertices.Vertices[j], transformSoFar);

                        f.WriteLine("v {0:F7} {1:F7} {2:F7}", Safe(vertex.X), Safe(vertex.Y), Safe(vertex.Z));
                    }
                    else
                    {
                        HelperMethods.Log(LogLevelEnum.Debug, "Error rendering vertices for {0:X}", chunkNode.Name);
                    }
                }

                f.WriteLine();

                for (var j = meshSubset.FirstVertex;
                    j < meshSubset.NumVertices + meshSubset.FirstVertex;
                    j++)
                {
                    f.WriteLine("vt {0:F7} {1:F7} 0", Safe(tmpUVs.UVs[j].U), Safe(1 - tmpUVs.UVs[j].V));
                }
            }

            f.WriteLine();

            if (tmpMesh.NormalsData != 0)
            {
                for (var j = meshSubset.FirstVertex;
                    j < meshSubset.NumVertices + meshSubset.FirstVertex;
                    j++)
                {
                    f.WriteLine("vn {0:F7} {1:F7} {2:F7}",
                        tmpNormals.Normals[j].X,
                        tmpNormals.Normals[j].Y,
                        tmpNormals.Normals[j].Z);
                }
            }

            // f.WriteLine("g {0}", GroupOverride ?? chunkNode.Name);
            
            if (Args.Smooth)
            {
                f.WriteLine("s {0}", FaceIndex++);
            }               

            // Now write out the faces info based on the MtlName
            for (int j = meshSubset.FirstIndex;
                j < meshSubset.NumIndices + meshSubset.FirstIndex;
                j++)
            {
                f.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",    // Vertices, UVs, Normals
                    tmpIndices.Indices[j] + 1 + CurrentVertexPosition,
                    tmpIndices.Indices[j + 1] + 1 + CurrentVertexPosition,
                    tmpIndices.Indices[j + 2] + 1 + CurrentVertexPosition);

                j += 2;
            }

            tempVertexPosition += meshSubset.NumVertices;  // add the number of vertices so future objects can start at the right place
            tempIndicesPosition += meshSubset.NumIndices;  // Not really used...
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

        //
        // Utils.Log(LogLevelEnum.Debug, "There are {0} Bones", chunkProx.NumBones);
        for (int i = 0; i < chunkProx.NumPhysicalProxies; i++)        // Write out all the bones
        {
            // write out this bones vertex info.
            // Need to find a way to get the material name associated with the bone, so we can link the hitbox to the body part.
            f.WriteLine("g");
            // Utils.Log(LogLevelEnum.Debug, "Num Vertices: {0} ", chunkProx.HitBoxes[i].NumVertices);
            for (int j = 0; j < chunkProx.PhysicalProxies[i].NumVertices; j++)
            {
                //Utils.Log(LogLevelEnum.Debug, "{0} {1} {2}", chunkProx.HitBoxes[i].Vertices[j].x, chunkProx.HitBoxes[i].Vertices[j].y, chunkProx.HitBoxes[i].Vertices[j].z);
                // Transform the vertex
                //Vector3 vertex = chunkNode.GetTransform(tmpVertsUVs.Vertices[j]);

                string s1 = String.Format("v {0:F7} {1:F7} {2:F7}",
                    chunkProx.PhysicalProxies[i].Vertices[j].X,
                    chunkProx.PhysicalProxies[i].Vertices[j].Y,
                    chunkProx.PhysicalProxies[i].Vertices[j].Z);
                f.WriteLine(s1);
            }
            f.WriteLine();
            string s7 = String.Format("g {0}", i);
            f.WriteLine(s7);

            // The material file doesn't have any elements with the Name of the material.  Use i
            string s_material = String.Format("usemtl {0}", i);
            f.WriteLine(s_material);

            for (int j = 0; j < chunkProx.PhysicalProxies[i].NumIndices; j++)
            {
                //string s2 = String.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                string s2 = String.Format("f {0} {1} {2}",
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
        // CurrentVertexPosition = TempVertexPosition;
        // CurrentIndicesPosition = TempIndicesPosition;

    }
}
