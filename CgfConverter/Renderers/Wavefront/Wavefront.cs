using CgfConverter.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Extensions;

namespace CgfConverter
{
    public partial class Wavefront : BaseRenderer
    {
        public Wavefront(ArgsHandler argsHandler, CryEngine cryEngine) : base(argsHandler, cryEngine) { }

        public FileInfo OutputFile_Model { get; internal set; }
        public FileInfo OutputFile_Material { get; internal set; }
        public int CurrentVertexPosition { get; internal set; }
        public int TempIndicesPosition { get; internal set; }
        public int TempVertexPosition { get; internal set; }
        public int CurrentIndicesPosition { get; internal set; }
        public String GroupOverride { get; internal set; }
        public Int32 FaceIndex { get; internal set; }

        /// <summary>
        /// Renders an .obj file, and matching .mat file for the current model
        /// </summary>
        /// <param name="outputDir">Folder to write files to</param>
        /// <param name="preservePath">When using an <paramref name="outputDir"/>, preserve the original hierarchy</param>
        public override void Render(String outputDir = null, Boolean preservePath = true)
        {
            // We need to create the obj header, then for each submesh write the vertex, UV and normal data.
            // First, let's figure out the name of the output file.  Should be <object name>.obj

            // Each Mesh will have a mesh subset and a series of datastream objects.  Need temporary pointers to these
            // so we can manipulate

            // Get object name.  This is the Root Node chunk Name
            // Get the objOutputFile name

            // String outputFile = outputFile;

            this.OutputFile_Model = new FileInfo(this.GetOutputFile("obj", outputDir, preservePath));
            this.OutputFile_Material = new FileInfo(this.GetOutputFile("mtl", outputDir, preservePath));

            if (this.Args.GroupMeshes)
                this.GroupOverride = Path.GetFileNameWithoutExtension(this.OutputFile_Model.Name);

            Console.WriteLine(@"Output file is {0}\...\{1}", outputDir, this.OutputFile_Model.Name);

            if (!OutputFile_Model.Directory.Exists)
                OutputFile_Model.Directory.Create();

            this.WriteMaterial(this.CryData);

            using (StreamWriter file = new StreamWriter(OutputFile_Model.FullName))
            {
                file.WriteLine("# cgf-converter .obj export version {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                file.WriteLine("#");

                if (OutputFile_Material.Exists)
                    file.WriteLine("mtllib {0}", OutputFile_Material.Name);

                this.FaceIndex = 1;

                var nullParents = this.CryData.NodeMap.Values.Where(p => p.ParentNode == null).ToArray();

                if (nullParents.Length > 1)
                {
                    foreach (var node in nullParents)
                    {
                        Utils.Log(LogLevelEnum.Warning, "Rendering node with null parent {0}", node.Name);
                    }
                }

                foreach (CryEngineCore.ChunkNode node in this.CryData.NodeMap.Values)
                {
                    // Don't render shields
                    if (this.Args.SkipShieldNodes && node.Name.StartsWith("$shield"))
                    {
                        Utils.Log(LogLevelEnum.Debug, "Skipped shields node {0}", node.Name);
                        continue;
                    }

                    // Don't render shields
                    if (this.Args.SkipProxyNodes && node.Name.StartsWith("proxy"))
                    {
                        Utils.Log(LogLevelEnum.Debug, "Skipped proxy node {0}", node.Name);
                        continue;
                    }

                    if (node.ObjectChunk == null)
                    {
                        Utils.Log(LogLevelEnum.Warning, "Skipped node with missing Object {0}", node.Name);
                        continue;
                    }

                    switch (node.ObjectChunk.ChunkType)
                    {
                        #region case ChunkTypeEnum.Mesh:

                        case ChunkType.Mesh:
                            // Render Meshes

                            if ((node.ParentNode != null) && (node.ParentNode.ChunkType != ChunkType.Node))
                            {
                                Utils.Log(LogLevelEnum.Debug, "Rendering {0} to parent {1}", node.Name, node.ParentNode.Name);
                            }

                            // TODO: Transform Root Nodes here?

                            file.WriteLine("o {0}", node.Name);
                            // Grab the mesh and process that.
                            this.WriteObjNode(file, node);
                            break;

                        #endregion
                        #region case ChunkTypeEnum.Helper:

                        case ChunkType.Helper:
                            // Ignore Helpers nodes
                            // TODO: Investigate if there's something we should do here
                            break;

                        #endregion
                        #region default:

                        default:
                            // Warn us if we're skipping other nodes of interest
                            Utils.Log(LogLevelEnum.Debug, "Skipped a {0} chunk", node.ObjectChunk.ChunkType);
                            break;

                            #endregion
                    }
                }

                // If this is a .chr file, just write out the hitbox info.  OBJ files can't do armatures.
                foreach (CryEngineCore.ChunkCompiledPhysicalProxies tmpProxy in this.CryData.Chunks.Where(a => a.ChunkType == ChunkType.CompiledPhysicalProxies))
                {
                    // TODO: align these properly
                    this.WriteObjHitBox(file, tmpProxy);
                }

            }  // End of writing the output file
        }

        public Double safe(Double value)
        {
            if (value == Double.NegativeInfinity)
                return Double.MinValue;

            if (value == Double.PositiveInfinity)
                return Double.MaxValue;

            if (value == Double.NaN)
                return 0;

            return value;
        }

        public void WriteObjNode(StreamWriter f, CryEngineCore.ChunkNode chunkNode)  // Pass a node to this to have it write to the Stream
        {
            // Get the Transform here. It's the node chunk Transform.m(41/42/42) divided by 100, added to the parent transform.
            // The transform of a child has to add the transforms of ALL the parents.

            CryEngineCore.ChunkMesh tmpMesh = chunkNode.ObjectChunk as CryEngineCore.ChunkMesh;

            if (tmpMesh == null)
                return;

            if (tmpMesh.MeshSubsetsData == 0)   // This is probably wrong.  These may be parents with no geometry, but still have an offset
            {
                Utils.Log(LogLevelEnum.Debug, "*******Found a Mesh chunk with no Submesh ID (ID: {0:X}, Name: {1}).  Skipping...", tmpMesh.ID, chunkNode.Name);
                // tmpMesh.WriteChunk();
                // Utils.Log(LogLevelEnum.Debug, "Node Chunk: {0}", chunkNode.Name);
                // transform = cgfData.GetTransform(chunkNode, transform);
                return;
            }
            if (tmpMesh.VerticesData == 0 && tmpMesh.VertsUVsData == 0)  // This is probably wrong.  These may be parents with no geometry, but still have an offset
            {
                Utils.Log(LogLevelEnum.Debug, "*******Found a Mesh chunk with no Vertex info (ID: {0:X}, Name: {1}).  Skipping...", tmpMesh.ID, chunkNode.Name);
                //tmpMesh.WriteChunk();
                //Utils.Log(LogLevelEnum.Debug, "Node Chunk: {0}", chunkNode.Name);
                //transform = cgfData.GetTransform(chunkNode, transform);
                return;
            }

            // Going to assume that there is only one VerticesData datastream for now.  Need to watch for this.   
            // Some 801 types have vertices and not VertsUVs.
            CryEngineCore.ChunkMtlName tmpMtlName = chunkNode._model.ChunkMap.GetValue(chunkNode.MatID, null) as CryEngineCore.ChunkMtlName;
            CryEngineCore.ChunkMeshSubsets tmpMeshSubsets = tmpMesh._model.ChunkMap.GetValue(tmpMesh.MeshSubsetsData, null) as CryEngineCore.ChunkMeshSubsets; // Listed as Object ID for the Node
            CryEngineCore.ChunkDataStream tmpIndices = tmpMesh._model.ChunkMap.GetValue(tmpMesh.IndicesData, null) as CryEngineCore.ChunkDataStream;
            CryEngineCore.ChunkDataStream tmpVertices = tmpMesh._model.ChunkMap.GetValue(tmpMesh.VerticesData, null) as CryEngineCore.ChunkDataStream;
            CryEngineCore.ChunkDataStream tmpNormals = tmpMesh._model.ChunkMap.GetValue(tmpMesh.NormalsData, null) as CryEngineCore.ChunkDataStream;
            CryEngineCore.ChunkDataStream tmpUVs = tmpMesh._model.ChunkMap.GetValue(tmpMesh.UVsData, null) as CryEngineCore.ChunkDataStream;
            CryEngineCore.ChunkDataStream tmpVertsUVs = tmpMesh._model.ChunkMap.GetValue(tmpMesh.VertsUVsData, null) as CryEngineCore.ChunkDataStream;

            // We only use 3 things in obj files:  vertices, normals and UVs.  No need to process the Tangents.

            int numChildren = chunkNode.__NumChildren;           // use in a for loop to print the mesh for each child

            var tempVertexPosition = CurrentVertexPosition;
            var tempIndicesPosition = CurrentIndicesPosition;
            var localTransform = chunkNode.LocalTransform;

            foreach (var meshSubset in tmpMeshSubsets.MeshSubsets)
            {
                // Write vertices data for each MeshSubSet (v)
                f.WriteLine("g {0}", this.GroupOverride ?? chunkNode.Name);

                if (tmpMesh.VerticesData == 0)
                {
                    #region Write Vertices Out (v, vt)

                    // Probably using VertsUVs (3.7+).  Write those vertices out. Do UVs at same time.
                    for (int j = meshSubset.FirstVertex;
                        j < meshSubset.NumVertices + meshSubset.FirstVertex;
                        j++)
                    {
                        // Let's try this using this node chunk's rotation matrix, and the transform is the sum of all the transforms.
                        // Get the transform.
                        // Dymek's code.  Scales the object by the bounding box.
                        float multiplerX = Math.Abs(tmpMesh.MinBound.X - tmpMesh.MaxBound.X) / 2f;
                        float multiplerY = Math.Abs(tmpMesh.MinBound.Y - tmpMesh.MaxBound.Y) / 2f;
                        float multiplerZ = Math.Abs(tmpMesh.MinBound.Z - tmpMesh.MaxBound.Z) / 2f;
                        if (multiplerX < 1) { multiplerX = 1; }
                        if (multiplerY < 1) { multiplerY = 1; }
                        if (multiplerZ < 1) { multiplerZ = 1; }
                        tmpVertsUVs.Vertices[j].X = tmpVertsUVs.Vertices[j].X * multiplerX + (tmpMesh.MaxBound.X + tmpMesh.MinBound.X) / 2;
                        tmpVertsUVs.Vertices[j].Y = tmpVertsUVs.Vertices[j].Y * multiplerY + (tmpMesh.MaxBound.Y + tmpMesh.MinBound.Y) / 2;
                        tmpVertsUVs.Vertices[j].Z = tmpVertsUVs.Vertices[j].Z * multiplerZ + (tmpMesh.MaxBound.Z + tmpMesh.MinBound.Z) / 2;

                        Vector3 scale;
                        Quaternion rotation;
                        Vector3 translation;
                        Matrix4x4.Decompose(localTransform, out scale, out rotation, out translation);
                        Matrix3x3 rotMatrix = rotation.ConvertToRotationMatrix();
                        Vector3 vertex = rotMatrix * tmpVertsUVs.Vertices[j];

                        f.WriteLine("v {0:F7} {1:F7} {2:F7}", safe(vertex.X), safe(vertex.Y), safe(vertex.Z));
                    }

                    f.WriteLine();

                    for (int j = meshSubset.FirstVertex;
                        j < meshSubset.NumVertices + meshSubset.FirstVertex;
                        j++)
                    {
                        f.WriteLine("vt {0:F7} {1:F7} 0", safe(tmpVertsUVs.UVs[j].U), safe(1 - tmpVertsUVs.UVs[j].V));
                    }

                    #endregion
                }
                else
                {
                    #region Write Vertices Out (v, vt)

                    for (int j = meshSubset.FirstVertex;
                        j < meshSubset.NumVertices + meshSubset.FirstVertex;
                        j++)
                    {
                        if (tmpVertices != null)
                        {
                            // Rotate/translate the vertex
                            Vector3 scale;
                            Quaternion rotation;
                            Vector3 translation;
                            Matrix4x4.Decompose(localTransform, out scale, out rotation, out translation);
                            Matrix3x3 rotMatrix = rotation.ConvertToRotationMatrix();
                            Vector3 vertex = rotMatrix * tmpVertices.Vertices[j];

                            f.WriteLine("v {0:F7} {1:F7} {2:F7}", safe(vertex.X), safe(vertex.Y), safe(vertex.Z));
                        }
                        else
                        {
                            Utils.Log(LogLevelEnum.Debug, "Error rendering vertices for {0:X}", chunkNode.Name);
                        }
                    }

                    f.WriteLine();

                    for (var j = meshSubset.FirstVertex;
                        j < meshSubset.NumVertices + meshSubset.FirstVertex;
                        j++)
                    {
                        f.WriteLine("vt {0:F7} {1:F7} 0", safe(tmpUVs.UVs[j].U), safe(1 - tmpUVs.UVs[j].V));
                    }

                    #endregion
                }

                f.WriteLine();

                #region Write Normals Block (vn)

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

                #endregion

                // #region Write Group (g)
                // 
                // f.WriteLine("g {0}", this.GroupOverride ?? chunkNode.Name);
                // 
                // #endregion
                if (this.Args.Smooth)
                {
                    f.WriteLine("s {0}", this.FaceIndex++);
                }

                #region Write Material Block (usemtl)

                if (this.CryData.Materials.Count > meshSubset.MatID)
                {
                    f.WriteLine("usemtl {0}", this.CryData.Materials[(int)meshSubset.MatID].Name);
                }
                else
                {
                    if (this.CryData.Materials.Count > 0)
                    {
                        Utils.Log(LogLevelEnum.Debug, "Missing Material {0}", meshSubset.MatID);
                    }

                    // The material file doesn't have any elements with the Name of the material.  Use the object name.
                    f.WriteLine("usemtl {0}_{1}", this.CryData.RootNode.Name, meshSubset.MatID);
                }

                #endregion

                // Now write out the faces info based on the MtlName
                for (int j = meshSubset.FirstIndex;
                    j < meshSubset.NumIndices + meshSubset.FirstIndex;
                    j++)
                {
                    f.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",    // Vertices, UVs, Normals
                        tmpIndices.Indices[j] + 1 + this.CurrentVertexPosition,
                        tmpIndices.Indices[j + 1] + 1 + this.CurrentVertexPosition,
                        tmpIndices.Indices[j + 2] + 1 + this.CurrentVertexPosition);

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
                        chunkProx.PhysicalProxies[i].Indices[j] + 1 + this.CurrentVertexPosition,
                        chunkProx.PhysicalProxies[i].Indices[j + 1] + 1 + this.CurrentVertexPosition,
                        chunkProx.PhysicalProxies[i].Indices[j + 2] + 1 + this.CurrentVertexPosition);
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
}