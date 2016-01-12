using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//using CgfConverter;

namespace CgfConverter
{
    public partial class Wavefront
    {
        public Wavefront(ArgsHandler argsHandler, CryEngine cryEngine)
        {
            this.Args = argsHandler;
            this.CryData = cryEngine;
        }

        public ArgsHandler Args { get; private set; }
        public FileInfo OutputFile_Model { get; private set; }
        public FileInfo OutputFile_Material { get; private set; }
        public CryEngine CryData { get; private set; }
        public UInt32 CurrentVertexPosition { get; private set; }
        public UInt32 TempIndicesPosition { get; private set; }
        public UInt32 TempVertexPosition { get; private set; }
        public UInt32 CurrentIndicesPosition { get; private set; }
        public String GroupOverride { get; private set; }

        /// <summary>
        /// Renders an .obj file, and matching .mat file for the current model
        /// </summary>
        /// <param name="outputDir">Folder to write files to</param>
        /// <param name="preservePath">When using an <paramref name="outputDir"/>, preserve the original hierarchy</param>
        public void WriteObjFile(String outputDir, Boolean preservePath)
        {
            // We need to create the obj header, then for each submesh write the vertex, UV and normal data.
            // First, let's figure out the name of the output file.  Should be <object name>.obj

            // Each Mesh will have a mesh subset and a series of datastream objects.  Need temporary pointers to these
            // so we can manipulate

            // Get object name.  This is the Root Node chunk Name
            // Get the objOutputFile name

            // String outputFile = outputFile;

            String outputFile = "temp.obj";

            if (String.IsNullOrWhiteSpace(outputDir))
            {
                // Empty output directory means place alongside original models
                // If you want relative path, use "."

                outputFile = Path.Combine(new FileInfo(this.CryData.InputFile).DirectoryName, String.Format("{0}_out.obj", Path.GetFileNameWithoutExtension(this.CryData.InputFile)));
            }
            else
            {
                // If we have an output directory
                String preserveDir = preservePath ? Path.GetDirectoryName(this.CryData.InputFile) : "";
                outputFile = Path.Combine(outputDir, preserveDir, Path.ChangeExtension(Path.GetFileNameWithoutExtension(this.CryData.InputFile), "obj"));
            }

            
            if (this.Args.GroupMeshes)
                this.GroupOverride = Path.GetFileNameWithoutExtension(outputFile);

            this.OutputFile_Model = new FileInfo(outputFile);
            this.OutputFile_Material = new FileInfo(Path.ChangeExtension(OutputFile_Model.FullName, "mtl"));

            Console.WriteLine("Output file is {0}", OutputFile_Model.Name);

            if (!OutputFile_Model.Directory.Exists)
                OutputFile_Model.Directory.Create();

            this.WriteMaterial(this.CryData);

            using (StreamWriter file = new StreamWriter(OutputFile_Model.FullName))
            {
                string s1 = String.Format("# cgf-converter .obj export Version 0.84");
                file.WriteLine(s1);
                file.WriteLine("#");

                if (OutputFile_Material.Exists)
                    file.WriteLine("mtllib {0}", OutputFile_Material.Name);

                foreach (CryEngine.Model.ChunkNode node in this.CryData.NodeMap.Values)
                {
                    if (node.ObjectChunk == null)
                    {
                        Console.WriteLine("Skipped node with missing Object {0}", node.Name);
                        continue;
                    }

                    switch (node.ObjectChunk.ChunkType)
                    {
                        #region case ChunkTypeEnum.Mesh:

                        case ChunkTypeEnum.Mesh:
                            // Render Meshes

                            if (node.ParentNode == null)
                            {
                                Console.WriteLine("Rendering node with null parent {0}", node.Name);
                            }
                            else
                            {
                                if (node.ParentNode.ChunkType != ChunkTypeEnum.Node)
                                {
                                    Console.WriteLine("Rendering {0} to parent {1}", node.Name, node.ParentNode.Name);
                                }
                            }

                            // TODO: Transform Root Nodes here

                            //tmpNode.WriteChunk();
                            //Console.WriteLine("Writing {0}", tmpNode.Name);
                            file.WriteLine("o {0}", node.Name);
                            // Grab the mesh and process that.
                            this.WriteObjNode(file, node);
                            break;

                        #endregion
                        #region case ChunkTypeEnum.Helper:

                        case ChunkTypeEnum.Helper:
                            // Ignore Helpers nodes
                            // TODO: Investigate if there's something we should do here
                            break;

                        #endregion
                        #region default:

                        default:
                            // Warn us if we're skipping other nodes of interest
                            Console.WriteLine("Skipped a {0} chunk", node.ObjectChunk.ChunkType.ToDescription());
                            break;

                        #endregion
                    }
                }

                // If this is a .chr file, just write out the hitbox info.  OBJ files can't do armatures.
                foreach (CryEngine.Model.ChunkCompiledPhysicalProxies tmpProxy in this.CryData.ChunksByID.Values.Where(a => a.ChunkType == ChunkTypeEnum.CompiledPhysicalProxies))
                {
                    // TODO: align these properly
                    WriteObjHitBox(file, tmpProxy);
                }

            }  // End of writing the output file
        }

        public void WriteObjNode(StreamWriter f, CryEngine.Model.ChunkNode chunkNode)  // Pass a node to this to have it write to the Stream
        {
            // Get the Transform here. It's the node chunk Transform.m(41/42/42) divided by 100, added to the parent transform.
            // The transform of a child has to add the transforms of ALL the parents.  Need to use regression?  Maybe a while loop...

            CryEngine.Model.ChunkMesh tmpMesh = chunkNode.ObjectChunk as CryEngine.Model.ChunkMesh;

            if (tmpMesh == null)
                return;

            if (tmpMesh.MeshSubsets == 0)   // This is probably wrong.  These may be parents with no geometry, but still have an offset
            {
                Console.WriteLine("*******Found a Mesh chunk with no Submesh ID (ID: {0:X}, Name: {1}).  Skipping...", tmpMesh.ID, chunkNode.Name);
                //tmpMesh.WriteChunk();
                //Console.WriteLine("Node Chunk: {0}", chunkNode.Name);
                //transform = cgfData.GetTransform(chunkNode, transform);
                return;
            }
            if (tmpMesh.VerticesData == 0 && tmpMesh.VertsUVsData == 0)  // This is probably wrong.  These may be parents with no geometry, but still have an offset
            {
                Console.WriteLine("*******Found a Mesh chunk with no Vertex info (ID: {0:X}, Name: {1}).  Skipping...", tmpMesh.ID, chunkNode.Name);
                //tmpMesh.WriteChunk();
                //Console.WriteLine("Node Chunk: {0}", chunkNode.Name);
                //transform = cgfData.GetTransform(chunkNode, transform);
                return;
            }

            CryEngine.Model.ChunkMtlName tmpMtlName = chunkNode._model.ChunkMap[chunkNode.MatID] as CryEngine.Model.ChunkMtlName;
            CryEngine.Model.ChunkMeshSubsets tmpMeshSubsets = tmpMesh._model.ChunkMap[tmpMesh.MeshSubsets] as CryEngine.Model.ChunkMeshSubsets; // Listed as Object ID for the Node
            
            // Going to assume that there is only one VerticesData datastream for now.  Need to watch for this.   
            // Some 801 types have vertices and not VertsUVs.

            CryEngine.Model.ChunkDataStream tmpIndices = new CryEngine.Model.ChunkDataStream();
            CryEngine.Model.ChunkDataStream tmpNormals = new CryEngine.Model.ChunkDataStream();
            CryEngine.Model.ChunkDataStream tmpUVs = new CryEngine.Model.ChunkDataStream();
            CryEngine.Model.ChunkDataStream tmpVertices = new CryEngine.Model.ChunkDataStream();
            CryEngine.Model.ChunkDataStream tmpVertsUVs = new CryEngine.Model.ChunkDataStream();

            if (tmpMesh.IndicesData != 0) tmpIndices = tmpMesh._model.ChunkMap[tmpMesh.IndicesData] as CryEngine.Model.ChunkDataStream;
            if (tmpMesh.VerticesData != 0) tmpVertices = tmpMesh._model.ChunkMap[tmpMesh.VerticesData] as CryEngine.Model.ChunkDataStream;
            if (tmpMesh.NormalsData != 0) tmpNormals = tmpMesh._model.ChunkMap[tmpMesh.NormalsData] as CryEngine.Model.ChunkDataStream;
            if (tmpMesh.UVsData != 0) tmpUVs = tmpMesh._model.ChunkMap[tmpMesh.UVsData] as CryEngine.Model.ChunkDataStream;
            if (tmpMesh.VertsUVsData != 0) tmpVertsUVs = tmpMesh._model.ChunkMap[tmpMesh.VertsUVsData] as CryEngine.Model.ChunkDataStream;

            // We only use 3 things in obj files:  vertices, normals and UVs.  No need to process the Tangents.

            uint numChildren = chunkNode.__NumChildren;           // use in a for loop to print the mesh for each child

            var tempVertexPosition = this.CurrentVertexPosition;
            var tempIndicesPosition = this.CurrentIndicesPosition;

            foreach (var meshSubset in tmpMeshSubsets.MeshSubsets)
            {
                // Write vertices data for each MeshSubSet (v)
                f.WriteLine("g {0}", this.GroupOverride ?? chunkNode.Name);

                if (tmpMesh.VerticesData == 0)
                {
                    #region Write Vertices Out (v, vt)

                    // Probably using VertsUVs (3.7+).  Write those vertices out. Do UVs at same time.
                    for (uint j = meshSubset.FirstVertex;
                        j < meshSubset.NumVertices + meshSubset.FirstVertex;
                        j++)
                    {
                        // Let's try this using this node chunk's rotation matrix, and the transform is the sum of all the transforms.
                        // Get the transform.
                        Vector3 vertex = chunkNode.GetTransform(tmpVertsUVs.Vertices[j]);

                        f.WriteLine("v {0:F7} {1:F7} {2:F7}", vertex.x, vertex.y, vertex.z);
                    }

                    f.WriteLine();

                    for (uint j = meshSubset.FirstVertex;
                        j < meshSubset.NumVertices + meshSubset.FirstVertex;
                        j++)
                    {
                        f.WriteLine("vt {0:F7} {1:F7} 0", tmpVertsUVs.UVs[j].U, 1 - tmpVertsUVs.UVs[j].V);
                    }

                    #endregion
                }
                else
                {
                    #region Write Vertices Out (v, vt)

                    for (uint j = meshSubset.FirstVertex;
                        j < meshSubset.NumVertices + meshSubset.FirstVertex;
                        j++)
                    {
                        if (tmpVertices != null)
                        {
                            // Rotate/translate the vertex
                            Vector3 vertex = chunkNode.GetTransform(tmpVertices.Vertices[j]);

                            f.WriteLine("v {0:F7} {1:F7} {2:F7}", vertex.x, vertex.y, vertex.z);
                        }
                        else
                        {
                            Console.WriteLine("Error rendering vertices for {0:X}", chunkNode.Name);
                        }
                    }

                    f.WriteLine();

                    for (uint j = meshSubset.FirstVertex;
                        j < meshSubset.NumVertices + meshSubset.FirstVertex;
                        j++)
                    {
                        f.WriteLine("vt {0:F7} {1:F7} 0", tmpUVs.UVs[j].U, 1 - tmpUVs.UVs[j].V);
                    }

                    #endregion
                }

                f.WriteLine();

                #region Write Normals Block (vn)

                if (tmpMesh.NormalsData != 0)
                {
                    for (uint j = meshSubset.FirstVertex;
                        j < meshSubset.NumVertices + meshSubset.FirstVertex;
                        j++)
                    {
                        f.WriteLine("vn {0:F7} {1:F7} {2:F7}",
                            tmpNormals.Normals[j].x,
                            tmpNormals.Normals[j].y,
                            tmpNormals.Normals[j].z);
                    }
                }

                #endregion

                #region Write Group (g)

                f.WriteLine("g {0}", this.GroupOverride ?? chunkNode.Name);

                #endregion

                #region Write Material Block (usemtl)

                if (this.CryData.Materials.Length > meshSubset.MatID)
                {
                    f.WriteLine("usemtl {0}", this.CryData.Materials[meshSubset.MatID].Name);
                }
                else
                {
                    Console.WriteLine("Missing Material {0}", meshSubset.MatID);

                    // The material file doesn't have any elements with the Name of the material.  Use the object name.
                    f.WriteLine("usemtl {0}", this.CryData.RootNode.Name);
                    
                }

                #endregion

                // Now write out the faces info based on the MtlName
                for (uint j = meshSubset.FirstIndex;
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
            this.CurrentVertexPosition = tempVertexPosition;
            this.CurrentIndicesPosition = tempIndicesPosition;
        }

        public void WriteObjHitBox(StreamWriter f, CryEngine.Model.ChunkCompiledPhysicalProxies chunkProx)  // Pass a bone proxy to write to the stream.  For .chr files (armatures)
        {
            // The chunkProx has the vertex and index info, so much like WriteObjNode just need to write it out.  Much simpler than WriteObjNode though in theory
            // Assume only one CompiledPhysicalProxies per .chr file (or any file for that matter).  May not be a safe bet.
            // Need the materials

            //
            // Console.WriteLine("There are {0} Bones", chunkProx.NumBones);
            for (int i = 0; i < chunkProx.NumBones; i++)        // Write out all the bones
            {
                // write out this bones vertex info.
                // Need to find a way to get the material name associated with the bone, so we can link the hitbox to the body part.
                f.WriteLine("g");
                // Console.WriteLine("Num Vertices: {0} ", chunkProx.HitBoxes[i].NumVertices);
                for (int j = 0; j < chunkProx.HitBoxes[i].NumVertices; j++)
                {
                    //Console.WriteLine("{0} {1} {2}", chunkProx.HitBoxes[i].Vertices[j].x, chunkProx.HitBoxes[i].Vertices[j].y, chunkProx.HitBoxes[i].Vertices[j].z);
                    string s1 = String.Format("v {0:F7} {1:F7} {2:F7}",
                        chunkProx.HitBoxes[i].Vertices[j].x,
                        chunkProx.HitBoxes[i].Vertices[j].y,
                        chunkProx.HitBoxes[i].Vertices[j].z);
                    f.WriteLine(s1);
                }
                f.WriteLine();
                string s7 = String.Format("g {0}", i);
                f.WriteLine(s7);

                // The material file doesn't have any elements with the Name of the material.  Use i
                string s_material = String.Format("usemtl {0}", i);
                f.WriteLine(s_material);

                for (int j = 0; j < chunkProx.HitBoxes[i].NumIndices; j++)
                {
                    //string s2 = String.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                    string s2 = String.Format("f {0} {1} {2}",
                        chunkProx.HitBoxes[i].Indices[j] + 1 + this.CurrentVertexPosition,
                        chunkProx.HitBoxes[i].Indices[j + 1] + 1 + this.CurrentVertexPosition,
                        chunkProx.HitBoxes[i].Indices[j + 2] + 1 + this.CurrentVertexPosition);
                    f.WriteLine(s2);
                    j = j + 2;
                }
                this.CurrentVertexPosition += chunkProx.HitBoxes[i].NumVertices;
                this.CurrentIndicesPosition += chunkProx.HitBoxes[i].NumIndices;
                f.WriteLine();
            }
            f.WriteLine();
            // CurrentVertexPosition = TempVertexPosition;
            // CurrentIndicesPosition = TempIndicesPosition;

        }
    }
}