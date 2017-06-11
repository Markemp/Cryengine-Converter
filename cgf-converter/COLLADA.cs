using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using grendgine_collada; 
using System.Reflection;

namespace CgfConverter
{
    public class COLLADA : BaseRenderer // class to export to .dae format (COLLADA)
    {
        public XmlSchema schema = new XmlSchema();
        public FileInfo daeOutputFile;

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
            GetSchema();                                                    // Loads the schema.  Needs error checking in case it's offline.
            WriteRootNode();
            WriteAsset();
            WriteLibrary_Images();
            WriteScene();
            WriteLibrary_Effects();
            WriteLibrary_Materials();
            WriteLibrary_Geometries();
            WriteLibrary_VisualScenes();
            WriteIDs();
            if (!daeOutputFile.Directory.Exists)
                daeOutputFile.Directory.Create();
            TextWriter writer = new StreamWriter(daeOutputFile.FullName);   // Makes the Textwriter object for the output
            mySerializer.Serialize(writer, daeObject);                      // Serializes the daeObject and writes to the writer
            // Validate that the Collada document is ok
            writer.Close();
            //ValidateXml();                                                  // validates against the schema
            //ValidateDoc();                                                // validates IDs and URLs
            Utils.Log(LogLevelEnum.Debug, "End of Write Collada");
        }

        private void WriteRootNode()
        {
            //daeObject.Collada_Version = "1.5.0";  // Blender doesn't like 1.5. :(
            daeObject.Collada_Version = "1.4.1";
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
            //Console.WriteLine("Number of images {0}", CryData.Materials.Length);
            // We now have the image library set up.  start to populate.
            //foreach (CryEngine_Core.Material material in CryData.Materials)
            for (int k=0; k < CryData.Materials.Length; k++)
            {
                // each mat will have a number of texture files.  Need to create an <image> for each of them.
                //int numTextures = material.Textures.Length;
                int numTextures = CryData.Materials[k].Textures.Length;
                for (int i = 0; i < numTextures; i++)
                {
                    // For each texture in the material, we make a new <image> object and add it to the list. 
                    Grendgine_Collada_Image tmpImage = new Grendgine_Collada_Image();
                    tmpImage.ID = CryData.Materials[k].Name + "_" + CryData.Materials[k].Textures[i].Map;
                    tmpImage.Name = CryData.Materials[k].Name + "_" + CryData.Materials[k].Textures[i].Map;
                    tmpImage.Init_From = new Grendgine_Collada_Init_From();
                    // Build the URI path to the file as a .dds, clean up the slashes.
                    StringBuilder builder;
                    if (CryData.Materials[k].Textures[i].File.Contains(@"/") || CryData.Materials[k].Textures[i].File.Contains(@"\"))
                    {
                        // if Datadir is empty, need a clean name and can only search in the current directory.  If Datadir is provided, then look there.
                        if (this.Args.DataDir == null)
                        {
                            builder = new StringBuilder(CleanName(CryData.Materials[k].Textures[i].File) + ".dds");
                            if (Args.TiffTextures)
                                builder.Replace(".dds", ".tif");
                            
                        } else
                        {
                            builder = new StringBuilder(@"/" + this.Args.DataDir.Replace(@"\", @"/").Replace(" ", @"%20") + @"/" + CryData.Materials[k].Textures[i].File);
                        }
                    }
                    else
                    {
                        builder = new StringBuilder(CryData.Materials[k].Textures[i].File);
                    }

                    if (!this.Args.TiffTextures)
                        builder.Replace(".tif", ".dds");
                    else
                        builder.Replace(".dds", ".tif");

                    // if 1.4.1, use URI.  If 1.5.0, use Ref.
                    if (daeObject.Collada_Version == "1.4.1")
                    {
                        tmpImage.Init_From.Uri = builder.ToString();
                    } else
                    {
                        tmpImage.Init_From.Ref = builder.ToString();
                    }
                    imageList.Add(tmpImage);
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
                tmpMaterial.Instance_Effect = new Grendgine_Collada_Instance_Effect();
                // Name is blank if it's a material file with no submats.  Set to file name.
                // Need material ID here, so the meshes can reference it.  Use the chunk ID.
                if (CryData.Materials[i].Name == null)
                {
                    tmpMaterial.Name = CryData.RootNode.Name;       
                    tmpMaterial.ID = CryData.RootNode.Name;
                    tmpMaterial.Instance_Effect.URL = "#" + CryData.RootNode.Name + "-effect";
                } else
                {
                    tmpMaterial.Name = CryData.Materials[i].Name;
                    tmpMaterial.ID = CryData.Materials[i].Name + "-material";          // this is the order the materials appear in the .mtl file.  Needed for geometries.
                    tmpMaterial.Instance_Effect.URL = "#" + CryData.Materials[i].Name + "-effect";
                }
                // The # in front of tmpMaterial.name is needed to reference the effect in Library_effects.
                
                materials[i] = tmpMaterial;
            }
            libraryMaterials.Material = materials;
        }
        public void WriteLibrary_Effects()
        {
            // The Effects library.  This is actual material stuff, so... let's get into it!  First, let's make a library effects object
            Grendgine_Collada_Library_Effects libraryEffects = new Grendgine_Collada_Library_Effects();
            // Like materials.  We will need one effect for each material.
            int numEffects = CryData.Materials.Length;
            Grendgine_Collada_Effect[] effects = new Grendgine_Collada_Effect[numEffects];
            for (int i=0; i<numEffects; i++)
            {
                Grendgine_Collada_Effect tmpEffect = new Grendgine_Collada_Effect();
                //tmpEffect.Name = CryData.Materials[i].Name;
                tmpEffect.ID = CryData.Materials[i].Name + "-effect";
                tmpEffect.Name = CryData.Materials[i].Name;
                effects[i] = tmpEffect;

                // create the profile_common for the effect
                List<Grendgine_Collada_Profile_COMMON> profiles = new List<Grendgine_Collada_Profile_COMMON>();
                Grendgine_Collada_Profile_COMMON profile = new Grendgine_Collada_Profile_COMMON();
                profiles.Add(profile);

                // Create a list for the new_params
                List<Grendgine_Collada_New_Param> newparams = new List<Grendgine_Collada_New_Param>();
                #region set up the sampler and surface for the materials. 
                // Check to see if the texture exists, and if so make a sampler and surface.
                int numTextures = CryData.Materials[i].Textures.Length;
                for (int j = 0; j < CryData.Materials[i].Textures.Length; j++)
                {
                    // Add the Surface node
                    Grendgine_Collada_New_Param texSurface = new Grendgine_Collada_New_Param();
                    texSurface.sID = CleanName(CryData.Materials[i].Textures[j].File) + "-surface";
                    Grendgine_Collada_Surface surface = new Grendgine_Collada_Surface();
                    texSurface.Surface = surface;
                    surface.Init_From = new Grendgine_Collada_Init_From();
                    Grendgine_Collada_Surface surface2D = new Grendgine_Collada_Surface();
                    //texSurface.sID = CleanName(CryData.Materials[i].Textures[j].File) + CryData.Materials[i].Textures[j].Map + "-surface";
                    texSurface.Surface.Type = "2D";
                    texSurface.Surface.Init_From = new Grendgine_Collada_Init_From();
                    //texSurface.Surface.Init_From.Uri = CleanName(texture.File);
                    texSurface.Surface.Init_From.Uri = CryData.Materials[i].Name + "_" + CryData.Materials[i].Textures[j].Map;

                    // Add the Sampler node
                    Grendgine_Collada_New_Param texSampler = new Grendgine_Collada_New_Param();
                    texSampler.sID = CleanName(CryData.Materials[i].Textures[j].File) + "-sampler";
                    Grendgine_Collada_Sampler2D sampler2D = new Grendgine_Collada_Sampler2D();
                    texSampler.Sampler2D = sampler2D;
                    Grendgine_Collada_Source samplerSource = new Grendgine_Collada_Source();
                    texSampler.Sampler2D.Source = texSurface.sID;

                    newparams.Add(texSurface);
                    newparams.Add(texSampler);
                }
                #endregion

                #region Create the Technique
                // Make the techniques for the profile
                Grendgine_Collada_Effect_Technique_COMMON technique = new Grendgine_Collada_Effect_Technique_COMMON();
                Grendgine_Collada_Phong phong = new Grendgine_Collada_Phong();
                technique.Phong = phong;
                technique.sID = "common";
                profile.Technique = technique;

                phong.Diffuse = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type();
                phong.Specular = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type();

                // Add all the emissive, etc features to the phong
                // Need to check if a texture exists.  If so, refer to the sampler.  Should be a <Texture Map="Diffuse" line if there is a map.
                bool diffound = false;
                bool specfound = false;

                foreach (var texture in CryData.Materials[i].Textures)
                //for (int j=0; j < CryData.Materials[i].Textures.Length; j++)
                {
                    //Console.WriteLine("Processing material texture {0}", CleanName(texture.File));
                    if (texture.Map == CryEngine_Core.Material.Texture.MapTypeEnum.Diffuse)
                    {
                        //Console.WriteLine("Found Diffuse Map");
                        diffound = true;
                        phong.Diffuse.Texture = new Grendgine_Collada_Texture();
                        // Texcoord is the ID of the UV source in geometries.  Not needed.
                        phong.Diffuse.Texture.Texture = CleanName(texture.File) + "-sampler";
                    }
                    if (texture.Map == CryEngine_Core.Material.Texture.MapTypeEnum.Specular)
                    {
                        //Console.WriteLine("Found spec map");
                        specfound = true;
                        phong.Specular.Texture = new Grendgine_Collada_Texture();
                        phong.Specular.Texture.Texture = CleanName(texture.File) + "-sampler";
                    }
                    if (texture.Map == CryEngine_Core.Material.Texture.MapTypeEnum.Bumpmap)
                    {
                        // Bump maps go in an extra node.
                        // bump maps are added to an extra node.
                        Grendgine_Collada_Extra[] extras = new Grendgine_Collada_Extra[1];
                        Grendgine_Collada_Extra extra = new Grendgine_Collada_Extra();
                        extras[0] = extra;

                        technique.Extra = extras;
                        
                        // Create the technique for the extra
                        
                        Grendgine_Collada_Technique[] extraTechniques = new Grendgine_Collada_Technique[1];
                        Grendgine_Collada_Technique extraTechnique = new Grendgine_Collada_Technique();
                        extra.Technique = extraTechniques;
                        //extraTechnique.Data[0] = new XmlElement();

                        extraTechniques[0] = extraTechnique;
                        extraTechnique.profile = "FCOLLADA";
                        
                        Grendgine_Collada_BumpMap bumpMap = new Grendgine_Collada_BumpMap();
                        bumpMap.Textures = new Grendgine_Collada_Texture[1];
                        bumpMap.Textures[0] = new Grendgine_Collada_Texture();
                        bumpMap.Textures[0].Texture = CleanName(texture.File) + "-sampler";
                        extraTechnique.Data = new XmlElement[1];
                        extraTechnique.Data[0] = bumpMap;
                    }
                }
                if (diffound == false)
                {
                    phong.Diffuse.Color = new Grendgine_Collada_Color();
                    phong.Diffuse.Color.Value_As_String = CryData.Materials[i].__Diffuse.Replace(",", " ");
                    phong.Diffuse.Color.sID = "diffuse";
                }
                if (specfound == false)
                {
                    //Console.WriteLine("No spec found, using color");
                    phong.Specular.Color = new Grendgine_Collada_Color();
                    phong.Specular.Color.sID = "specular";
                    phong.Specular.Color.Value_As_String = CryData.Materials[i].__Specular.Replace(",", " ");
                }

                phong.Emission = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type();
                phong.Emission.Color = new Grendgine_Collada_Color();
                phong.Emission.Color.sID = "emission";
                phong.Emission.Color.Value_As_String = CryData.Materials[i].__Emissive.Replace(","," ");
                phong.Shininess = new Grendgine_Collada_FX_Common_Float_Or_Param_Type();
                phong.Shininess.Float = new Grendgine_Collada_SID_Float();
                phong.Shininess.Float.sID = "shininess";
                phong.Shininess.Float.Value = (float)CryData.Materials[i].Shininess;
                phong.Index_Of_Refraction = new Grendgine_Collada_FX_Common_Float_Or_Param_Type();
                phong.Index_Of_Refraction.Float = new Grendgine_Collada_SID_Float();

                phong.Transparent = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type();
                phong.Transparent.Color = new Grendgine_Collada_Color();
                phong.Transparent.Opaque = new Grendgine_Collada_FX_Opaque_Channel();
                phong.Transparent.Color.Value_As_String = (1 - CryData.Materials[i].Opacity).ToString();  // Subtract from 1 for proper value.

                #endregion



                tmpEffect.Profile_COMMON = profiles.ToArray();
                profile.New_Param = new Grendgine_Collada_New_Param[newparams.Count];
                profile.New_Param = newparams.ToArray();

                
            }
            libraryEffects.Effect = effects;
            daeObject.Library_Effects = libraryEffects;
            // libraryEffects contains a number of effects objects.  One effects object for each material.

        }
        public void WriteLibrary_Geometries()
        {
            // Geometry library.  this is going to be fun...
            Grendgine_Collada_Library_Geometries libraryGeometries = new Grendgine_Collada_Library_Geometries();
            //libraryGeometries.ID = this.CryData.RootNode.Name+"_root";
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
                //List<Grendgine_Collada_Triangles> triList = new List<Grendgine_Collada_Triangles>();
                List<Grendgine_Collada_Polylist> polylistList = new List<Grendgine_Collada_Polylist>();
                CryEngine_Core.ChunkDataStream tmpNormals = null;
                CryEngine_Core.ChunkDataStream tmpUVs = null;
                CryEngine_Core.ChunkDataStream tmpVertices = null;
                CryEngine_Core.ChunkDataStream tmpVertsUVs = null;
                CryEngine_Core.ChunkDataStream tmpIndices = null;
                //Console.WriteLine("Writing node chunk ID {0:X}", nodeChunk.ID);
                //nodeChunk.WriteChunk();

                if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Mesh)
                {
                    // Get the mesh chunk and submesh chunk for this node.
                    CryEngine_Core.ChunkMesh tmpMeshChunk = (CryEngine_Core.ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                    // Check to see if the Mesh points to a PhysicsData mesh.  Don't want to write these.
                    if (tmpMeshChunk.MeshPhysicsData != 0)
                    {
                        // TODO:  Implement this chunk
                    } else
                    {
                        //Console.WriteLine("tmpMeshChunk ID is {0:X}", nodeChunk.ObjectNodeID);
                        // tmpMeshChunk.WriteChunk();
                        Console.WriteLine("tmpmeshsubset ID is {0:X}", tmpMeshChunk.MeshSubsets);
                        CryEngine_Core.ChunkMeshSubsets tmpMeshSubsets = (CryEngine_Core.ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node

                        if (tmpMeshChunk.MeshSubsets != 0)
                        {
                            tmpMeshSubsets = (CryEngine_Core.ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node
                        }
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
                            //tmpIndices.WriteChunk();
                        }

                        // tmpGeo is a Geometry object for each meshsubset.  Name will be "Nodechunk name_matID".  Hopefully there is only one matID used per submesh
                        Grendgine_Collada_Geometry tmpGeo = new Grendgine_Collada_Geometry();
                        tmpGeo.Name = nodeChunk.Name;
                        tmpGeo.ID = nodeChunk.Name + "-mesh";
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
                        posSource.ID = nodeChunk.Name + "-mesh-pos";
                        posSource.Name = nodeChunk.Name + "-pos";
                        normSource.ID = nodeChunk.Name + "-mesh-norm";
                        normSource.Name = nodeChunk.Name + "-norm";
                        uvSource.Name = nodeChunk.Name + "-UV";
                        uvSource.ID = nodeChunk.Name + "-mesh-UV";

                        #region Create vertices node.  For polylist will just have VERTEX, for triangles it will have 3 entries.
                        Grendgine_Collada_Vertices vertices = new Grendgine_Collada_Vertices();
                        Grendgine_Collada_Input_Unshared[] inputshared = new Grendgine_Collada_Input_Unshared[3];
                        Grendgine_Collada_Input_Unshared posInput = new Grendgine_Collada_Input_Unshared();
                        Grendgine_Collada_Input_Unshared normInput = new Grendgine_Collada_Input_Unshared();
                        Grendgine_Collada_Input_Unshared uvInput = new Grendgine_Collada_Input_Unshared();
                        vertices.ID = nodeChunk.Name + "-vertices";
                        posInput.Semantic = Grendgine_Collada_Input_Semantic.POSITION;
                        normInput.Semantic = Grendgine_Collada_Input_Semantic.NORMAL;
                        uvInput.Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD;   // might need to replace UV with UV

                        posInput.source = "#" + posSource.ID;
                        normInput.source = "#" + normSource.ID;
                        uvInput.source = "#" + uvSource.ID;
                        inputshared[0] = posInput;
                        //inputshared[1] = normInput;
                        //inputshared[2] = uvInput;

                        vertices.Input = inputshared;
                        tmpGeo.Mesh.Vertices = vertices;

                        #endregion

                        //tris.Material = meshSubset.MatID.ToString();

                        // Create a float_array object to store all the data
                        Grendgine_Collada_Float_Array floatArrayVerts = new Grendgine_Collada_Float_Array();
                        Grendgine_Collada_Float_Array floatArrayNormals = new Grendgine_Collada_Float_Array();
                        Grendgine_Collada_Float_Array floatArrayUVs = new Grendgine_Collada_Float_Array();

                        floatArrayVerts.ID = posSource.ID + "-array";
                        floatArrayVerts.Digits = 6;
                        floatArrayVerts.Magnitude = 38;
                        floatArrayVerts.Count = (int)tmpVertices.NumElements * 3;
                        floatArrayNormals.ID = normSource.ID + "-array";
                        floatArrayNormals.Digits = 6;
                        floatArrayNormals.Magnitude = 38;
                        floatArrayNormals.Count = (int)tmpNormals.NumElements * 3;
                        floatArrayUVs.ID = uvSource.ID + "-array";
                        floatArrayUVs.Digits = 6;
                        floatArrayUVs.Magnitude = 38;
                        floatArrayUVs.Count = (int)tmpUVs.NumElements * 2;

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
                        for (uint j = 0; j < tmpUVs.NumElements; j++)
                        {
                            uvString.AppendFormat("{0} {1} ", tmpUVs.UVs[j].U, 1 - tmpUVs.UVs[j].V);
                        }

                        #region Create triangles string - Deprecated
                        /*
                        // Create the Triangles string
                        // Need to iterate through each of the submeshes to get the faces for each material.
                        Grendgine_Collada_Triangles[] tris = new Grendgine_Collada_Triangles[tmpMeshSubsets.NumMeshSubset]; // one tri for each matID
                        for (uint j = 0; j < tmpMeshSubsets.NumMeshSubset; j++) // Need to make a new Triangle entry for each submesh.
                        {
                            /// <summary> Write the triangles for each geometry.mesh object.  There can be multiple triangles for each mesh.</summary>
                            /// 
                            //tmpMeshSubsets.MeshSubsets[j].WriteMeshSubset();
                            tris[j] = new Grendgine_Collada_Triangles();
                            Grendgine_Collada_Input_Shared[] triInput = new Grendgine_Collada_Input_Shared[tmpMeshSubsets.NumMeshSubset];
                            tris[j].Input = triInput;

                            triInput[j] = new Grendgine_Collada_Input_Shared();
                            triInput[j].source = "#" + vertices.ID;
                            triInput[j].Semantic = Grendgine_Collada_Input_Semantic.VERTEX;

                            tris[j].Material = CryData.Materials[tmpMeshSubsets.MeshSubsets[j].MatID].Name + "-material";
                            tris[j].Count = (int)tmpMeshSubsets.MeshSubsets[j].NumIndices/3;
                            StringBuilder p = new StringBuilder();
                            for (uint k = tmpMeshSubsets.MeshSubsets[j].FirstIndex; k < tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices; k++)
                            {
                                //p.AppendFormat("{0} ", tmpIndices.Indices[k]);
                                p.AppendFormat("{0} ", tmpIndices.Indices[k]);
                            }
                            //Utils.Log(LogLevelEnum.Debug, "Indices {0}", p);
                            tris[j].Input = triInput;
                            tris[j].P = new Grendgine_Collada_Int_Array_String();
                            tris[j].P.Value_As_String = p.ToString();
                            // Create an input semantic here to point to the vertices
                            triList.Add(tris[j]);
                        }
                        tris = triList.ToArray();
                        //tmpGeo.Mesh.Triangles = tris;*/
                        #endregion

                        #region Create the polylist node.
                        Grendgine_Collada_Polylist[] polylists = new Grendgine_Collada_Polylist[tmpMeshSubsets.NumMeshSubset];
                        tmpGeo.Mesh.Polylist = polylists;
                        //tmpMeshSubsets.WriteChunk();
                        //Console.WriteLine("{0} materials in Crydata.Materials", CryData.Materials.Length);

                        for (uint j = 0; j < tmpMeshSubsets.NumMeshSubset; j++) // Need to make a new Polylist entry for each submesh.
                        {
                            polylists[j] = new Grendgine_Collada_Polylist();
                            polylists[j].Count = (int)tmpMeshSubsets.MeshSubsets[j].NumIndices / 3;
                            Console.WriteLine("Mat Index is {0}", tmpMeshSubsets.MeshSubsets[j].MatID);
                            polylists[j].Material = CryData.Materials[tmpMeshSubsets.MeshSubsets[j].MatID].Name + "-material";
                            // Create the 3 inputs.  vertex, normal, texcoord
                            polylists[j].Input = new Grendgine_Collada_Input_Shared[3];
                            polylists[j].Input[0] = new Grendgine_Collada_Input_Shared();
                            polylists[j].Input[0].Semantic = new Grendgine_Collada_Input_Semantic();
                            polylists[j].Input[0].Semantic = Grendgine_Collada_Input_Semantic.VERTEX;
                            polylists[j].Input[0].Offset = 0;
                            polylists[j].Input[0].source = "#" + vertices.ID;
                            polylists[j].Input[1] = new Grendgine_Collada_Input_Shared();
                            polylists[j].Input[1].Semantic = Grendgine_Collada_Input_Semantic.NORMAL;
                            polylists[j].Input[1].Offset = 1;
                            polylists[j].Input[1].source = "#" + normSource.ID;
                            polylists[j].Input[2] = new Grendgine_Collada_Input_Shared();
                            polylists[j].Input[2].Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD;
                            polylists[j].Input[2].Offset = 2;
                            polylists[j].Input[2].source = "#" + uvSource.ID;

                            // Create the vcount list.  All triangles, so the subset number of indices.
                            // This will have to get tuned as we get multiple polylists for a submesh
                            StringBuilder vc = new StringBuilder();
                            //Console.WriteLine("Creating vcount for Indices {0} to {1}", tmpMeshSubsets.MeshSubsets[j].FirstIndex, (tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices));
                            for (uint k = tmpMeshSubsets.MeshSubsets[j].FirstIndex; k < (tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices); k++)
                            {
                                vc.AppendFormat("3 ");
                                k += 2;
                            }
                            polylists[j].VCount = new Grendgine_Collada_Int_Array_String();
                            polylists[j].VCount.Value_As_String = vc.ToString();

                            // Create the P node for the Polylist.
                            StringBuilder p = new StringBuilder();
                            for (uint k = tmpMeshSubsets.MeshSubsets[j].FirstIndex; k < (tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices); k++)
                            {
                                p.AppendFormat("{0} {0} {0} {1} {1} {1} {2} {2} {2} ", tmpIndices.Indices[k], tmpIndices.Indices[k + 1], tmpIndices.Indices[k + 2]);
                                k += 2;
                            }
                            polylists[j].P = new Grendgine_Collada_Int_Array_String();
                            polylists[j].P.Value_As_String = p.ToString();

                        }

                        #endregion

                        #region Create the source float_array nodes.  Vertex, normal, UV.  May need color as well.

                        floatArrayVerts.Value_As_String = vertString.ToString();
                        floatArrayNormals.Value_As_String = normString.ToString();
                        floatArrayUVs.Value_As_String = uvString.ToString();

                        source[0].Float_Array = floatArrayVerts;
                        source[1].Float_Array = floatArrayNormals;
                        source[2].Float_Array = floatArrayUVs;
                        tmpGeo.Mesh.Source = source;

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
                        uvSource.Technique_Common.Accessor.Count = tmpUVs.NumElements;
                        Grendgine_Collada_Param[] paramUV = new Grendgine_Collada_Param[2];
                        paramUV[0] = new Grendgine_Collada_Param();
                        paramUV[1] = new Grendgine_Collada_Param();
                        paramUV[0].Name = "S";
                        paramUV[0].Type = "float";
                        paramUV[1].Name = "T";
                        paramUV[1].Type = "float";
                        uvSource.Technique_Common.Accessor.Param = paramUV;
                        geometryList.Add(tmpGeo);
                        #endregion
                    }



                }
                else if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Helper)
                {

                }
                else if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Controller)
                {

                }
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

            Grendgine_Collada_Node rootNode = new Grendgine_Collada_Node();
            rootNode = CreateNode(CryData.RootNode, rootNode);
            nodes.Add(rootNode);

            /*foreach (CryEngine_Core.ChunkNode nodeChunk in this.CryData.Chunks.Where(a => a.ChunkType == ChunkTypeEnum.Node))
            {
                // Chunks we will need for this node chunk
                // For each nodechunk with a non helper or controller object ID, create a node and add it to nodes list
                // now make a list of Nodes that correspond with each node chunk.
                Grendgine_Collada_Node tmpNode = new Grendgine_Collada_Node();
                tmpNode = CreateNode(nodeChunk, null);
                // Get the mesh chunk
                CryEngine_Core.ChunkMesh tmpMesh = nodeChunk.ObjectChunk as CryEngine_Core.ChunkMesh;

                nodes.Add(tmpNode);
            }*/

            visualScene.Node = nodes.ToArray();
            //visualScene.Name = "world";
            visualScene.ID = "Scene";
            visualScenes.Add(visualScene);
            libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
            daeObject.Library_Visual_Scene = libraryVisualScenes;
        }

        public Grendgine_Collada_Node CreateNode(CryEngine_Core.ChunkNode nodeChunk, Grendgine_Collada_Node parentNode)
        {
            // This will be used recursively to create a node object and return it to WriteLibrary_VisualScenes
            #region Create the node element for this nodeChunk
            Grendgine_Collada_Node tmpNode = new Grendgine_Collada_Node();
            if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Mesh)
            {
                CryEngine_Core.ChunkMesh tmpMeshChunk = (CryEngine_Core.ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                if (tmpMeshChunk.MeshPhysicsData != 0)
                {
                    // The mesh points to a meshphysics chunk.  While there is geometry (hitbox, collision?) we don't want to process
                    Console.WriteLine("Node chunk {0} belongs to a mesh physics chunk.  Writing simple node.", nodeChunk.Name);
                    tmpNode = CreateSimpleNode(nodeChunk);
                } else
                {
                    CryEngine_Core.ChunkMeshSubsets tmpMeshSubsets = (CryEngine_Core.ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node
                    Grendgine_Collada_Node_Type nodeType = new Grendgine_Collada_Node_Type();
                    nodeType = Grendgine_Collada_Node_Type.NODE;
                    tmpNode.Type = nodeType;
                    tmpNode.Name = nodeChunk.Name;
                    tmpNode.ID = nodeChunk.Name;
                    // Make the lists necessary for this Node.
                    List<Grendgine_Collada_Matrix> matrices = new List<Grendgine_Collada_Matrix>();
                    List<Grendgine_Collada_Instance_Geometry> instanceGeometries = new List<Grendgine_Collada_Instance_Geometry>();
                    List<Grendgine_Collada_Bind_Material> bindMaterials = new List<Grendgine_Collada_Bind_Material>();
                    List<Grendgine_Collada_Instance_Material_Geometry> instanceMaterials = new List<Grendgine_Collada_Instance_Material_Geometry>();

                    Grendgine_Collada_Matrix matrix = new Grendgine_Collada_Matrix();
                    StringBuilder matrixString = new StringBuilder();

                    // matrixString might have to be an identity matrix, since GetTransform is applying the transform to all the vertices.
                    /*matrixString.AppendFormat("{0:F7} {1:F7} {2:F7} {3:F7} {4:F7} {5:F7} {6:F7} {7:F7} {8:F7} {9:F7} {10:F7} {11:F7} {12:F7} {13:F7} {14:F7} {15:F7}",
                        nodeChunk.Transform.m11, nodeChunk.Transform.m12, nodeChunk.Transform.m13, nodeChunk.Transform.m14,
                        nodeChunk.Transform.m21, nodeChunk.Transform.m22, nodeChunk.Transform.m23, nodeChunk.Transform.m24,
                        nodeChunk.Transform.m31, nodeChunk.Transform.m32, nodeChunk.Transform.m33, nodeChunk.Transform.m34,
                        nodeChunk.Transform.m41 / 100, nodeChunk.Transform.m42 / 100, nodeChunk.Transform.m43 / 100, nodeChunk.Transform.m44);*/
                    matrixString.AppendFormat("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 0");
                    matrix.Value_As_String = matrixString.ToString();
                    matrix.sID = "transform";
                    matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
                    tmpNode.Matrix = matrices.ToArray();

                    // Each node will have one instance geometry, although it could be a list.
                    Grendgine_Collada_Instance_Geometry instanceGeometry = new Grendgine_Collada_Instance_Geometry();
                    instanceGeometry.Name = nodeChunk.Name;
                    instanceGeometry.URL = "#" + nodeChunk.Name + "-mesh";  // this is the ID of the geometry.

                    Grendgine_Collada_Bind_Material bindMaterial = new Grendgine_Collada_Bind_Material();
                    bindMaterial.Technique_Common = new Grendgine_Collada_Technique_Common_Bind_Material();
                    bindMaterial.Technique_Common.Instance_Material = new Grendgine_Collada_Instance_Material_Geometry[tmpMeshSubsets.NumMeshSubset];
                    bindMaterials.Add(bindMaterial);
                    instanceGeometry.Bind_Material = bindMaterials.ToArray();
                    instanceGeometries.Add(instanceGeometry);

                    tmpNode.Instance_Geometry = instanceGeometries.ToArray();

                    // This gets complicated.  We need to make one instance_material for each material used in this node chunk.  The mat IDs used in this
                    // node chunk are stored in meshsubsets, so for each subset we need to grab the mat, get the target (id), and make an instance_material for it.
                    //Grendgine_Collada_Instance_Material_Geometry instanceMats = new Grendgine_Collada_Instance_Material_Geometry();

                    for (int i = 0; i < tmpMeshSubsets.NumMeshSubset; i++)
                    {
                        // For each mesh subset, we want to create an instance material and add it to instanceMaterials list.
                        Grendgine_Collada_Instance_Material_Geometry tmpInstanceMat = new Grendgine_Collada_Instance_Material_Geometry();
                        //tmpInstanceMat.Target = "#" + tmpMeshSubsets.MeshSubsets[i].MatID;
                        tmpInstanceMat.Target = "#" + CryData.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name + "-material";
                        //tmpInstanceMat.Symbol = CryData.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name;
                        tmpInstanceMat.Symbol = CryData.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name + "-material";
                        instanceMaterials.Add(tmpInstanceMat);
                    }
                    tmpNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material = instanceMaterials.ToArray();
                }

            }
            else if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Helper) // Simple node here
            {
                //Console.WriteLine("Node chunk {0} belongs to a helpers chunk.  Writing simple node.", nodeChunk.Name);
                tmpNode = CreateSimpleNode(nodeChunk);
            }
            #endregion

            // Recursively call this for each of the children. 
            // We need to make an array of node elements of length numChildren to hold each of child nodes
            //Console.WriteLine("Processing Node Chunk {0}.  Number of children: {1}", nodeChunk.Name, nodeChunk.__NumChildren);
            Grendgine_Collada_Node[] childNodes = new Grendgine_Collada_Node[nodeChunk.__NumChildren];
            int counter = 0;
            foreach (CryEngine_Core.ChunkNode childNodeChunk in this.CryData.Chunks.Where(a => a.ChunkType == ChunkTypeEnum.Node))
            {
                //Console.WriteLine("Found a node chunk ID {0:X}.  Parent ID is {1:X}", childNodeChunk.ID, childNodeChunk.ParentNodeID);
                if (childNodeChunk.ParentNodeID == nodeChunk.ID )
                {
                    //Console.WriteLine("Found the parent node chuck of node chunk {0}", childNodeChunk.Name);
                    Grendgine_Collada_Node childNode = new Grendgine_Collada_Node();
                    
                    childNode = CreateNode(childNodeChunk, tmpNode);
                    childNodes[counter] = childNode;
                    counter++;
                }
            }
            tmpNode.node = childNodes;
            return tmpNode;
        }
        public void WriteScene()
        {
            Grendgine_Collada_Scene scene = new Grendgine_Collada_Scene();
            Grendgine_Collada_Instance_Visual_Scene visualScene = new Grendgine_Collada_Instance_Visual_Scene();
            visualScene.URL = "#Scene";
            visualScene.Name = "Scene";
            scene.Visual_Scene = visualScene;
            daeObject.Scene = scene;

        }

        public Grendgine_Collada_Node CreateSimpleNode(CryEngine_Core.ChunkNode nodeChunk)
        {
            // This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics
            Grendgine_Collada_Node_Type nodeType = new Grendgine_Collada_Node_Type();
            Grendgine_Collada_Node tmpNode = new Grendgine_Collada_Node();
            nodeType = Grendgine_Collada_Node_Type.NODE;
            tmpNode.Type = nodeType;
            tmpNode.Name = nodeChunk.Name;
            tmpNode.ID = nodeChunk.Name;
            // Make the lists necessary for this Node.
            List<Grendgine_Collada_Matrix> matrices = new List<Grendgine_Collada_Matrix>();

            Grendgine_Collada_Matrix matrix = new Grendgine_Collada_Matrix();
            StringBuilder matrixString = new StringBuilder();
            // matrixString might have to be an identity matrix, since GetTransform is applying the transform to all the vertices.
            /*matrixString.AppendFormat("{0:F7} {1:F7} {2:F7} {3:F7} {4:F7} {5:F7} {6:F7} {7:F7} {8:F7} {9:F7} {10:F7} {11:F7} {12:F7} {13:F7} {14:F7} {15:F7}",
                nodeChunk.Transform.m11, nodeChunk.Transform.m12, nodeChunk.Transform.m13, nodeChunk.Transform.m14,
                nodeChunk.Transform.m21, nodeChunk.Transform.m22, nodeChunk.Transform.m23, nodeChunk.Transform.m24,
                nodeChunk.Transform.m31, nodeChunk.Transform.m32, nodeChunk.Transform.m33, nodeChunk.Transform.m34,
                nodeChunk.Transform.m41 / 100, nodeChunk.Transform.m42 / 100, nodeChunk.Transform.m43 / 100, nodeChunk.Transform.m44);*/
            matrixString.AppendFormat("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 0");
            matrix.Value_As_String = matrixString.ToString();
            matrix.sID = "transform";
            matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
            tmpNode.Matrix = matrices.ToArray();
            // there is no geometry and no instance_material for these.
            return tmpNode;
        }
        public void ValidateXml()  // For testing
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(null, @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\COLLADA_1_5.xsd");
                settings.ValidationType = ValidationType.Schema;

                XmlReader reader = XmlReader.Create(daeOutputFile.FullName, settings);
                XmlDocument document = new XmlDocument();
                document.Load(reader);

                ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

                Console.WriteLine("Validating Schema...");
                // the following call to Validate succeeds.
                document.Validate(eventHandler);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Console.WriteLine("Error: {0}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    Console.WriteLine("Warning {0}", e.Message);
                    break;
            }

        }

        public void WriteIDs()
        {

        }
        public void ValidateDoc()  // NYI
        {
            /// <summary> This method will check all the URLs used in the Collada object and see if any reference IDs that don't exist.  It will
            /// also check for duplicate IDs</summary>
            /// 
            // Check for duplicate IDs.  Populate the idList with all the IDs.
            //Console.WriteLine("In ValidateDoc");
            XElement root = XElement.Load(daeOutputFile.FullName);
            //Console.WriteLine("{0}", root.Value) ;
            //var nodes = root.Descendants("asset");//.Where(x => x.Attribute("id").Value == "adder_a_cockpit_standard");
            var nodes = root.Descendants();
            foreach (var node in nodes)
            {
                // Write out the node for now.
                // Console.WriteLine("ID: {0}", node);
                if (node.HasAttributes)
                {
                    //Console.WriteLine(" {0}={1}", node.Name, node.Value);
                    foreach (var attrib in nodes.Where(a => a.Name.Equals("adder_a_cockpit_standard-mesh-pos")))
                    {
                        Console.WriteLine("attrib: {0} == {1}", attrib.Name, attrib.Value);
                    }
                }

            }

            // Create a list of URLs and see if any reference an ID that doesn't exist.
        }

        #region Private Methods

        /// <summary>Takes the Material file name and returns just the file name with no extension</summary>
        private string CleanName(string cleanMe) 
        {
            string[] stringSeparators = new string[] { @"\", @"/" };
            string[] result;

            if (cleanMe.Contains(@"/") || cleanMe.Contains(@"\"))
            {
                // Take out path info
                result = cleanMe.Split(stringSeparators, StringSplitOptions.None);
                cleanMe = result[result.Length - 1];
            }
            // Check to see if extension is added, and if so strip it out. Look for "."
            if (cleanMe.Contains(@"."))
            {
                string[] periodSep = new string[] { @"." }; 
                result = cleanMe.Split(periodSep,StringSplitOptions.None);
                cleanMe = result[0];
                //Console.WriteLine("Cleanme is {0}", cleanMe);
            }
            return cleanMe;
        }
        #endregion
    }
}