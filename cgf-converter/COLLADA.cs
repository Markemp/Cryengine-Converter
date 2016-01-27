using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Schema;
using System.Xml.Serialization;
using grendgine_collada; // No idea how to actually use this.
using System.Reflection;

namespace CgfConverter
{
    //[System.Xml.Serialization.XmlRootAttribute(ElementName = "COLLADA", Namespace = "https://www.khronos.org/files/collada_schema_1_5", IsNullable = false)]
    //[XmlRootAttribute("COLLADA", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]

    public class COLLADA : BaseRenderer // class to export to .dae format (COLLADA)
    {
        public XmlSchema schema = new XmlSchema();
        public FileInfo daeOutputFile;
        public XmlDocument daeDoc = new XmlDocument();                      // the COLLADA XML doc.  Going to try serialized instead.
        public Grendgine_Collada daeObject = new Grendgine_Collada();       // This is the serializable class.
        XmlSerializer mySerializer = new XmlSerializer(typeof(Grendgine_Collada));

        public COLLADA(ArgsHandler argsHandler, CryEngine cryEngine) : base(argsHandler, cryEngine) { }

        public override void Render(String outputDir = null, Boolean preservePath = true)
        {
            // The root of the functions to write Collada files
            // At this point, we should have a cryData.Asset object, fully populated.
            Utils.Log(LogLevelEnum.Debug);
            Utils.Log(LogLevelEnum.Debug, "*** Starting WriteCOLLADA() ***");
            Utils.Log(LogLevelEnum.Debug);

            // File name will be "object name.dae"
            daeOutputFile = new FileInfo(this.GetOutputFile("dae", outputDir, preservePath));
            Console.WriteLine("output file: {0}", outputDir);
            GetSchema();                                                    // Loads the schema.  Needs error checking in case it's offline.
            WriteRootNode();
            WriteAsset();
            WriteLibrary_Images();
            WriteScene();
            WriteLibrary_Materials();
            WriteLibrary_Effects();
            WriteLibrary_Geometries();
            WriteLibrary_VisualScenes();
            if (!daeOutputFile.Directory.Exists)
                daeOutputFile.Directory.Create();
            TextWriter writer = new StreamWriter(daeOutputFile.FullName);   // Makes the Textwriter object for the output
            mySerializer.Serialize(writer, daeObject);                      // Serializes the daeObject and writes to the writer
            Utils.Log(LogLevelEnum.Debug, "End of Write Collada");
        }

        private void WriteRootNode()
        {
            //Grendgine_Collada collada = new Grendgine_Collada();
            daeObject.Collada_Version = "1.5.0";
        }

        public void GetSchema()                                             // Get the schema from kronos.org.  Needs error checking in case it's offline
        {
            schema.ElementFormDefault = XmlSchemaForm.Qualified;
            schema.TargetNamespace = "https://www.khronos.org/files/collada_schema_1_5";
        }

        public void WriteAsset()
        {
            // Writes the Asset element in a Collada XML doc
            DateTime fileCreated = DateTime.Now;
            DateTime fileModified = DateTime.Now;           // since this only creates, both times should be the same
            Grendgine_Collada_Asset asset = new Grendgine_Collada_Asset();
            asset.Revision = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Grendgine_Collada_Asset_Contributor[] contributors = new Grendgine_Collada_Asset_Contributor[2];
            contributors[0] = new Grendgine_Collada_Asset_Contributor();
            contributors[0].Author = "Heffay";
            contributors[0].Author_Website = "https://github.com/Markemp/Cryengine-Converter";
            contributors[0].Author_Email = "markemp@gmail.com";
            contributors[0].Source_Data = this.CryData.RootNode.Name;                    // The cgf/cga/skin/whatever file we read
            // Get the actual file creators from the Source Chunk
            contributors[1] = new Grendgine_Collada_Asset_Contributor();
            foreach (CryEngine_Core.ChunkSourceInfo tmpSource in this.CryData.Chunks.Where(a => a.ChunkType == ChunkTypeEnum.SourceInfo))
            {
                contributors[1].Author = tmpSource.Author;
                contributors[1].Source_Data = tmpSource.SourceFile;
            }
            asset.Created = fileCreated;
            asset.Modified = fileModified;
            asset.Up_Axis = "Z_UP";
            asset.Title = this.CryData.RootNode.Name;
            daeObject.Asset = asset;
            daeObject.Asset.Contributor = contributors;

        }
        public void WriteLibrary_Images()
        {
            // I think this is a  list of all the images used by the asset.
            Grendgine_Collada_Library_Images libraryImages = new Grendgine_Collada_Library_Images();
            daeObject.Library_Images = libraryImages;
            List<Grendgine_Collada_Image> imageList = new List<Grendgine_Collada_Image>();
            // We now have the image library set up.  start to populate.
            int numImages = 0;
            foreach (CryEngine_Core.Material material in CryData.Materials)
            {
                // each mat will have a number of texture files.  Need to create an <image> for each of them.
                int numTextures = material.Textures.Length;
                for (int i = 0; i < numTextures; i++)
                {
                    // For each texture in the material, we make a new <image> object and add it to the list. 
                    Grendgine_Collada_Image tmpImage = new Grendgine_Collada_Image();
                    tmpImage.ID = material.Name + "_" + material.Textures[i].Map;
                    tmpImage.Init_From = new Grendgine_Collada_Init_From();
                    // Build the URI path to the file as a .dds, clean up the slashes.
                    StringBuilder builder;
                    if (material.Textures[i].File.Contains(@"/") || material.Textures[i].File.Contains(@"\"))
                    {
                        builder = new StringBuilder(this.Args.DataDir + @"\" + material.Textures[i].File);
                    }
                    else
                    {
                        builder = new StringBuilder(material.Textures[i].File);
                    }

                    if (!this.Args.TiffTextures)
                        builder.Replace(".tif", ".dds");
                    else
                        builder.Replace(".dds", ".tif");

                    builder.Replace(@"/", @"\");

                    //tmpImage.Init_From.Ref = mats.Textures[i].File;
                    tmpImage.Init_From.Ref = builder.ToString();
                    imageList.Add(tmpImage);
                    numImages++;                    // increment the number of image files found.
                }
            }
            // images is the array of image (Gredgine_Collada_Image) objects
            Grendgine_Collada_Image[] images = imageList.ToArray();
            daeObject.Library_Images.Image = images;
        }
        public void WriteLibrary_Materials()
        {
            // Create the list of materials used in this object
            // There is just one .mtl file we need to worry about.
            Grendgine_Collada_Library_Materials libraryMaterials = new Grendgine_Collada_Library_Materials();
            // We have our top level.
            daeObject.Library_Materials = libraryMaterials;
            int numMaterials = CryData.Materials.Length;
            // Now create a material for each material in the object
            Utils.Log(LogLevelEnum.Debug, "Number of materials: {0}", numMaterials);
            Grendgine_Collada_Material[] materials = new Grendgine_Collada_Material[numMaterials];
            for (int i = 0; i < numMaterials; i++)
            {
                Grendgine_Collada_Material tmpMaterial = new Grendgine_Collada_Material();
                tmpMaterial.Name = CryData.Materials[i].Name;
                // Create the instance_effect for each material
                tmpMaterial.Instance_Effect = new Grendgine_Collada_Instance_Effect();
                // The # in front of tmpMaterial.name is needed to reference the effect in Library_effects.
                tmpMaterial.Instance_Effect.URL = "#" + tmpMaterial.Name;
                // Need material ID here, so the meshes can reference it.  Use the chunk ID.
                tmpMaterial.ID = i.ToString();          // this is the order the materials appear in the .mtl file.  Needed for geometries.
                materials[i] = tmpMaterial;
            }
            libraryMaterials.Material = materials;
        }
        public void WriteLibrary_Effects()
        {
            // The Effects library.  This is actual material stuff, so... let's get into it!  First, let's make a library effects object
            Grendgine_Collada_Library_Effects libraryEffects = new Grendgine_Collada_Library_Effects();
            daeObject.Library_Effects = libraryEffects;
            // libraryEffects contains a number of effects objects.  One effects object for each material.

        }
        public void WriteLibrary_Geometries()
        {
            // Geometry library.  this is going to be fun...
            Grendgine_Collada_Library_Geometries libraryGeometries = new Grendgine_Collada_Library_Geometries();
            libraryGeometries.ID = this.CryData.RootNode.Name;
            // Make a list for all the geometries objects we will need. Will convert to array at end.  Define the array here as well
            // Unfortunately we have to define a Geometry for EACH meshsubset in the meshsubsets, since the mesh can contain multiple materials
            List<Grendgine_Collada_Geometry> geometryList = new List<Grendgine_Collada_Geometry>();

            // For each of the nodes, we need to write the geometry.
            // Need to figure out how to assign the right material to the node as well.
            // Use a foreach statement to get all the node chunks.  This will get us the meshes, which will contain the vertex, UV and normal info.
            foreach (CryEngine_Core.ChunkNode nodeChunk in this.CryData.Chunks.Where(a => a.ChunkType == ChunkTypeEnum.Node))
            {
                // Create a geometry object.  Use the chunk ID for the geometry ID
                // Will have to be careful with this, since with .cga/.cgam pairs will need to match by Name.
                // Now make the mesh object.  This will have 3 sources, 1 vertices, and 1 triangles (with material ID)
                // If the Object ID of Node chunk points to a Helper or a Controller though, place an empty.
                // Will have to figure out transforms here too.
                // need to make a list of the sources and triangles to add to tmpGeo.Mesh
                List<Grendgine_Collada_Source> sourceList = new List<Grendgine_Collada_Source>();
                List<Grendgine_Collada_Triangles> triList = new List<Grendgine_Collada_Triangles>();
                CryEngine_Core.ChunkDataStream tmpNormals = null;
                CryEngine_Core.ChunkDataStream tmpUVs = null;
                CryEngine_Core.ChunkDataStream tmpVertices = null;
                CryEngine_Core.ChunkDataStream tmpVertsUVs = null;
                CryEngine_Core.ChunkDataStream tmpIndices = null;

                if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Mesh)
                {
                    // Get the mesh chunk and submesh chunk for this node.
                    CryEngine_Core.ChunkMesh tmpMeshChunk = (CryEngine_Core.ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                    CryEngine_Core.ChunkMeshSubsets tmpMeshSubsets = (CryEngine_Core.ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node

                    // Get pointers to the vertices data
                    if (tmpMeshChunk.VerticesData != 0)
                    {
                        tmpVertices = (CryEngine_Core.ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.VerticesData];
                    }
                    if (tmpMeshChunk.NormalsData != 0)
                    {
                        tmpNormals = (CryEngine_Core.ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.NormalsData];
                    }
                    if (tmpMeshChunk.UVsData != 0)
                    {
                        tmpUVs = (CryEngine_Core.ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.UVsData];
                    }
                    if (tmpMeshChunk.VertsUVsData != 0)
                    {
                        tmpVertsUVs = (CryEngine_Core.ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.VertsUVsData];
                    }
                    if (tmpMeshChunk.IndicesData != 0)
                    {
                        tmpIndices = (CryEngine_Core.ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.IndicesData];
                    }

                    // tmpGeo is a Geometry object for each meshsubset.  Name will be "Nodechunk name_matID".  Hopefully there is only one matID used per submesh
                    Grendgine_Collada_Geometry tmpGeo = new Grendgine_Collada_Geometry();
                    tmpGeo.Name = nodeChunk.Name;
                    tmpGeo.ID = nodeChunk.Name;
                    Grendgine_Collada_Mesh tmpMesh = new Grendgine_Collada_Mesh();
                    tmpGeo.Mesh = tmpMesh;
                    Grendgine_Collada_Source[] source = new Grendgine_Collada_Source[3];
                    // need a collada_source for position, normal, UV and color, what the source is (verts), and the tri index
                    Grendgine_Collada_Source posSource = new Grendgine_Collada_Source();
                    Grendgine_Collada_Source normSource = new Grendgine_Collada_Source();
                    Grendgine_Collada_Source uvSource = new Grendgine_Collada_Source();
                    source[0] = posSource;
                    source[1] = normSource;
                    source[2] = uvSource;
                    Grendgine_Collada_Vertices verts = new Grendgine_Collada_Vertices();
                    Grendgine_Collada_Triangles[] tri = new Grendgine_Collada_Triangles[tmpMeshSubsets.NumMeshSubset];

                    posSource.ID = nodeChunk.Name + "_pos";
                    posSource.Name = nodeChunk.Name + "_pos";
                    normSource.ID = nodeChunk.Name + "_norm";
                    normSource.Name = nodeChunk.Name + "_norm";
                    uvSource.Name = nodeChunk.Name + "_UV";
                    uvSource.ID = nodeChunk.Name + "_UV";
                    verts.ID = nodeChunk.Name + "_verts";
                    //tris.Material = meshSubset.MatID.ToString();
                    // Create a float_array object to store all the data
                    Grendgine_Collada_Float_Array floatArrayVerts = new Grendgine_Collada_Float_Array();
                    Grendgine_Collada_Float_Array floatArrayNormals = new Grendgine_Collada_Float_Array();
                    Grendgine_Collada_Float_Array floatArrayUVs = new Grendgine_Collada_Float_Array();

                    floatArrayVerts.ID = posSource.Name + "_array";
                    floatArrayVerts.Count = (int)tmpVertices.NumElements * 3;
                    floatArrayNormals.ID = normSource.Name + "_array";
                    floatArrayNormals.Count = (int)tmpNormals.NumElements * 3;
                    floatArrayUVs.ID = uvSource.Name + "_array";
                    floatArrayUVs.Count = (int)tmpUVs.NumElements * 3 / 2;

                    // Build the string of vertices with a stringbuilder
                    StringBuilder vertString = new StringBuilder();
                    StringBuilder normString = new StringBuilder();
                    StringBuilder uvString = new StringBuilder();
                    // Vertices
                    for (uint j = 0; j < tmpMeshChunk.NumVertices; j++)
                    //uint j = meshSubset.FirstVertex; j < meshSubset.NumVertices + meshSubset.FirstVertex; j++
                    {
                        // Rotate/translate the vertex
                        Vector3 vertex = nodeChunk.GetTransform(tmpVertices.Vertices[j]);
                        vertString.AppendFormat("{0:F7} {1:F7} {2:F7} ", vertex.x, vertex.y, vertex.z);
                        Vector3 normal = tmpNormals.Normals[j];
                        normString.AppendFormat("{0:F7} {1:F7} {2:F7} ", normal.x, normal.y, normal.z);
                    }
                    // Create UV string
                    for (uint j = 0; j < tmpMeshChunk.NumVertices / 3 * 2; j++)
                    {
                        uvString.AppendFormat("{0:F7} {1:F7}", tmpUVs.UVs[j].U, tmpUVs.UVs[j].V);   //1 - tmpUVs.UVs[j].V
                    }

                    // Create the Triangles string
                    // Need to iterate through each of the submeshes to get the faces for each material.
                    Grendgine_Collada_Triangles[] tris = new Grendgine_Collada_Triangles[tmpMeshSubsets.NumMeshSubset]; // one tri for each matID
                    for (uint j = 0; j < tmpMeshSubsets.NumMeshSubset; j++) // Need to make a new Triangle entry for each submesh.
                    {
                        /// <summary> Write the triangles for each geometry.mesh object.  There can be multiple triangles for each mesh.</summary>
                        /// 
                        //tmpMeshSubsets.MeshSubsets[j].WriteMeshSubset();
                        tris[j] = new Grendgine_Collada_Triangles();
                        tris[j].Material = tmpMeshSubsets.MeshSubsets[j].MatID.ToString();
                        tris[j].Count = (int)tmpMeshSubsets.MeshSubsets[j].NumIndices;
                        Grendgine_Collada_Input_Shared[] triInput = new Grendgine_Collada_Input_Shared[tmpMeshSubsets.NumMeshSubset];
                        triInput[j] = new Grendgine_Collada_Input_Shared();
                        triInput[j].source = "#" + posSource.ID;
                        triInput[j].Semantic = Grendgine_Collada_Input_Semantic.VERTEX;
                        tris[j].Input = triInput;
                        StringBuilder p = new StringBuilder();
                        for (uint k = tmpMeshSubsets.MeshSubsets[j].FirstIndex; k < tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices; k++)
                        {
                            //Console.Write("{0} ", tmpIndices.Indices[k]);
                            p.AppendFormat("{0} ", tmpIndices.Indices[k]);
                        }
                        //Utils.Log(LogLevelEnum.Debug, "Indices {0}", p);
                        tris[j].Input = triInput;
                        tris[j].P = new Grendgine_Collada_Int_Array_String();
                        tris[j].P.Value_As_String = p.ToString();
                        triList.Add(tris[j]);
                    }
                    tris = triList.ToArray();

                    // get the 3 inputs for verts
                    Grendgine_Collada_Input_Unshared[] inputshared = new Grendgine_Collada_Input_Unshared[3];
                    Grendgine_Collada_Input_Unshared posInput = new Grendgine_Collada_Input_Unshared();
                    Grendgine_Collada_Input_Unshared normInput = new Grendgine_Collada_Input_Unshared();
                    Grendgine_Collada_Input_Unshared uvInput = new Grendgine_Collada_Input_Unshared();

                    posInput.Semantic = Grendgine_Collada_Input_Semantic.POSITION;
                    normInput.Semantic = Grendgine_Collada_Input_Semantic.NORMAL;
                    uvInput.Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD;   // might need to replace UV with UV
                    posInput.source = "#" + posSource.ID;
                    normInput.source = "#" + normSource.ID;
                    uvInput.source = "#" + uvSource.ID;

                    verts.Input = inputshared;
                    floatArrayVerts.Value_As_String = vertString.ToString();
                    floatArrayNormals.Value_As_String = normString.ToString();
                    floatArrayUVs.Value_As_String = uvString.ToString();

                    inputshared[0] = posInput;
                    inputshared[1] = normInput;
                    inputshared[2] = uvInput;
                    source[0].Float_Array = floatArrayVerts;
                    source[1].Float_Array = floatArrayNormals;
                    source[2].Float_Array = floatArrayUVs;
                    tmpGeo.Mesh.Source = source;
                    tmpGeo.Mesh.Vertices = verts;
                    tmpGeo.Mesh.Triangles = tris;

                    // create the technique_common for each of these
                    posSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source();
                    posSource.Technique_Common.Accessor = new Grendgine_Collada_Accessor();
                    posSource.Technique_Common.Accessor.Source = "#" + floatArrayVerts.ID;
                    posSource.Technique_Common.Accessor.Stride = 3;
                    posSource.Technique_Common.Accessor.Count = tmpMeshChunk.NumVertices;
                    Grendgine_Collada_Param[] paramPos = new Grendgine_Collada_Param[3];
                    paramPos[0] = new Grendgine_Collada_Param();
                    paramPos[1] = new Grendgine_Collada_Param();
                    paramPos[2] = new Grendgine_Collada_Param();
                    paramPos[0].Name = "X";
                    paramPos[0].Type = "float";
                    paramPos[1].Name = "Y";
                    paramPos[1].Type = "float";
                    paramPos[2].Name = "Z";
                    paramPos[2].Type = "float";
                    posSource.Technique_Common.Accessor.Param = paramPos;
                    normSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source();
                    normSource.Technique_Common.Accessor = new Grendgine_Collada_Accessor();
                    normSource.Technique_Common.Accessor.Source = "#" + floatArrayNormals.ID;
                    normSource.Technique_Common.Accessor.Stride = 3;
                    normSource.Technique_Common.Accessor.Count = tmpMeshChunk.NumVertices;
                    Grendgine_Collada_Param[] paramNorm = new Grendgine_Collada_Param[3];
                    paramNorm[0] = new Grendgine_Collada_Param();
                    paramNorm[1] = new Grendgine_Collada_Param();
                    paramNorm[2] = new Grendgine_Collada_Param();
                    paramNorm[0].Name = "X";
                    paramNorm[0].Type = "float";
                    paramNorm[1].Name = "Y";
                    paramNorm[1].Type = "float";
                    paramNorm[2].Name = "Z";
                    paramNorm[2].Type = "float";
                    normSource.Technique_Common.Accessor.Param = paramNorm;
                    uvSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source();
                    uvSource.Technique_Common.Accessor = new Grendgine_Collada_Accessor();
                    uvSource.Technique_Common.Accessor.Source = "#" + floatArrayUVs.ID;
                    uvSource.Technique_Common.Accessor.Stride = 2;
                    uvSource.Technique_Common.Accessor.Count = tmpMeshChunk.NumVertices * 3 / 2;
                    Grendgine_Collada_Param[] paramUV = new Grendgine_Collada_Param[2];
                    paramUV[0] = new Grendgine_Collada_Param();
                    paramUV[1] = new Grendgine_Collada_Param();
                    paramUV[0].Name = "S";
                    paramUV[0].Type = "float";
                    paramUV[1].Name = "T";
                    paramUV[1].Type = "float";
                    uvSource.Technique_Common.Accessor.Param = paramUV;
                    // tris are easy.  Just the index of faces
                    geometryList.Add(tmpGeo);


                }
                else if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Helper)
                {

                }
                else if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Controller)
                {

                }
                //tmpGeo.Mesh = tmpMesh;

                // Add the tmpGeo geometry to the list




            }
            libraryGeometries.Geometry = geometryList.ToArray();
            daeObject.Library_Geometries = libraryGeometries;

        }
        public void WriteLibrary_Controllers()
        {

        }
        public void WriteLibrary_VisualScenes()
        {
            /// <summary> Provides a library in which to place visual_scene elements. </summary>
            /// 
            // Set up the library
            Grendgine_Collada_Library_Visual_Scenes libraryVisualScenes = new Grendgine_Collada_Library_Visual_Scenes();
            // There can be multiple visual scenes.  Will just have one (World) for now.  All node chunks go under Nodes for that visual scene
            List<Grendgine_Collada_Visual_Scene> visualScenes = new List<Grendgine_Collada_Visual_Scene>();
            Grendgine_Collada_Visual_Scene visualScene = new Grendgine_Collada_Visual_Scene();
            List<Grendgine_Collada_Node> nodes = new List<Grendgine_Collada_Node>();

            foreach (CryEngine_Core.ChunkNode nodeChunk in this.CryData.Chunks.Where(a => a.ChunkType == ChunkTypeEnum.Node))
            {
                // Chunks we will need for this node chunk
                if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Mesh)
                {
                    CryEngine_Core.ChunkMesh tmpMeshChunk = (CryEngine_Core.ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                    CryEngine_Core.ChunkMeshSubsets tmpMeshSubsets = (CryEngine_Core.ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node

                    // For each nodechunk with a non helper or controller object ID, create a node and add it to nodes list
                    // now make a list of Nodes that correspond with each node chunk.

                    Grendgine_Collada_Node tmpNode = new Grendgine_Collada_Node();
                    tmpNode.Name = nodeChunk.Name;
                    // Get the mesh chunk
                    CryEngine_Core.ChunkMesh tmpMesh = nodeChunk.ObjectChunk as CryEngine_Core.ChunkMesh;
                    Grendgine_Collada_Node_Type nodeType = new Grendgine_Collada_Node_Type();
                    nodeType = Grendgine_Collada_Node_Type.NODE;
                    tmpNode.Type = nodeType;
                    List<Grendgine_Collada_Matrix> matrices = new List<Grendgine_Collada_Matrix>();
                    Grendgine_Collada_Matrix matrix = new Grendgine_Collada_Matrix();
                    StringBuilder matrixString = new StringBuilder();
                    matrixString.AppendFormat("{0:F7} {1:F7} {2:F7} {3:F7} {4:F7} {5:F7} {6:F7} {7:F7} {8:F7} {9:F7} {10:F7} {11:F7} {12:F7} {13:F7} {14:F7} {15:F7}",
                        nodeChunk.Transform.m11, nodeChunk.Transform.m12, nodeChunk.Transform.m13, nodeChunk.Transform.m14,
                        nodeChunk.Transform.m21, nodeChunk.Transform.m22, nodeChunk.Transform.m23, nodeChunk.Transform.m24,
                        nodeChunk.Transform.m31, nodeChunk.Transform.m32, nodeChunk.Transform.m33, nodeChunk.Transform.m34,
                        nodeChunk.Transform.m41 / 100, nodeChunk.Transform.m42 / 100, nodeChunk.Transform.m43 / 100, nodeChunk.Transform.m44);
                    matrix.Value_As_String = matrixString.ToString();
                    matrices.Add(matrix);
                    tmpNode.Matrix = matrices.ToArray();
                    // Make a set of instance_geometry objects.  One for each submesh, and assign the material to it.
                    List<Grendgine_Collada_Instance_Geometry> instanceGeometries = new List<Grendgine_Collada_Instance_Geometry>();
                    // rethink this.  Might just want one instance geometry per node.
                    /*for (int i = 0; i < tmpMeshSubsets.NumMeshSubset; i++)
                    {
                        // For each mesh subset, we want to create an instance geometry and add it to instanceGeometries list.
                        Console.WriteLine("{0}", i);
                        Grendgine_Collada_Instance_Geometry instanceGeometry = new Grendgine_Collada_Instance_Geometry();
                        instanceGeometry.URL = "#" + this.CryData.RootNode.Name;
                        instanceGeometries.Add(instanceGeometry);
                        
                    }*/
                    tmpNode.Instance_Geometry = instanceGeometries.ToArray();
                    nodes.Add(tmpNode);
                }


            }
            visualScene.Node = nodes.ToArray();
            visualScene.Name = "world";
            visualScenes.Add(visualScene);
            libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
            daeObject.Library_Visual_Scene = libraryVisualScenes;


        }

        public Grendgine_Collada_Node CreateNode(CryEngine_Core.ChunkNode nodeChunk)
        {
            // This will be used recursively to create a node object and return it to WriteLibrary_VisualScenes
            Grendgine_Collada_Node tmpNode = new Grendgine_Collada_Node();
            if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Mesh)
            {
                CryEngine_Core.ChunkMesh tmpMeshChunk = (CryEngine_Core.ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                CryEngine_Core.ChunkMeshSubsets tmpMeshSubsets = (CryEngine_Core.ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node
                Grendgine_Collada_Node_Type nodeType = new Grendgine_Collada_Node_Type();
                nodeType = Grendgine_Collada_Node_Type.NODE;
                tmpNode.Type = nodeType;
                List<Grendgine_Collada_Matrix> matrices = new List<Grendgine_Collada_Matrix>();
                Grendgine_Collada_Matrix matrix = new Grendgine_Collada_Matrix();
                StringBuilder matrixString = new StringBuilder();
                matrixString.AppendFormat("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15}",
                    nodeChunk.Transform.m11, nodeChunk.Transform.m12, nodeChunk.Transform.m13, nodeChunk.Transform.m14,
                    nodeChunk.Transform.m21, nodeChunk.Transform.m22, nodeChunk.Transform.m23, nodeChunk.Transform.m24,
                    nodeChunk.Transform.m31, nodeChunk.Transform.m32, nodeChunk.Transform.m33, nodeChunk.Transform.m34,
                    nodeChunk.Transform.m41/100, nodeChunk.Transform.m42/100, nodeChunk.Transform.m43/100, nodeChunk.Transform.m44);
                matrix.Value_As_String = matrixString.ToString();
                matrix.sID = "transform";
                matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
                tmpNode.Matrix = matrices.ToArray();
                // Make a set of instance_geometry objects.  One for each node, and add an instance_material for each material bind
                Grendgine_Collada_Instance_Geometry instanceGeometry = new Grendgine_Collada_Instance_Geometry();
                Grendgine_Collada_Instance_Geometry[] instanceGeometries = new Grendgine_Collada_Instance_Geometry[1];
                instanceGeometry.Name = nodeChunk.Name;
                instanceGeometry.URL = "#" + nodeChunk.Name;  // this is the ID of the geometry.
                Grendgine_Collada_Bind_Material[] bindMaterials = new Grendgine_Collada_Bind_Material[1];
                Grendgine_Collada_Bind_Material bindMaterial = new Grendgine_Collada_Bind_Material();
                instanceGeometry.Bind_Material[0] = bindMaterial;
                // This gets complicated.  We need to make one instance_material for each material used in this node chunk.  The mat IDs used in this
                // node chunk are stored in meshsubsets, so for each subset we need to grab the mat, get the target (id), and make an instance_material for it.
                Grendgine_Collada_Instance_Material_Geometry[] instanceMats = new Grendgine_Collada_Instance_Material_Geometry[1];

                /*for (int i = 0; i < tmpMeshSubsets.NumMeshSubset; i++)
                {
                    // For each mesh subset, we want to create an instance geometry and add it to instanceGeometries list.
                    Console.WriteLine("{0}", i);
                    Grendgine_Collada_Instance_Geometry instanceGeometry = new Grendgine_Collada_Instance_Geometry();
                    instanceGeometry.URL = "#" + this.CryData.RootNode.Name;
                    instanceGeometries.Add(instanceGeometry);

                }*/
                instanceGeometries[0] = instanceGeometry;
                tmpNode.Instance_Geometry = instanceGeometries;

            }
            return tmpNode;
        }
        public void WriteScene()
        {
            Grendgine_Collada_Scene scene = new Grendgine_Collada_Scene();
            Grendgine_Collada_Instance_Visual_Scene visualScene = new Grendgine_Collada_Instance_Visual_Scene();
            visualScene.URL = "#world";
            scene.Visual_Scene = visualScene;
            daeObject.Scene = scene;

        }

    }
}