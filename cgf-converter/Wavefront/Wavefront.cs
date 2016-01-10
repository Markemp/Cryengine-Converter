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
        public ArgsHandler Args { get; private set; }

        public Wavefront(ArgsHandler argsHandler)
        {
            this.Args = argsHandler;
        }

        FileInfo OutputFile_Model;
        FileInfo OutputFile_Material;
        CryEngine CryData;

        public void WriteObjFile(CryEngine cryEngine)
        {
            // We need to create the obj header, then for each submesh write the vertex, UV and normal data.
            // First, let's figure out the name of the output file.  Should be <object name>.obj

            // Each Mesh will have a mesh subset and a series of datastream objects.  Need temporary pointers to these
            // so we can manipulate
            Console.WriteLine();
            // Console.WriteLine("*** Starting WriteObjFile() ***");
            Console.WriteLine();
            this.CryData = cryEngine;

            // Get object name.  This is the Root Node chunk Name
            // Get the objOutputFile name
            OutputFile_Model = this.Args.OutputFile ?? new FileInfo(this.CryData.Asset.RootNode.Name + ".obj");
            OutputFile_Material = new FileInfo(Path.ChangeExtension(OutputFile_Model.FullName, "mtl"));

            Console.WriteLine("Output file is {0}", OutputFile_Model.Name);

            if (!OutputFile_Model.Directory.Exists)
                OutputFile_Model.Directory.Create();

            this.WriteMaterial();

            using (StreamWriter file = new StreamWriter(OutputFile_Model.FullName))
            {
                string s1 = String.Format("# cgf-converter .obj export Version 0.84");
                file.WriteLine(s1);
                file.WriteLine("#");

                if (OutputFile_Material.Exists)
                    file.WriteLine("mtllib {0}", OutputFile_Material.Name);

                if (this.CryData.Asset.RootNode.NumChildren == 0)
                {
                    Console.WriteLine("WriteOBJ:  Rootnode.numchildren == 0.");
                    // We have a root node with no children, so simple object.
                    file.WriteLine("o {0}", this.CryData.Asset.RootNode.Name);

                    WriteObjNode(file, this.CryData.Asset.RootNode);
                }
                else
                {
                    // Not a simple object.  Will need to call WriteObjNode for each Node Chunk
                    //Console.WriteLine("WriteOBJ:  Rootnode.numchildren != 0.");
                    foreach (CryEngine.Model.ChunkNode tmpNode in this.CryData.Asset.CgfChunks.Where(a => a.chunkType == ChunkType.Node))
                    {
                        //tmpNode.WriteChunk();
                        //Console.WriteLine("Writing {0}", tmpNode.Name);
                        file.WriteLine("o {0}", tmpNode.Name);
                        // Grab the mesh and process that.
                        WriteObjNode(file, tmpNode);
                    }
                }

                // If this is a .chr file, just write out the hitbox info.  OBJ files can't do armatures.
                foreach (CryEngine.Model.ChunkCompiledPhysicalProxies tmpProxy in this.CryData.Asset.CgfChunks.Where(a => a.chunkType == ChunkType.CompiledPhysicalProxies))
                {
                    //string s_hitbox = String.Format("o Hitbox");
                    //file.WriteLine(s_hitbox);
                    WriteObjHitBox(file, tmpProxy);
                }

            }  // End of writing the output file
        }
        public void WriteObjNode(StreamWriter f, CryEngine.Model.ChunkNode chunkNode)  // Pass a node to this to have it write to the Stream
        {
            // Console.WriteLine("\n*** Processing Chunk node {0:X}", chunkNode.id);
            // Console.WriteLine("***     Object ID {0:X}", chunkNode.Object);

            // We are only processing Nodes that have Materials.  The chunkType should never be Helper.  Check for Nodes to not process
            // This is wrong.  We have to process nodes that have helpers as the mesh info for the transform.
            if (this.CryData.Asset.ChunkDictionary[chunkNode.Object].chunkType == ChunkType.Helper)
            {
                // This needs work.
                //transform = cgfData.GetTransform(chunkNode, transform);
                return;
            }
            CryEngine.Model.ChunkMesh tmpMesh = (CryEngine.Model.ChunkMesh)this.CryData.Asset.ChunkDictionary[chunkNode.Object];

            // Get the Transform here. It's the node chunk Transform.m(41/42/42) divided by 100, added to the parent transform.
            // The transform of a child has to add the transforms of ALL the parents.  Need to use regression?  Maybe a while loop...

            if (chunkNode.Parent != 0xFFFFFFFF)
            {
                // Not the parent node.  Parent node shouldn't have a transform, so no need to calculate it.
                // add the current node's transform to transform.x, y and z.
                //transform = cgfData.GetTransform2(chunkNode, transform);
                //Console.WriteLine("Transform for {0} is {1},{2},{3}", chunkNode.Name, transform.x, transform.y, transform.z);
            }
            //transform.WriteVector3();

            if (this.CryData.Asset.ChunkDictionary[chunkNode.Object].chunkType == ChunkType.Helper)
            {
                // This can still have transform, so need to to the transform before skipping.  We should still write an empty, but..obj.
                Console.WriteLine("*********************Found a node chunk for a Helper (ID: {0:X}).  Skipping...", tmpMesh.id);
                //tmpMesh.WriteChunk();
                //Console.WriteLine("Node Chunk: {0}", chunkNode.Name);
                //transform = cgfData.GetTransform(chunkNode, transform);
                return;
            }

            if (tmpMesh.MeshSubsets == 0)   // This is probably wrong.  These may be parents with no geometry, but still have an offset
            {
                Console.WriteLine("*******Found a Mesh chunk with no Submesh ID (ID: {0:X}, Name: {1}).  Skipping...", tmpMesh.id, chunkNode.Name);
                //tmpMesh.WriteChunk();
                //Console.WriteLine("Node Chunk: {0}", chunkNode.Name);
                //transform = cgfData.GetTransform(chunkNode, transform);
                return;
            }
            if (tmpMesh.VerticesData == 0 && tmpMesh.VertsUVsData == 0)  // This is probably wrong.  These may be parents with no geometry, but still have an offset
            {
                Console.WriteLine("*******Found a Mesh chunk with no Vertex info (ID: {0:X}, Name: {1}).  Skipping...", tmpMesh.id, chunkNode.Name);
                //tmpMesh.WriteChunk();
                //Console.WriteLine("Node Chunk: {0}", chunkNode.Name);
                //transform = cgfData.GetTransform(chunkNode, transform);
                return;
            }
            CryEngine.Model.ChunkMtlName tmpMtlName = (CryEngine.Model.ChunkMtlName)this.CryData.Asset.ChunkDictionary[chunkNode.MatID];
            CryEngine.Model.ChunkMeshSubsets tmpMeshSubsets = (CryEngine.Model.ChunkMeshSubsets)this.CryData.Asset.ChunkDictionary[tmpMesh.MeshSubsets];  // Listed as Object ID for the Node
            
            // Going to assume that there is only one VerticesData datastream for now.  Need to watch for this.   
            // Some 801 types have vertices and not VertsUVs.
            //Console.WriteLine("TEMPMESH WITH NO CurrentIndicesPosition");
            //tmpMesh.WriteChunk();
            CryEngine.Model.ChunkDataStream tmpIndices = (CryEngine.Model.ChunkDataStream)this.CryData.Asset.ChunkDictionary[tmpMesh.IndicesData];
            CryEngine.Model.ChunkDataStream tmpNormals = new CryEngine.Model.ChunkDataStream();
            CryEngine.Model.ChunkDataStream tmpUVs = new CryEngine.Model.ChunkDataStream();
            CryEngine.Model.ChunkDataStream tmpVertices = new CryEngine.Model.ChunkDataStream();
            CryEngine.Model.ChunkDataStream tmpVertsUVs = new CryEngine.Model.ChunkDataStream();

            if (tmpMesh.VerticesData != 0)
            {
                tmpVertices = (CryEngine.Model.ChunkDataStream)this.CryData.Asset.ChunkDictionary[tmpMesh.VerticesData];
            }
            if (tmpMesh.NormalsData != 0)
            {
                tmpNormals = (CryEngine.Model.ChunkDataStream)this.CryData.Asset.ChunkDictionary[tmpMesh.NormalsData];
            }
            if (tmpMesh.UVsData != 0)
            {
                tmpUVs = (CryEngine.Model.ChunkDataStream)this.CryData.Asset.ChunkDictionary[tmpMesh.UVsData];
            }
            if (tmpMesh.VertsUVsData != 0)
            {
                tmpVertsUVs = (CryEngine.Model.ChunkDataStream)this.CryData.Asset.ChunkDictionary[tmpMesh.VertsUVsData];
            }
            // We only use 3 things in obj files:  vertices, normals and UVs.  No need to process the Tangents.

            uint numChildren = chunkNode.NumChildren;           // use in a for loop to print the mesh for each child

            foreach (var meshSubset in tmpMeshSubsets.MeshSubsets)
            {
                // Write vertices data for each MeshSubSet (v)
                f.WriteLine("g");

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
                        // Rotate/translate the vertex
                        Vector3 vertex = chunkNode.GetTransform(tmpVertices.Vertices[j]);

                        f.WriteLine("v {0:F7} {1:F7} {2:F7}", vertex.x, vertex.y, vertex.z);
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

                f.WriteLine("g {0}", chunkNode.Name);

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
                    f.WriteLine("usemtl {0}", this.CryData.Asset.RootNode.Name);
                    
                }

                #endregion

                // Now write out the faces info based on the MtlName
                for (uint j = meshSubset.FirstIndex;
                    j < meshSubset.NumIndices + meshSubset.FirstIndex;
                    j++)
                {
                    f.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",    // Vertices, UVs, Normals
                        tmpIndices.Indices[j] + 1 + this.CryData.Asset.CurrentVertexPosition,
                        tmpIndices.Indices[j + 1] + 1 + this.CryData.Asset.CurrentVertexPosition,
                        tmpIndices.Indices[j + 2] + 1 + this.CryData.Asset.CurrentVertexPosition);

                    j += 2;
                }

                this.CryData.Asset.TempVertexPosition += meshSubset.NumVertices;  // add the number of vertices so future objects can start at the right place
                this.CryData.Asset.TempIndicesPosition += meshSubset.NumIndices;  // Not really used...
            }

            // Extend the current vertex, uv and normal positions by the length of those arrays.
            this.CryData.Asset.CurrentVertexPosition = this.CryData.Asset.TempVertexPosition;
            this.CryData.Asset.CurrentIndicesPosition = this.CryData.Asset.TempIndicesPosition;
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
                        chunkProx.HitBoxes[i].Indices[j] + 1 + this.CryData.Asset.CurrentVertexPosition,
                        chunkProx.HitBoxes[i].Indices[j + 1] + 1 + this.CryData.Asset.CurrentVertexPosition,
                        chunkProx.HitBoxes[i].Indices[j + 2] + 1 + this.CryData.Asset.CurrentVertexPosition);
                    f.WriteLine(s2);
                    j = j + 2;
                }
                this.CryData.Asset.CurrentVertexPosition += chunkProx.HitBoxes[i].NumVertices;
                this.CryData.Asset.CurrentIndicesPosition += chunkProx.HitBoxes[i].NumIndices;
                f.WriteLine();
            }
            f.WriteLine();
            // CurrentVertexPosition = TempVertexPosition;
            // CurrentIndicesPosition = TempIndicesPosition;

        }
    }
}