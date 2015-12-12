using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//using CgfConverter;

namespace CgfConverter
{
    class ObjFile
    {
        FileInfo objOutputFile;
        CgfData cgfData;

        public void WriteObjFile(CgfData cryData)
        {
            // We need to create the obj header, then for each submech write the vertex, UV and normal data.
            // First, let's figure out the name of the output file.  Should be <object name>.obj

            // Each Mesh will have a mesh subset and a series of datastream objects.  Need temporary pointers to these
            // so we can manipulate
            Console.WriteLine();
            // Console.WriteLine("*** Starting WriteObjFile() ***");
            Console.WriteLine();
            cgfData = cryData;

            // Get object name.  This is the Root Node chunk Name
            // Get the objOutputFile name
            objOutputFile = new FileInfo(cgfData.RootNode.Name + ".obj");
            Console.WriteLine("Output file is {0}", objOutputFile.Name);

            using (StreamWriter file = new StreamWriter(objOutputFile.Name))
            {
                string s1 = String.Format("# cgf-converter .obj export Version 0.83");
                file.WriteLine(s1);
                file.WriteLine("#");

                // Start populating the MatFile object
                cgfData.MatFile = new MaterialFile();           // This will have all the material info
                cgfData.MatFile.b = file;                       // The stream writer, so the functions know where to write.
                //cgfData.MatFile.Datafile = this;                // Reference back to this CgfData so MatFile can read need info (like chunks)

                cgfData.MatFile.GetMtlFileName(cgfData);               // Gets the MtlFile name                
                if (cgfData.MatFile.XmlMtlFile.Exists)
                {
                    Console.WriteLine("Matfile Full Name is {0}", cgfData.MatFile.XmlMtlFile.FullName);
                    cgfData.MatFile.WriteMtlLibInfo(file);  // writes the mtllib file.
                }
                else
                {
                    Console.WriteLine("Unable to get Mtl File name (not in expected locations).");
                }

                if (cgfData.RootNode.NumChildren == 0)
                {
                    // We have a root node with no children, so simple object.
                    string s3 = String.Format("o {0}", cgfData.RootNode.Name);
                    file.WriteLine(s3);

                    WriteObjNode(file, cgfData.RootNode);
                }
                else
                {
                    // Not a simple object.  Will need to call WriteObjNode for each Node Chunk
                    foreach (CgfData.ChunkNode tmpNode in cgfData.CgfChunks.Where(a => a.chunkType == ChunkType.Node))
                    {
                        if (tmpNode.MatID != 0)  // because we don't want to process an object with no material.  ...maybe we do
                        {
                            //tmpNode.WriteChunk();
                            string s3 = String.Format("o {0}", tmpNode.Name);
                            file.WriteLine(s3);
                            // Grab the mesh and process that.
                            WriteObjNode(file, tmpNode);
                        }
                        else
                        {
                            //Console.WriteLine("****** DIDN'T WRITE THIS NODE *****");
                            //tmpNode.WriteChunk();
                        }
                    }
                }

                // If this is a .chr file, just write out the hitbox info.  OBJ files can't do armatures.
                foreach (CgfData.ChunkCompiledPhysicalProxies tmpProxy in cgfData.CgfChunks.Where(a => a.chunkType == ChunkType.CompiledPhysicalProxies))
                {
                    //string s_hitbox = String.Format("o Hitbox");
                    //file.WriteLine(s_hitbox);
                    WriteObjHitBox(file, tmpProxy);
                }

            }  // End of writing the output file
        }
        public void WriteObjNode(StreamWriter f, CgfData.ChunkNode chunkNode)  // Pass a node to this to have it write to the Stream
        {
            // Console.WriteLine("\n*** Processing Chunk node {0:X}", chunkNode.id);
            // Console.WriteLine("***     Object ID {0:X}", chunkNode.Object);

            // We are only processing Nodes that have Materials.  The chunkType should never be Helper.  Check for Nodes to not process
            CgfData.ChunkMesh tmpMesh = (CgfData.ChunkMesh)cgfData.ChunkDictionary[chunkNode.Object];
            Vector3 transform = new Vector3();

            // Get the Transform here. It's the node chunk Transform.m(41/42/42) divided by 100, added to the parent transform.
            // The transform of a child has to add the transforms of ALL the parents.  Need to use regression?  Maybe a while loop...
            transform.x = 0;  // initializing the transform vector.  
            transform.y = 0;
            transform.z = 0;
            if (chunkNode.Parent != 0xFFFFFFFF)
            {
                // Not the parent node.  Parent node shouldn't have a transform, so no need to calculate it.
                // add the current node's transform to transform.x, y and z.
                transform = cgfData.GetTransform(chunkNode, transform);
            }

            if (cgfData.ChunkDictionary[chunkNode.Object].chunkType == ChunkType.Helper)
            {
                Console.WriteLine("*********************Found a node chunk for a Helper (ID: {0:X}).  Skipping...", tmpMesh.id);
                //tmpMesh.WriteChunk();
                return;
            }

            if (tmpMesh.MeshSubsets == 0)   // This is probably wrong.  These may be parents with no geometry, but still have an offset
            {
                Console.WriteLine("*********************Found a Mesh chunk with no Submesh ID (ID: {0:X}).  Skipping...", tmpMesh.id);
                //tmpMesh.WriteChunk();
                return;
            }
            if (tmpMesh.VerticesData == 0 && tmpMesh.VertsUVsData == 0)  // This is probably wrong.  These may be parents with no geometry, but still have an offset
            {
                Console.WriteLine("*********************Found a Mesh chunk with no Vertex info (ID: {0:X}).  Skipping...", tmpMesh.id);
                //tmpMesh.WriteChunk();
                return;
            }
            CgfData.ChunkMtlName tmpMtlName = (CgfData.ChunkMtlName)cgfData.ChunkDictionary[chunkNode.MatID];
            CgfData.ChunkMeshSubsets tmpMeshSubsets = new CgfData.ChunkMeshSubsets();
            tmpMeshSubsets = (CgfData.ChunkMeshSubsets)cgfData.ChunkDictionary[tmpMesh.MeshSubsets];  // Listed as Object ID for the Node

            // Going to assume that there is only one VerticesData datastream for now.  Need to watch for this.   
            // Some 801 types have vertices and not VertsUVs.
            //Console.WriteLine("TEMPMESH WITH NO CurrentIndicesPosition");
            //tmpMesh.WriteChunk();
            CgfData.ChunkDataStream tmpIndices = (CgfData.ChunkDataStream)cgfData.ChunkDictionary[tmpMesh.IndicesData];
            CgfData.ChunkDataStream tmpNormals = new CgfData.ChunkDataStream();
            CgfData.ChunkDataStream tmpUVs = new CgfData.ChunkDataStream();
            CgfData.ChunkDataStream tmpVertices = new CgfData.ChunkDataStream();
            CgfData.ChunkDataStream tmpVertsUVs = new CgfData.ChunkDataStream();

            if (tmpMesh.VerticesData != 0)
            {
                tmpVertices = (CgfData.ChunkDataStream)cgfData.ChunkDictionary[tmpMesh.VerticesData];
            }
            if (tmpMesh.NormalsData != 0)
            {
                tmpNormals = (CgfData.ChunkDataStream)cgfData.ChunkDictionary[tmpMesh.NormalsData];
            }
            if (tmpMesh.UVsData != 0)
            {
                tmpUVs = (CgfData.ChunkDataStream)cgfData.ChunkDictionary[tmpMesh.UVsData];
            }
            if (tmpMesh.VertsUVsData != 0)
            {
                tmpVertsUVs = (CgfData.ChunkDataStream)cgfData.ChunkDictionary[tmpMesh.VertsUVsData];
            }
            // We only use 3 things in obj files:  vertices, normals and UVs.  No need to process the Tangents.

            uint numChildren = chunkNode.NumChildren;           // use in a for loop to print the mesh for each child

            for (int i = 0; i < tmpMeshSubsets.NumMeshSubset; i++)
            {
                // Write vertices data for each MeshSubSet (v)
                f.WriteLine("g");
                if (tmpMesh.VerticesData == 0)
                {
                    // Probably using VertsUVs (3.7+).  Write those vertices out. Do UVs at same time.
                    for (int j = (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex;
                        j < (int)tmpMeshSubsets.MeshSubsets[i].NumVertices + (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex;
                        j++)
                    {
                        string s4 = String.Format("v {0:F7} {1:F7} {2:F7}",
                            tmpVertsUVs.Vertices[j].x + transform.x,
                            tmpVertsUVs.Vertices[j].y + transform.y,
                            tmpVertsUVs.Vertices[j].z + transform.z);
                        f.WriteLine(s4);
                    }
                    f.WriteLine();
                    for (int j = (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex;
                        j < (int)tmpMeshSubsets.MeshSubsets[i].NumVertices + (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex;
                        j++)
                    {
                        string s4 = String.Format("vt {0:F7} {1:F7} 0",
                            tmpVertsUVs.UVs[j].U,
                            1 - tmpVertsUVs.UVs[j].V);
                        f.WriteLine(s4);
                    }
                    f.WriteLine();
                }
                else
                {
                    // Using Verts.  Write those vertices out.
                    for (int j = (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex;
                        j < (int)tmpMeshSubsets.MeshSubsets[i].NumVertices + (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex;
                        j++)
                    {
                        string s4 = String.Format("v {0:F7} {1:F7} {2:F7}",
                            tmpVertices.Vertices[j].x + transform.x,
                            tmpVertices.Vertices[j].y + transform.y,
                            tmpVertices.Vertices[j].z + transform.z);
                        f.WriteLine(s4);
                    }
                    f.WriteLine();
                    for (int j = (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex;
                        j < (int)tmpMeshSubsets.MeshSubsets[i].NumVertices + (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex;
                        j++)
                    {
                        string s5 = String.Format("vt {0:F7} {1:F7} 0",
                            tmpUVs.UVs[j].U,
                            1 - tmpUVs.UVs[j].V);
                        f.WriteLine(s5);
                    }
                    //f.WriteLine();

                }

                // Write Normals block (vn)
                if (tmpMesh.NormalsData != 0)
                {
                    for (int j = (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex;
                        j < (int)tmpMeshSubsets.MeshSubsets[i].NumVertices + (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex;
                        j++)
                    {
                        string s6 = String.Format("vn {0:F7} {1:F7} {2:F7}",
                            tmpNormals.Normals[j].x,
                            tmpNormals.Normals[j].y,
                            tmpNormals.Normals[j].z);
                        f.WriteLine(s6);
                    }
                }

                //f.WriteLine();
                string s7 = String.Format("g {0}", chunkNode.Name);
                f.WriteLine(s7);

                // usemtl <number> refers to the position of this material in the .mtl file.  If it's 0, it's the first entry, etc. MaterialNameArray[]
                if (!cgfData.MatFile.MtlFile.Exists)     // No names in the material file.  use the chunknode name.
                {
                    // The material file doesn't have any elements with the Name of the material.  Use the object name.
                    string s_material = String.Format("usemtl {0}", cgfData.RootNode.Name);
                    f.WriteLine(s_material);
                }
                else
                {
                    string s_material = String.Format("usemtl {0}", cgfData.MatFile.MaterialNameArray[tmpMeshSubsets.MeshSubsets[i].MatID].MaterialName);
                    f.WriteLine(s_material);
                }

                // Now write out the faces info based on the MtlName
                for (int j = (int)tmpMeshSubsets.MeshSubsets[i].FirstIndex;
                    j < (int)tmpMeshSubsets.MeshSubsets[i].NumIndices + (int)tmpMeshSubsets.MeshSubsets[i].FirstIndex;
                    j++)
                {
                    string s9 = String.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",    // Vertices, UVs, Normals
                        tmpIndices.Indices[j] + 1 + cgfData.CurrentVertexPosition,
                        tmpIndices.Indices[j + 1] + 1 + cgfData.CurrentVertexPosition,
                        tmpIndices.Indices[j + 2] + 1 + cgfData.CurrentVertexPosition);
                    f.WriteLine(s9);
                    j = j + 2;
                }
                //f.WriteLine();
                cgfData.TempVertexPosition += tmpMeshSubsets.MeshSubsets[i].NumVertices;  // add the number of vertices so future objects can start at the right place
                cgfData.TempIndicesPosition += tmpMeshSubsets.MeshSubsets[i].NumIndices;  // Not really used...
            }
            // Extend the current vertex, uv and normal positions by the length of those arrays.
            cgfData.CurrentVertexPosition = cgfData.TempVertexPosition;
            cgfData.CurrentIndicesPosition = cgfData.TempIndicesPosition;
        }
        public void WriteObjHitBox(StreamWriter f, CgfData.ChunkCompiledPhysicalProxies chunkProx)  // Pass a bone proxy to write to the stream.  For .chr files (armatures)
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
                        chunkProx.HitBoxes[i].Indices[j] + 1 + cgfData.CurrentVertexPosition,
                        chunkProx.HitBoxes[i].Indices[j + 1] + 1 + cgfData.CurrentVertexPosition,
                        chunkProx.HitBoxes[i].Indices[j + 2] + 1 + cgfData.CurrentVertexPosition);
                    f.WriteLine(s2);
                    j = j + 2;
                }
                cgfData.CurrentVertexPosition += chunkProx.HitBoxes[i].NumVertices;
                cgfData.CurrentIndicesPosition += chunkProx.HitBoxes[i].NumIndices;
                f.WriteLine();
            }
            f.WriteLine();
            // CurrentVertexPosition = TempVertexPosition;
            // CurrentIndicesPosition = TempIndicesPosition;

        }

    }
}