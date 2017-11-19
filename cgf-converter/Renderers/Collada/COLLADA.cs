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
using CgfConverter.CryEngine_Core;
using System.Text.RegularExpressions;

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

            Utils.Log(LogLevelEnum.Debug, "Number of models: {0}", CryData.Models.Count);
            for (int i = 0; i < CryData.Models.Count; i++)
            {
                Utils.Log(LogLevelEnum.Debug, "\tNumber of nodes in model: {0}", CryData.Models[i].NodeMap.Count);
            }

            #region Output testing
            //foreach (ChunkDataStream stream in CryData.Models[1].ChunkMap.Values.Where(a => a.ChunkType == ChunkTypeEnum.DataStream))
            //{
            //    if (stream.DataStreamType == DataStreamTypeEnum.TANGENTS)
            //    {
            //        foreach (var vec in stream.Tangents)
            //        {
            //            Console.WriteLine("Tangent: {0:F6} {1:F6} {2:F6}", vec.x/127.0, vec.y/127.0, vec.z/127.0);
            //        }
            //        Console.WriteLine("Max x: {0}", stream.Normals.Max(a => a.x));
            //        Console.WriteLine("Max y: {0}", stream.Normals.Max(a => a.y));
            //        Console.WriteLine("Max z: {0}", stream.Normals.Max(a => a.z));
            //    }
            //}
            //foreach (ChunkNode node in CryData.Models[1].NodeMap.Values)
            //{
            //    Console.WriteLine("Node Chunk {0} in model {1}", node.Name, node._model.FileName);
            //    node.WriteChunk();
            //    Console.ReadKey();
            //}
            //foreach (ChunkNode node in CryData.Models[1].NodeMap.Values.Where(a => a.Name.Contains("Belly_Wing_Right_Decal")))
            //{
            //    node.WriteChunk();
            //    node.ParentNode.WriteChunk();
            //    node.ParentNode.ParentNode.WriteChunk();
            //    node.ParentNode.ParentNode.ParentNode.WriteChunk();
            //    Console.ReadKey();
            //}
            //foreach (var result in CryData.Models[0].SkinningInfo.BoneMapping)        // To see if the bone index > than the number of bones and bone weights
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        if (result.Weight[i] > 0)
            //            Console.WriteLine("Bone Weight: {0}", result.Weight[i]);
            //    }
            //}
            //Console.WriteLine("{0} bone weights found", CryData.Models[0].SkinningInfo.BoneMapping.Count);

            #endregion

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
            // If there is Skinning info, create the controller library and set up visual scene to refer to it.  Otherwise just write the Visual Scene
            if (CryData.SkinningInfo.HasSkinningInfo)
            {
                WriteLibrary_Controllers();
                WriteLibrary_VisualScenesWithSkeleton();
            }
            else
            {
                WriteLibrary_VisualScenes();
            }
            //WriteLibrary_Controllers();
            //WriteLibrary_VisualScenes();
            //WriteIDs();
            if (!daeOutputFile.Directory.Exists)
                daeOutputFile.Directory.Create();
            TextWriter writer = new StreamWriter(daeOutputFile.FullName);   // Makes the Textwriter object for the output
            mySerializer.Serialize(writer, daeObject);                      // Serializes the daeObject and writes to the writer
            // Validate that the Collada document is ok
            writer.Close();
            //ValidateXml();                                                  // validates against the schema
            //ValidateDoc();                                                // validates IDs and URLs
            Utils.Log(LogLevelEnum.Debug, "End of Write Collada.  Export complete.");
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
            asset.Unit = new Grendgine_Collada_Asset_Unit()
            {
                Meter = 1.0,
                Name = "meter"
            };
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
            for (int k = 0; k < CryData.Materials.Length; k++)
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
                            builder = new StringBuilder(CleanMtlFileName(CryData.Materials[k].Textures[i].File) + ".dds");
                            if (Args.TiffTextures)
                                builder.Replace(".dds", ".tif");

                        }
                        else
                        {
                            //builder = new StringBuilder(@"/" + this.Args.DataDir.Replace(@"\", @"/").Replace(" ", @"%20") + @"/" + CryData.Materials[k].Textures[i].File);
                            builder = new StringBuilder(@"/" + this.Args.DataDir.FullName.Replace(" ", @"%20") + @"/" + CryData.Materials[k].Textures[i].File);
                        }
                    }
                    else
                    {
                        builder = new StringBuilder(CryData.Materials[k].Textures[i].File);
                    }

                    if (!this.Args.TiffTextures)
                    {
                        builder.Replace(".tif", ".dds");
                        builder.Replace(".TIF", ".dds");
                    }
                    else
                        builder.Replace(".dds", ".tif");

                    // if 1.4.1, use URI.  If 1.5.0, use Ref.
                    if (daeObject.Collada_Version == "1.4.1")
                    {
                        tmpImage.Init_From.Uri = builder.ToString();
                    }
                    else
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
            Utils.Log(LogLevelEnum.Info, "Number of materials: {0}", numMaterials);
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
                }
                else
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
            for (int i = 0; i < numEffects; i++)
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
                    texSurface.sID = CleanMtlFileName(CryData.Materials[i].Textures[j].File) + "-surface";
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
                    texSampler.sID = CleanMtlFileName(CryData.Materials[i].Textures[j].File) + "-sampler";
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
                        diffound = true;
                        phong.Diffuse.Texture = new Grendgine_Collada_Texture();
                        // Texcoord is the ID of the UV source in geometries.  Not needed.
                        phong.Diffuse.Texture.Texture = CleanMtlFileName(texture.File) + "-sampler";
                        phong.Diffuse.Texture.TexCoord = "";
                    }
                    if (texture.Map == CryEngine_Core.Material.Texture.MapTypeEnum.Specular)
                    {
                        //Console.WriteLine("Found spec map");
                        specfound = true;
                        phong.Specular.Texture = new Grendgine_Collada_Texture();
                        phong.Specular.Texture.Texture = CleanMtlFileName(texture.File) + "-sampler";
                        phong.Specular.Texture.TexCoord = "";

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
                        bumpMap.Textures[0].Texture = CleanMtlFileName(texture.File) + "-sampler";
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
                phong.Emission.Color.Value_As_String = CryData.Materials[i].__Emissive.Replace(",", " ");
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

        /// <summary>
        /// Write the Library_Geometries element.  These won't be instantiated except through the visual scene or controllers.
        /// </summary>
        public void WriteLibrary_Geometries()
        {
            Grendgine_Collada_Library_Geometries libraryGeometries = new Grendgine_Collada_Library_Geometries();

            // Make a list for all the geometries objects we will need. Will convert to array at end.  Define the array here as well
            // Unfortunately we have to define a Geometry for EACH meshsubset in the meshsubsets, since the mesh can contain multiple materials
            List<Grendgine_Collada_Geometry> geometryList = new List<Grendgine_Collada_Geometry>();

            // For each of the nodes, we need to write the geometry.
            // Use a foreach statement to get all the node chunks.  This will get us the meshes, which will contain the vertex, UV and normal info.
            foreach (ChunkNode nodeChunk in this.CryData.Chunks.Where(a => a.ChunkType == ChunkTypeEnum.Node))
            {
                // Create a geometry object.  Use the chunk ID for the geometry ID
                // Will have to be careful with this, since with .cga/.cgam pairs will need to match by Name.
                // Now make the mesh object.  This will have 3 sources, 1 vertices, and 1 or more polylist (with material ID)
                // If the Object ID of Node chunk points to a Helper or a Controller though, place an empty.
                // Will have to figure out transforms here too.
                // need to make a list of the sources and triangles to add to tmpGeo.Mesh
                List<Grendgine_Collada_Source> sourceList = new List<Grendgine_Collada_Source>();
                //List<Grendgine_Collada_Triangles> triList = new List<Grendgine_Collada_Triangles>();  // Use PolyList over trilist
                List<Grendgine_Collada_Polylist> polylistList = new List<Grendgine_Collada_Polylist>();
                ChunkDataStream tmpNormals = null;
                ChunkDataStream tmpUVs = null;
                ChunkDataStream tmpVertices = null;
                ChunkDataStream tmpVertsUVs = null;
                ChunkDataStream tmpIndices = null;
                ChunkDataStream tmpColors = null;
                ChunkDataStream tmpTangents = null;
                //GeometryInfo geometryInfo = nodeChunk.ObjectChunk.

                // Don't render shields
                if (this.Args.SkipShieldNodes && nodeChunk.Name.StartsWith("$shield"))
                {
                    Utils.Log(LogLevelEnum.Debug, "Skipped shields node {0}", nodeChunk.Name);
                    continue;
                }

                // Don't render proxies
                if (this.Args.SkipProxyNodes && nodeChunk.Name.StartsWith("proxy"))
                {
                    Utils.Log(LogLevelEnum.Debug, "Skipped proxy node {0}", nodeChunk.Name);
                    continue;
                }

                if (nodeChunk.ObjectChunk == null)
                {
                    Utils.Log(LogLevelEnum.Warning, "Skipped node with missing Object {0}", nodeChunk.Name);
                    continue;
                }

                if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Mesh)
                {
                    // Get the mesh chunk and submesh chunk for this node.
                    ChunkMesh tmpMeshChunk = (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                    // Check to see if the Mesh points to a PhysicsData mesh.  Don't want to write these.
                    if (tmpMeshChunk.MeshPhysicsData != 0)
                    {
                        // TODO:  Implement this chunk
                    }
                    if (tmpMeshChunk.MeshSubsets != 0)             // For the SC files, you can have Mesh chunks with no Mesh Subset.  Need to skip these.  They are in the .cga file and contain no geometry.  Just stub info.
                    {
                        //Console.WriteLine("tmpMeshChunk ID is {0:X}", nodeChunk.ObjectNodeID);
                        //tmpMeshChunk.WriteChunk();
                        //Console.WriteLine("tmpmeshsubset ID is {0:X}", tmpMeshChunk.MeshSubsets);
                        ChunkMeshSubsets tmpMeshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node

                        if (tmpMeshChunk.MeshSubsets != 0)
                        {
                            tmpMeshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node
                        }
                        // Get pointers to the vertices data
                        if (tmpMeshChunk.VerticesData != 0)
                        {
                            tmpVertices = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.VerticesData];
                        }
                        if (tmpMeshChunk.NormalsData != 0)
                        {
                            tmpNormals = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.NormalsData];
                        }
                        if (tmpMeshChunk.UVsData != 0)
                        {
                            tmpUVs = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.UVsData];
                        }
                        if (tmpMeshChunk.VertsUVsData != 0)     // Star Citizen file.  That means VerticesData and UVsData will probably be empty.  Need to handle both cases.
                        {
                            tmpVertsUVs = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.VertsUVsData];
                        }
                        if (tmpMeshChunk.IndicesData != 0)
                        {
                            tmpIndices = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.IndicesData];
                        }
                        // Ignore Tangent and Color data for now.
                        if (tmpMeshChunk.ColorsData != 0)
                        {
                            tmpColors = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.ColorsData];
                        }
                        if (tmpMeshChunk.TangentsData != 0)
                        {
                            tmpTangents = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.TangentsData];
                        }
                        if (tmpVertices == null && tmpVertsUVs == null)
                        {
                            // There is no vertex data for this node.  Skip.
                            continue;
                        }

                        // tmpGeo is a Geometry object for each meshsubset.  Name will be "Nodechunk name_matID".  Hopefully there is only one matID used per submesh
                        Grendgine_Collada_Geometry tmpGeo = new Grendgine_Collada_Geometry();
                        tmpGeo.Name = nodeChunk.Name;
                        tmpGeo.ID = nodeChunk.Name + "-mesh";
                        Grendgine_Collada_Mesh tmpMesh = new Grendgine_Collada_Mesh();
                        tmpGeo.Mesh = tmpMesh;

                        // TODO:  Move the source creation to a separate function.  Too much retyping.
                        Grendgine_Collada_Source[] source = new Grendgine_Collada_Source[3];        // 3 possible source types.
                        // need a collada_source for position, normal, UV, tangents and color, what the source is (verts), and the tri index
                        //Grendgine_Collada_Source posSource = GetMeshSource(tmpVertices, nodeChunk);       // TODO:  Implement this.
                        Grendgine_Collada_Source posSource = new Grendgine_Collada_Source();
                        Grendgine_Collada_Source normSource = new Grendgine_Collada_Source();
                        Grendgine_Collada_Source uvSource = new Grendgine_Collada_Source();
                        //Grendgine_Collada_Source tangentSource = new Grendgine_Collada_Source();
                        source[0] = posSource;
                        source[1] = normSource;
                        source[2] = uvSource;
                        //source[3] = colorSource;
                        //source[3] = tangentSource;
                        posSource.ID = nodeChunk.Name + "-mesh-pos";
                        posSource.Name = nodeChunk.Name + "-pos";
                        normSource.ID = nodeChunk.Name + "-mesh-norm";
                        normSource.Name = nodeChunk.Name + "-norm";
                        uvSource.Name = nodeChunk.Name + "-UV";
                        uvSource.ID = nodeChunk.Name + "-mesh-UV";
                        //tangentSource.Name = nodeChunk.Name + "-tangent";
                        //tangentSource.ID = nodeChunk.Name + "-mesh-tangent";

                        #region Create vertices node.  For polylist will just have VERTEX.
                        Grendgine_Collada_Vertices vertices = new Grendgine_Collada_Vertices();
                        vertices.ID = nodeChunk.Name + "-vertices";
                        tmpGeo.Mesh.Vertices = vertices;
                        Grendgine_Collada_Input_Unshared[] inputshared = new Grendgine_Collada_Input_Unshared[3];
                        Grendgine_Collada_Input_Unshared posInput = new Grendgine_Collada_Input_Unshared();
                        posInput.Semantic = Grendgine_Collada_Input_Semantic.POSITION;
                        vertices.Input = inputshared;
                        #endregion

                        Grendgine_Collada_Input_Unshared normInput = new Grendgine_Collada_Input_Unshared();
                        Grendgine_Collada_Input_Unshared uvInput = new Grendgine_Collada_Input_Unshared();
                        Grendgine_Collada_Input_Unshared colorInput = new Grendgine_Collada_Input_Unshared();
                        //Grendgine_Collada_Input_Unshared tangentInput = new Grendgine_Collada_Input_Unshared();

                        normInput.Semantic = Grendgine_Collada_Input_Semantic.NORMAL;
                        uvInput.Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD;   // might need to replace TEXCOORD with UV
                        colorInput.Semantic = Grendgine_Collada_Input_Semantic.COLOR;
                        //tangentInput.Semantic = Grendgine_Collada_Input_Semantic.TANGENT;

                        posInput.source = "#" + posSource.ID;
                        normInput.source = "#" + normSource.ID;
                        uvInput.source = "#" + uvSource.ID;
                        //tangentInput.source = "#" + tangentSource.ID;
                        inputshared[0] = posInput;

                        // Create a float_array object to store all the data
                        Grendgine_Collada_Float_Array floatArrayVerts = new Grendgine_Collada_Float_Array();
                        Grendgine_Collada_Float_Array floatArrayNormals = new Grendgine_Collada_Float_Array();
                        Grendgine_Collada_Float_Array floatArrayUVs = new Grendgine_Collada_Float_Array();
                        Grendgine_Collada_Float_Array floatArrayColors = new Grendgine_Collada_Float_Array();
                        Grendgine_Collada_Float_Array floatArrayTangents = new Grendgine_Collada_Float_Array();
                        // Strings for vertices
                        StringBuilder vertString = new StringBuilder();
                        StringBuilder normString = new StringBuilder();
                        StringBuilder uvString = new StringBuilder();

                        if (tmpVertices != null)  // Will be null if it's using VertsUVs.
                        {
                            floatArrayVerts.ID = posSource.ID + "-array";
                            floatArrayVerts.Digits = 6;
                            floatArrayVerts.Magnitude = 38;
                            floatArrayVerts.Count = (int)tmpVertices.NumElements * 3;
                            floatArrayUVs.ID = uvSource.ID + "-array";
                            floatArrayUVs.Digits = 6;
                            floatArrayUVs.Magnitude = 38;
                            floatArrayUVs.Count = (int)tmpUVs.NumElements * 2;
                            floatArrayNormals.ID = normSource.ID + "-array";
                            floatArrayNormals.Digits = 6;
                            floatArrayNormals.Magnitude = 38;
                            floatArrayNormals.Count = (int)tmpNormals.NumElements * 3;
                            // Create Vertices and normals string
                            for (uint j = 0; j < tmpMeshChunk.NumVertices; j++)
                            {
                                Vector3 vertex = (tmpVertices.Vertices[j]);
                                vertString.AppendFormat("{0:F6} {1:F6} {2:F6} ", vertex.x, vertex.y, vertex.z);
                                Vector3 normal = tmpNormals.Normals[j];
                                normString.AppendFormat("{0:F6} {1:F6} {2:F6} ", safe(normal.x), safe(normal.y), safe(normal.z));
                            }
                            // Create UV string
                            for (uint j = 0; j < tmpUVs.NumElements; j++)
                            {
                                uvString.AppendFormat("{0:F6} {1:F6} ", safe(tmpUVs.UVs[j].U), 1 - safe(tmpUVs.UVs[j].V));
                            }
                        }
                        else                // VertsUV structure.  Pull out verts and UVs from tmpVertsUVs.
                        {
                            floatArrayVerts.ID = posSource.ID + "-array";
                            floatArrayVerts.Digits = 6;
                            floatArrayVerts.Magnitude = 38;
                            floatArrayVerts.Count = (int)tmpVertsUVs.NumElements * 3;
                            floatArrayUVs.ID = uvSource.ID + "-array";
                            floatArrayUVs.Digits = 6;
                            floatArrayUVs.Magnitude = 38;
                            floatArrayUVs.Count = (int)tmpVertsUVs.NumElements * 2;
                            floatArrayNormals.ID = normSource.ID + "-array";
                            floatArrayNormals.Digits = 6;
                            floatArrayNormals.Magnitude = 38;
                            floatArrayNormals.Count = (int)tmpVertsUVs.NumElements * 3;
                            // Create Vertices and normals string
                            for (uint j = 0; j < tmpMeshChunk.NumVertices; j++)
                            {
                                // Rotate/translate the vertex
                                // Dymek's code to rescale by bounding box.  Only apply to geometry (cga or cgf), and not skin or chr objects.
                                if (!CryData.InputFile.EndsWith("skin") && !CryData.InputFile.EndsWith("chr"))
                                {
                                    double multiplerX = Math.Abs(tmpMeshChunk.MinBound.x - tmpMeshChunk.MaxBound.x) / 2f;
                                    double multiplerY = Math.Abs(tmpMeshChunk.MinBound.y - tmpMeshChunk.MaxBound.y) / 2f;
                                    double multiplerZ = Math.Abs(tmpMeshChunk.MinBound.z - tmpMeshChunk.MaxBound.z) / 2f;
                                    if (multiplerX < 1) { multiplerX = 1; }
                                    if (multiplerY < 1) { multiplerY = 1; }
                                    if (multiplerZ < 1) { multiplerZ = 1; }
                                    tmpVertsUVs.Vertices[j].x = tmpVertsUVs.Vertices[j].x * multiplerX + (tmpMeshChunk.MaxBound.x + tmpMeshChunk.MinBound.x) / 2;
                                    tmpVertsUVs.Vertices[j].y = tmpVertsUVs.Vertices[j].y * multiplerY + (tmpMeshChunk.MaxBound.y + tmpMeshChunk.MinBound.y) / 2;
                                    tmpVertsUVs.Vertices[j].z = tmpVertsUVs.Vertices[j].z * multiplerZ + (tmpMeshChunk.MaxBound.z + tmpMeshChunk.MinBound.z) / 2;
                                }

                                Vector3 vertex = tmpVertsUVs.Vertices[j];
                                vertString.AppendFormat("{0:F6} {1:F6} {2:F6} ", vertex.x, vertex.y, vertex.z);
                                Vector3 normal = new Vector3();
                                // Normals depend on the data size.  16 byte structures have the normals in the Tangents.  20 byte structures are in the VertsUV.
                                if (tmpVertsUVs.BytesPerElement == 20)
                                {
                                    normal = tmpVertsUVs.Normals[j];
                                } 
                                else
                                {
                                    //normal = tmpTangents.Normals[j];
                                    //normal.x = normal.x / 32767.0;
                                    //normal.y = normal.y / 32767.0;
                                    //normal.z = normal.z / 32767.0;                                    
                                    normal = tmpVertsUVs.Normals[j];
                                }
                                normString.AppendFormat("{0:F6} {1:F6} {2:F6} ", safe(normal.x), safe(normal.y), safe(normal.z));
                            }
                            // Create UV string
                            for (uint j = 0; j < tmpVertsUVs.NumElements; j++)
                            {
                                uvString.AppendFormat("{0:F6} {1:F6} ", safe(tmpVertsUVs.UVs[j].U), safe(1 - tmpVertsUVs.UVs[j].V));
                            }
                        }
                        CleanNumbers(vertString);
                        CleanNumbers(normString);
                        CleanNumbers(uvString);

                        //floatArrayTangents.ID = tangentSource.ID + "-array";
                        //floatArrayTangents.Digits = 6;
                        //floatArrayTangents.Magnitude = 38;
                        //floatArrayTangents.Count = (int)tmpTangents.NumElements * 2;
                        //StringBuilder tangentString = new StringBuilder();
                        // Create Tangent string
                        //for (uint j = 0; j < tmpTangents.NumElements; j++)
                        //{
                        //    tangentString.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} ", 
                        //        tmpTangents.Tangents[j, 0].w / 32767, tmpTangents.Tangents[j, 0].x / 32767, tmpTangents.Tangents[j, 0].y / 32767, tmpTangents.Tangents[j, 0].z / 32767,
                        //        tmpTangents.Tangents[j, 1].w / 32767, tmpTangents.Tangents[j, 1].x / 32767, tmpTangents.Tangents[j, 1].y / 32767, tmpTangents.Tangents[j, 1].z / 32767);
                        //}
                        //CleanNumbers(tangentString);

                        #region Create the polylist node.
                        Grendgine_Collada_Polylist[] polylists = new Grendgine_Collada_Polylist[tmpMeshSubsets.NumMeshSubset];
                        tmpGeo.Mesh.Polylist = polylists;

                        for (uint j = 0; j < tmpMeshSubsets.NumMeshSubset; j++) // Need to make a new Polylist entry for each submesh.
                        {
                            polylists[j] = new Grendgine_Collada_Polylist();
                            polylists[j].Count = (int)tmpMeshSubsets.MeshSubsets[j].NumIndices / 3;
                            if (CryData.Materials.Count() != 0)
                            {
                                polylists[j].Material = CryData.Materials[tmpMeshSubsets.MeshSubsets[j].MatID].Name + "-material";
                            }
                            // Create the 4 inputs.  vertex, normal, texcoord, tangent
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
                            //polylists[j].Input[3] = new Grendgine_Collada_Input_Shared();
                            //polylists[j].Input[3].Semantic = Grendgine_Collada_Input_Semantic.TANGENT;
                            //polylists[j].Input[3].Offset = 3;
                            //polylists[j].Input[3].source = "#" + tangentSource.ID;

                            // Create the vcount list.  All triangles, so the subset number of indices.
                            StringBuilder vc = new StringBuilder();
                            for (uint k = tmpMeshSubsets.MeshSubsets[j].FirstIndex; k < (tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices); k++)
                            {
                                vc.AppendFormat("3 ");
                                k += 2;
                            }
                            polylists[j].VCount = new Grendgine_Collada_Int_Array_String
                            {
                                Value_As_String = vc.ToString().TrimEnd()
                            };

                            // Create the P node for the Polylist.
                            StringBuilder p = new StringBuilder();
                            for (uint k = tmpMeshSubsets.MeshSubsets[j].FirstIndex; k < (tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices); k++)
                            {
                                //p.AppendFormat("{0} {0} {0} {0} {1} {1} {1} {1} {2} {2} {2} {2} ", tmpIndices.Indices[k], tmpIndices.Indices[k + 1], tmpIndices.Indices[k + 2]);
                                p.AppendFormat("{0} {0} {0} {1} {1} {1} {2} {2} {2} ", tmpIndices.Indices[k], tmpIndices.Indices[k + 1], tmpIndices.Indices[k + 2]);
                                k += 2;
                            }
                            polylists[j].P = new Grendgine_Collada_Int_Array_String();
                            polylists[j].P.Value_As_String = p.ToString().TrimEnd();
                        }

                        #endregion

                        #region Create the source float_array nodes.  Vertex, normal, UV.  May need color as well.

                        floatArrayVerts.Value_As_String = vertString.ToString().TrimEnd();
                        floatArrayNormals.Value_As_String = normString.ToString().TrimEnd();
                        floatArrayUVs.Value_As_String = uvString.ToString().TrimEnd();
                        //floatArrayColors.Value_As_String = colorString.ToString();
                        //floatArrayTangents.Value_As_String = tangentString.ToString().TrimEnd();

                        source[0].Float_Array = floatArrayVerts;
                        source[1].Float_Array = floatArrayNormals;
                        source[2].Float_Array = floatArrayUVs;
                        //source[3].Float_Array = floatArrayColors;
                        //source[3].Float_Array = floatArrayTangents;
                        tmpGeo.Mesh.Source = source;

                        // create the technique_common for each of these
                        posSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source();
                        posSource.Technique_Common.Accessor = new Grendgine_Collada_Accessor();
                        posSource.Technique_Common.Accessor.Source = "#" + floatArrayVerts.ID;
                        posSource.Technique_Common.Accessor.Stride = 3;
                        posSource.Technique_Common.Accessor.Count = (uint)tmpMeshChunk.NumVertices;
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
                        normSource.Technique_Common.Accessor.Count = (uint)tmpMeshChunk.NumVertices;
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
                        if (tmpVertices != null)
                        {
                            uvSource.Technique_Common.Accessor.Count = tmpUVs.NumElements;
                        }
                        else
                        {
                            uvSource.Technique_Common.Accessor.Count = tmpVertsUVs.NumElements;
                        }

                        Grendgine_Collada_Param[] paramUV = new Grendgine_Collada_Param[2];
                        paramUV[0] = new Grendgine_Collada_Param();
                        paramUV[1] = new Grendgine_Collada_Param();
                        paramUV[0].Name = "S";
                        paramUV[0].Type = "float";
                        paramUV[1].Name = "T";
                        paramUV[1].Type = "float";
                        uvSource.Technique_Common.Accessor.Param = paramUV;

                        //tangentSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source();
                        //tangentSource.Technique_Common.Accessor = new Grendgine_Collada_Accessor();
                        //tangentSource.Technique_Common.Accessor.Source = "#" + floatArrayTangents.ID;
                        //tangentSource.Technique_Common.Accessor.Stride = 8;
                        //tangentSource.Technique_Common.Accessor.Count = tmpTangents.NumElements;

                        /*
                        //if (tmpColors != null)
                        //{
                        //    colorSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source();
                        //    colorSource.Technique_Common.Accessor = new Grendgine_Collada_Accessor();
                        //    colorSource.Technique_Common.Accessor.Source = "#" + floatArrayColors.ID;
                        //    colorSource.Technique_Common.Accessor.Stride = 3;
                        //    colorSource.Technique_Common.Accessor.Count = tmpColors.NumElements;
                        //    Grendgine_Collada_Param[] paramColor = new Grendgine_Collada_Param[3];
                        //    paramColor[0] = new Grendgine_Collada_Param();
                        //    paramColor[1] = new Grendgine_Collada_Param();
                        //    paramColor[2] = new Grendgine_Collada_Param();
                        //    paramColor[0].Name = "R";
                        //    paramColor[0].Type = "float";
                        //    paramColor[1].Name = "G";
                        //    paramColor[1].Type = "float";
                        //    paramColor[2].Name = "B";
                        //    paramColor[2].Type = "float";
                        //    colorSource.Technique_Common.Accessor.Param = paramColor;
                        //}
                        */

                        geometryList.Add(tmpGeo);

                        #endregion
                    }
                }
                // There is no geometry for a helper or controller node.  Can skip the rest.
            }
            libraryGeometries.Geometry = geometryList.ToArray();
            daeObject.Library_Geometries = libraryGeometries;
        }

        public void WriteLibrary_Controllers()
        {
            // Set up the controller library.
            Grendgine_Collada_Library_Controllers libraryController = new Grendgine_Collada_Library_Controllers();

            // There can be multiple controllers in the controller library.  But for Cryengine files, there is only one rig.
            // So if a rig exists, make that the controller.  This applies mostly to .chr files, which will have a rig and geometry.
            Grendgine_Collada_Controller controller = new Grendgine_Collada_Controller();          // just need the one.
            controller.ID = "Controller";
            // Create the skin object and assign to the controller
            Grendgine_Collada_Skin skin = new Grendgine_Collada_Skin
            {
                source = "#" + daeObject.Library_Geometries.Geometry[0].ID,
                Bind_Shape_Matrix = new Grendgine_Collada_Float_Array_String()
            };
            skin.Bind_Shape_Matrix.Value_As_String = CreateStringFromMatrix44(Matrix44.Identity());         // We will assume the BSM is the identity matrix for now
            // Create the 3 sources for this controller:  joints, bind poses, and weights
            skin.Source = new Grendgine_Collada_Source[3];

            // Populate the data.
            // Need to map the exterior vertices (geometry) to the int vertices.  Or use the Bone Map datastream if it exists (check HasBoneMapDatastream).
            #region Joints Source
            Grendgine_Collada_Source jointsSource = new Grendgine_Collada_Source()
            {
                ID = "Controller-joints"
            };
            jointsSource.Name_Array = new Grendgine_Collada_Name_Array()
            {
                ID = "Controller-joints-array",
                Count = CryData.SkinningInfo.CompiledBones.Count,
            };
            StringBuilder boneNames = new StringBuilder();
            for (int i = 0; i < CryData.SkinningInfo.CompiledBones.Count; i++)
            {
                boneNames.Append(CryData.SkinningInfo.CompiledBones[i].boneName.Replace(' ', '_') + " ");
            }
            jointsSource.Name_Array.Value_Pre_Parse = boneNames.ToString().TrimEnd();
            jointsSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source();
            jointsSource.Technique_Common.Accessor = new Grendgine_Collada_Accessor
            {
                Source = "#Controller-joints-array",
                Count = (uint)CryData.SkinningInfo.CompiledBones.Count,
                Stride = 1
            };
            skin.Source[0] = jointsSource;
            #endregion

            #region Bind Pose Array Source
            Grendgine_Collada_Source bindPoseArraySource = new Grendgine_Collada_Source()
            {
                ID = "Controller-bind_poses"
            };
            bindPoseArraySource.Float_Array = new Grendgine_Collada_Float_Array()
            {
                ID = "Controller-bind_poses-array",
                Count = CryData.SkinningInfo.CompiledBones.Count * 16,
            };
            bindPoseArraySource.Float_Array.Value_As_String = GetBindPoseArray(CryData.SkinningInfo.CompiledBones);
            bindPoseArraySource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
            {
                Accessor = new Grendgine_Collada_Accessor
                {
                    Source = "#Controller-bind_poses-array",
                    Count = (uint)CryData.SkinningInfo.CompiledBones.Count,
                    Stride = 16,
                }
            };
            bindPoseArraySource.Technique_Common.Accessor.Param = new Grendgine_Collada_Param[1];
            bindPoseArraySource.Technique_Common.Accessor.Param[0] = new Grendgine_Collada_Param
            {
                Name = "TRANSFORM",
                Type = "float4x4"
            };
            skin.Source[1] = bindPoseArraySource;
            #endregion

            #region Weights Source
            Grendgine_Collada_Source weightArraySource = new Grendgine_Collada_Source()
            {
                ID = "Controller-weights"
            };
            weightArraySource.Technique_Common = new Grendgine_Collada_Technique_Common_Source();
            Grendgine_Collada_Accessor accessor = weightArraySource.Technique_Common.Accessor = new Grendgine_Collada_Accessor();

            weightArraySource.Float_Array = new Grendgine_Collada_Float_Array()
            {
                ID = "Controller-weights-array",
            };
            StringBuilder weights = new StringBuilder();

            if (CryData.SkinningInfo.IntVertices == null)       // This is a case where there are bones, and only Bone Mapping data from a datastream chunk.  Skin files.
            {
                weightArraySource.Float_Array.Count = CryData.SkinningInfo.BoneMapping.Count;
                for (int i = 0; i < CryData.SkinningInfo.BoneMapping.Count; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        weights.Append(((float)CryData.SkinningInfo.BoneMapping[i].Weight[j] / 255).ToString() + " ");
                    }
                };
                accessor.Count = (uint)CryData.SkinningInfo.BoneMapping.Count * 4;
            }
            else                                                // Bones and int verts.  Will use int verts for weights, but this doesn't seem perfect either.
            {
                weightArraySource.Float_Array.Count = CryData.SkinningInfo.Ext2IntMap.Count;
                for (int i = 0; i < CryData.SkinningInfo.Ext2IntMap.Count; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        weights.Append(CryData.SkinningInfo.IntVertices[CryData.SkinningInfo.Ext2IntMap[i]].Weights[j] + " ");
                    }
                    accessor.Count = (uint)CryData.SkinningInfo.Ext2IntMap.Count * 4;
                };
            }
            CleanNumbers(weights);
            weightArraySource.Float_Array.Value_As_String = weights.ToString().TrimEnd();
            // Add technique_common part.
            accessor.Source = "#Controller-weights-array";
            accessor.Stride = 1;
            accessor.Param = new Grendgine_Collada_Param[1];
            accessor.Param[0] = new Grendgine_Collada_Param();
            accessor.Param[0].Name = "WEIGHT";
            accessor.Param[0].Type = "float";
            skin.Source[2] = weightArraySource;

            #endregion

            #region Joints
            skin.Joints = new Grendgine_Collada_Joints
            {
                Input = new Grendgine_Collada_Input_Unshared[2]
            };
            skin.Joints.Input[0] = new Grendgine_Collada_Input_Unshared
            {
                Semantic = new Grendgine_Collada_Input_Semantic()
            };
            skin.Joints.Input[0].Semantic = Grendgine_Collada_Input_Semantic.JOINT;
            skin.Joints.Input[0].source = "#Controller-joints";
            skin.Joints.Input[1] = new Grendgine_Collada_Input_Unshared
            {
                Semantic = new Grendgine_Collada_Input_Semantic()
            };
            skin.Joints.Input[1].Semantic = Grendgine_Collada_Input_Semantic.INV_BIND_MATRIX;
            skin.Joints.Input[1].source = "#Controller-bind_poses";
            #endregion

            #region Vertex Weights
            Grendgine_Collada_Vertex_Weights vertexWeights = skin.Vertex_Weights = new Grendgine_Collada_Vertex_Weights();
            vertexWeights.Count = CryData.SkinningInfo.BoneMapping.Count;
            skin.Vertex_Weights.Input = new Grendgine_Collada_Input_Shared[2];
            Grendgine_Collada_Input_Shared jointSemantic = skin.Vertex_Weights.Input[0] = new Grendgine_Collada_Input_Shared();
            jointSemantic.Semantic = Grendgine_Collada_Input_Semantic.JOINT;
            jointSemantic.source = "#Controller-joints";
            jointSemantic.Offset = 0;
            Grendgine_Collada_Input_Shared weightSemantic = skin.Vertex_Weights.Input[1] = new Grendgine_Collada_Input_Shared();
            weightSemantic.Semantic = Grendgine_Collada_Input_Semantic.WEIGHT;
            weightSemantic.source = "#Controller-weights";
            weightSemantic.Offset = 1;
            StringBuilder vCount = new StringBuilder();
            //for (int i = 0; i < CryData.Models[0].SkinningInfo.IntVertices.Count; i++)
            for (int i = 0; i < CryData.SkinningInfo.BoneMapping.Count; i++)
            {
                vCount.Append("4 ");
            };
            vertexWeights.VCount = new Grendgine_Collada_Int_Array_String();
            vertexWeights.VCount.Value_As_String = vCount.ToString().TrimEnd();
            StringBuilder vertices = new StringBuilder();
            //for (int i = 0; i < CryData.Models[0].SkinningInfo.IntVertices.Count * 4; i++)
            int index = 0;
            if (!CryData.Models[0].SkinningInfo.HasIntToExtMapping)
            {
                for (int i=0; i < CryData.SkinningInfo.BoneMapping.Count; i++)
                {
                    int wholePart = (int)i / 4;
                    vertices.Append(CryData.SkinningInfo.BoneMapping[i].BoneIndex[0] + " " + index + " ");
                    vertices.Append(CryData.SkinningInfo.BoneMapping[i].BoneIndex[1] + " " + (index+1) + " ");
                    vertices.Append(CryData.SkinningInfo.BoneMapping[i].BoneIndex[2] + " " + (index+2) + " ");
                    vertices.Append(CryData.SkinningInfo.BoneMapping[i].BoneIndex[3] + " " + (index+3) + " ");
                    index = index + 4;
                }
            }
            else
            {
                for (int i = 0; i < CryData.SkinningInfo.Ext2IntMap.Count; i++)
                {
                    int wholePart = (int)i / 4;
                    vertices.Append(CryData.SkinningInfo.IntVertices[CryData.SkinningInfo.Ext2IntMap[i]].BoneIDs[0] + " " + index + " ");
                    vertices.Append(CryData.SkinningInfo.IntVertices[CryData.SkinningInfo.Ext2IntMap[i]].BoneIDs[1] + " " + (index + 1) + " ");
                    vertices.Append(CryData.SkinningInfo.IntVertices[CryData.SkinningInfo.Ext2IntMap[i]].BoneIDs[2] + " " + (index + 2) + " ");
                    vertices.Append(CryData.SkinningInfo.IntVertices[CryData.SkinningInfo.Ext2IntMap[i]].BoneIDs[3] + " " + (index + 3) + " ");

                    index = index + 4;
                }
            }
            vertexWeights.V = new Grendgine_Collada_Int_Array_String();
            vertexWeights.V.Value_As_String = vertices.ToString().TrimEnd();
            #endregion

            // create the extra element for the FCOLLADA profile
            controller.Extra = new Grendgine_Collada_Extra[1];
            controller.Extra[0] = new Grendgine_Collada_Extra();
            controller.Extra[0].Technique = new Grendgine_Collada_Technique[1];
            controller.Extra[0].Technique[0] = new Grendgine_Collada_Technique();
            controller.Extra[0].Technique[0].profile = "FCOLLADA";
            controller.Extra[0].Technique[0].UserProperties = "SkinController";
            

            // Add the parts to their parents
            controller.Skin = skin;
            libraryController.Controller = new Grendgine_Collada_Controller[1];
            libraryController.Controller[0] = controller;
            daeObject.Library_Controllers = libraryController;
        }

        /// <summary>
        /// Provides a library in which to place visual_scene elements. 
        /// </summary>
        public void WriteLibrary_VisualScenes()
        {
            // Set up the library
            Grendgine_Collada_Library_Visual_Scenes libraryVisualScenes = new Grendgine_Collada_Library_Visual_Scenes();

            // There can be multiple visual scenes.  Will just have one (World) for now.  All node chunks go under Nodes for that visual scene
            List<Grendgine_Collada_Visual_Scene> visualScenes = new List<Grendgine_Collada_Visual_Scene>();
            List<Grendgine_Collada_Node> nodes = new List<Grendgine_Collada_Node>();

            // Check to see if there is a CompiledBones chunk.  If so, add a Node.
            if (CryData.Chunks.Any(a => a.ChunkType == ChunkTypeEnum.CompiledBones || a.ChunkType == ChunkTypeEnum.CompiledBonesSC))
            {
                Grendgine_Collada_Node boneNode = new Grendgine_Collada_Node();
                boneNode = CreateJointNode(CryData.Bones.RootBone);
                nodes.Add(boneNode);
            }

            // Geometry visual Scene.
            Grendgine_Collada_Visual_Scene visualScene = new Grendgine_Collada_Visual_Scene();
            Grendgine_Collada_Node rootNode = new Grendgine_Collada_Node();

            if (CryData.Models.Count > 1) // Star Citizen model with .cga/.cgam pair.
            {
                // First model file (.cga or .cgf) will contain the main Root Node, along with all non geometry Node chunks (placeholders).
                // Second one will have all the datastreams, but needs to be tied to the RootNode of the first model.
                // THERE CAN BE MULTIPLE ROOT NODES IN EACH FILE!  Check to see if the parentnodeid ~0 and be sure to add a node for it.
                List<Grendgine_Collada_Node> positionNodes = new List<Grendgine_Collada_Node>();        // For SC files, these are the nodes in the .cga/.cgf files.
                List<ChunkNode> positionRoots = CryData.Models[0].NodeMap.Values.Where(a => a.ParentNodeID == ~0).ToList();
                foreach (ChunkNode root in positionRoots)
                {
                    positionNodes.Add(CreateNode(root));
                }
                nodes.AddRange(positionNodes.ToArray());
            }
            else
            {
                nodes.Add(CreateNode(CryData.RootNode));
            }

            visualScene.Node = nodes.ToArray();
            visualScene.ID = "Scene";
            visualScenes.Add(visualScene);

            libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
            daeObject.Library_Visual_Scene = libraryVisualScenes;
        }

        /// <summary>
        /// Provides a library in which to place visual_scene elements for chr files (rigs + geometry). 
        /// </summary>
        public void WriteLibrary_VisualScenesWithSkeleton()
        {
            // Set up the library
            Grendgine_Collada_Library_Visual_Scenes libraryVisualScenes = new Grendgine_Collada_Library_Visual_Scenes();

            // There can be multiple visual scenes.  Will just have one (World) for now.  All node chunks go under Nodes for that visual scene
            List<Grendgine_Collada_Visual_Scene> visualScenes = new List<Grendgine_Collada_Visual_Scene>();
            List<Grendgine_Collada_Node> nodes = new List<Grendgine_Collada_Node>();

            // Check to see if there is a CompiledBones chunk.  If so, add a Node.  
            if (CryData.Chunks.Any(a => a.ChunkType == ChunkTypeEnum.CompiledBones || a.ChunkType == ChunkTypeEnum.CompiledBonesSC))
            {
                Grendgine_Collada_Node boneNode = new Grendgine_Collada_Node();
                boneNode = CreateJointNode(CryData.Bones.RootBone);
                nodes.Add(boneNode);
            }

            // Geometry visual Scene.
            Grendgine_Collada_Visual_Scene visualScene = new Grendgine_Collada_Visual_Scene();
            Grendgine_Collada_Node rootNode = new Grendgine_Collada_Node
            {
                ID = CryData.Models[0].FileName,
                Name = CryData.Models[0].FileName,
                Type = Grendgine_Collada_Node_Type.NODE,
                Matrix = new Grendgine_Collada_Matrix[1]
            };
            rootNode.Matrix[0] = new Grendgine_Collada_Matrix
            {
                Value_As_String = CreateStringFromMatrix44(Matrix44.Identity())
            };
            rootNode.Instance_Controller = new Grendgine_Collada_Instance_Controller[1];
            rootNode.Instance_Controller[0] = new Grendgine_Collada_Instance_Controller();
            rootNode.Instance_Controller[0].URL = "#Controller";
            rootNode.Instance_Controller[0].Skeleton = new Grendgine_Collada_Skeleton[1];
            Grendgine_Collada_Skeleton skeleton = rootNode.Instance_Controller[0].Skeleton[0] = new Grendgine_Collada_Skeleton();
            skeleton.Value = "#Armature";
            rootNode.Instance_Controller[0].Bind_Material = new Grendgine_Collada_Bind_Material[1];
            Grendgine_Collada_Bind_Material bindMaterial = rootNode.Instance_Controller[0].Bind_Material[0] = new Grendgine_Collada_Bind_Material();

            // Create an Instance_Material for each material
            bindMaterial.Technique_Common = new Grendgine_Collada_Technique_Common_Bind_Material();
            List<Grendgine_Collada_Instance_Material_Geometry> instanceMaterials = new List<Grendgine_Collada_Instance_Material_Geometry>();
            bindMaterial.Technique_Common.Instance_Material = new Grendgine_Collada_Instance_Material_Geometry[CryData.Materials.Length];
            // This gets complicated.  We need to make one instance_material for each material used in this node chunk.  The mat IDs used in this
            // node chunk are stored in meshsubsets, so for each subset we need to grab the mat, get the target (id), and make an instance_material for it.
            for (int i = 0; i < CryData.Materials.Length; i++)
            {
                // For each mesh subset, we want to create an instance material and add it to instanceMaterials list.
                Grendgine_Collada_Instance_Material_Geometry tmpInstanceMat = new Grendgine_Collada_Instance_Material_Geometry();
                //tmpInstanceMat.Target = "#" + tmpMeshSubsets.MeshSubsets[i].MatID;
                tmpInstanceMat.Target = "#" + CryData.Materials[i].Name + "-material";
                //tmpInstanceMat.Symbol = CryData.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name;
                tmpInstanceMat.Symbol = CryData.Materials[i].Name + "-material";
                instanceMaterials.Add(tmpInstanceMat);
            }
            rootNode.Instance_Controller[0].Bind_Material[0].Technique_Common.Instance_Material = instanceMaterials.ToArray();

            nodes.Add(rootNode);
            visualScene.Node = nodes.ToArray();
            visualScene.ID = "Scene";
            visualScenes.Add(visualScene);

            libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
            daeObject.Library_Visual_Scene = libraryVisualScenes;
        }

        #region Private Methods
        private Grendgine_Collada_Node CreateNode(ChunkNode nodeChunk)
        {
            // This will be used recursively to create a node object and return it to WriteLibrary_VisualScenes
            List<Grendgine_Collada_Node> childNodes = new List<Grendgine_Collada_Node>();

            #region Create the node element for this nodeChunk
            Grendgine_Collada_Node tmpNode = new Grendgine_Collada_Node();
            // Check to see if there is a second model file, and if the mesh chunk is actually there.
            if (CryData.Models.Count > 1)
            {
                // Star Citizen pair.  Get the Node and Mesh chunks from the geometry file, unless it's a Proxy node.
                string nodeName = nodeChunk.Name;
                if (CryData.Models[1].NodeMap.ContainsKey(nodeName))        // make sure there is a geometry node in the geometry file
                {
                    ChunkNode geometryNode = CryData.Models[1].NodeMap.Values.Where(a => a.Name == nodeChunk.Name).First();
                    ChunkMesh geometryMesh = (ChunkMesh)CryData.Models[1].ChunkMap[geometryNode.ObjectNodeID];
                    tmpNode = CreateGeometryNode(geometryNode, geometryMesh);
                }
                else
                {
                    tmpNode = CreateSimpleNode(nodeChunk);
                }
            }
            else                    // Regular Cryengine file.
            {
                if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Mesh)
                {
                    ChunkMesh tmpMeshChunk = (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                    if (tmpMeshChunk.MeshSubsets == 0 || tmpMeshChunk.NumVertices == 0)  // Can have a node with a mesh and meshsubset, but no vertices.  Write as simple node.
                    {
                        // The mesh points to a meshphysics chunk.  Process as a simple node.
                        tmpNode = CreateSimpleNode(nodeChunk);
                    }
                    else
                    {
                        if (nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets].ID != 0)
                        {
                            tmpNode = CreateGeometryNode(nodeChunk, (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID]);
                        }
                        else
                        {
                            tmpNode = CreateSimpleNode(nodeChunk);
                        }
                    }
                }
                else
                {
                    tmpNode = CreateSimpleNode(nodeChunk);
                }
            }
            #endregion
            // Add childnodes
            tmpNode.node = CreateChildNodes(nodeChunk);
            return tmpNode;
        }

        /// <summary>
        /// This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics
        /// </summary>
        /// <param name="nodeChunk">The node chunk for this Collada Node.</param>
        /// <returns>Grendgine_Collada_Node for the node chunk</returns>
        private Grendgine_Collada_Node CreateSimpleNode(ChunkNode nodeChunk)
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
            CalculateTransform(nodeChunk);
            matrixString.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} {12:F6} {13:F6} {14:F6} {15:F6}",
                nodeChunk.LocalTransform.m11, nodeChunk.LocalTransform.m12, nodeChunk.LocalTransform.m13, nodeChunk.LocalTransform.m14,
                nodeChunk.LocalTransform.m21, nodeChunk.LocalTransform.m22, nodeChunk.LocalTransform.m23, nodeChunk.LocalTransform.m24,
                nodeChunk.LocalTransform.m31, nodeChunk.LocalTransform.m32, nodeChunk.LocalTransform.m33, nodeChunk.LocalTransform.m34,
                nodeChunk.LocalTransform.m41, nodeChunk.LocalTransform.m42, nodeChunk.LocalTransform.m43, nodeChunk.LocalTransform.m44);

            matrix.Value_As_String = matrixString.ToString();
            matrix.sID = "transform";
            matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
            tmpNode.Matrix = matrices.ToArray();

            // Add childnodes
            tmpNode.node = CreateChildNodes(nodeChunk);
            return tmpNode;
        }

        /// <summary>
        /// Used by CreateNode and CreateSimpleNodes to create all the child nodes for the given node.
        /// </summary>
        /// <param name="nodeChunk">Node with children to add.</param>
        /// <returns>A node with all the children added.</returns>
        private Grendgine_Collada_Node[] CreateChildNodes(ChunkNode nodeChunk)
        {
            if (nodeChunk.__NumChildren != 0)
            {
                List<Grendgine_Collada_Node> childNodes = new List<Grendgine_Collada_Node>();
                foreach (ChunkNode childNodeChunk in nodeChunk.AllChildNodes.ToList())
                {
                    Grendgine_Collada_Node childNode = new Grendgine_Collada_Node();
                    childNode = CreateNode(childNodeChunk);
                    childNodes.Add(childNode);
                }
                return childNodes.ToArray();
            }
            else
            {
                return null;
            }
        }

        private Grendgine_Collada_Node CreateJointNode(CompiledBone bone)
        {
            // This will be used recursively to create a node object and return it to WriteLibrary_VisualScenes
            // If this is the root bone, set the node id to Armature.  Otherwise set to armature_<bonename>
            string id = "Armature";
            if (bone.parentID != 0)
            {
                id += "_" + bone.boneName.Replace(' ', '_');
            }
            Grendgine_Collada_Node tmpNode = new Grendgine_Collada_Node()
            {
                ID = id,
                Name = bone.boneName.Replace(' ', '_'),
                sID = bone.boneName.Replace(' ', '_'),
                Type = Grendgine_Collada_Node_Type.JOINT
            };

            Grendgine_Collada_Matrix matrix = new Grendgine_Collada_Matrix();
            List<Grendgine_Collada_Matrix> matrices = new List<Grendgine_Collada_Matrix>();
            // Populate the matrix.  This is based on the BONETOWORLD data in this bone.
            StringBuilder matrixValues = new StringBuilder();
            //matrixValues.AppendFormat("\n\t\t\t{0,11:F6} {1,11:F6} {2,11:F6} {3,11:F6}\n\t\t\t{4,11:F6} {5,11:F6} {6,11:F6} {7,11:F6}\n\t\t\t{8,11:F6} {9,11:F6} {10,11:F6} {11,11:F6}\n\t\t\t0 0 0 1",
            matrixValues.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} 0 0 0 1",
                bone.LocalTransform.m11,
                bone.LocalTransform.m12,
                bone.LocalTransform.m13,
                bone.LocalTransform.m14,
                bone.LocalTransform.m21,
                bone.LocalTransform.m22,
                bone.LocalTransform.m23,
                bone.LocalTransform.m24,
                bone.LocalTransform.m31,
                bone.LocalTransform.m32,
                bone.LocalTransform.m33,
                bone.LocalTransform.m34
                );

            CleanNumbers(matrixValues);
            matrix.Value_As_String = matrixValues.ToString();
            matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
            tmpNode.Matrix = matrices.ToArray();

            // Recursively call this for each of the child bones to this bone.
            if (bone.numChildren > 0)
            {
                Grendgine_Collada_Node[] childNodes = new Grendgine_Collada_Node[bone.numChildren];
                int counter = 0;

                foreach (CompiledBone childBone in CryData.Bones.GetAllChildBones(bone))
                {
                    Grendgine_Collada_Node childNode = new Grendgine_Collada_Node();
                    childNode = CreateJointNode(childBone);
                    childNodes[counter] = childNode;
                    counter++;
                }
                tmpNode.node = childNodes;
            }
            return tmpNode;
        }

        private Grendgine_Collada_Node CreateGeometryNode(ChunkNode nodeChunk, ChunkMesh tmpMeshChunk)
        {
            Grendgine_Collada_Node tmpNode = new Grendgine_Collada_Node();
            ChunkMeshSubsets tmpMeshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node
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
            // Use same principle as CreateJointNode.  The Transform matrix (Matrix44) is the world transform matrix.
            CalculateTransform(nodeChunk);
            matrixString.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} {12:F6} {13:F6} {14:F6} {15:F6}",
                nodeChunk.LocalTransform.m11, nodeChunk.LocalTransform.m12, nodeChunk.LocalTransform.m13, nodeChunk.LocalTransform.m14,
                nodeChunk.LocalTransform.m21, nodeChunk.LocalTransform.m22, nodeChunk.LocalTransform.m23, nodeChunk.LocalTransform.m24,
                nodeChunk.LocalTransform.m31, nodeChunk.LocalTransform.m32, nodeChunk.LocalTransform.m33, nodeChunk.LocalTransform.m34,
                nodeChunk.LocalTransform.m41, nodeChunk.LocalTransform.m42, nodeChunk.LocalTransform.m43, nodeChunk.LocalTransform.m44);
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
            for (int i = 0; i < tmpMeshSubsets.NumMeshSubset; i++)
            {
                // For each mesh subset, we want to create an instance material and add it to instanceMaterials list.
                Grendgine_Collada_Instance_Material_Geometry tmpInstanceMat = new Grendgine_Collada_Instance_Material_Geometry();
                //tmpInstanceMat.Target = "#" + tmpMeshSubsets.MeshSubsets[i].MatID;
                if (CryData.Materials.Count() > 0)
                {
                    tmpInstanceMat.Target = "#" + CryData.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name + "-material";
                    //tmpInstanceMat.Symbol = CryData.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name;
                    tmpInstanceMat.Symbol = CryData.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name + "-material";
                }
                
                instanceMaterials.Add(tmpInstanceMat);
            }
            tmpNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material = instanceMaterials.ToArray();
            return tmpNode;
        }

        /// <summary>
        /// Creates the Collada Source element for a given datastream).
        /// </summary>
        /// <param name="vertices">The vertices of the source datastream.  This can be position, normals, colors, tangents, etc.</param>
        /// <param name="nodeChunk">The Node chunk of the datastream.  Need this for names, mesh, and submesh info.</param>
        /// <returns>Grendgine_Collada_Source object with the source data.</returns>
        private Grendgine_Collada_Source GetMeshSource(ChunkDataStream vertices, ChunkNode nodeChunk)
        {
            Grendgine_Collada_Source source = new Grendgine_Collada_Source();
            source.ID = nodeChunk.Name + "-mesh-pos";
            source.Name = nodeChunk.Name + "-pos";

            // TODO:  Implement this.

            return source;
        }

        /// <summary>
        /// Retrieves the worldtobone (bind pose matrix) for the bone.
        /// </summary>
        /// <param name="compiledBones">List of bones to get the BPM from.</param>
        /// <returns>The float_array that represents the BPM of all the bones, in order.</returns>
        private string GetBindPoseArray(List<CompiledBone> compiledBones)
        {
            StringBuilder value = new StringBuilder();
            for (int i = 0; i < compiledBones.Count; i++)
            {
                value.Append(CreateStringFromMatrix44(compiledBones[i].worldToBone.GetMatrix44()) + " ");
            }
            return value.ToString().TrimEnd();
        }

        private string GetRootBoneName(ChunkCompiledBones bones)
        {
            return bones.RootBone.boneName;
        }

        /// <summary>
        /// Adds the scene element to the Collada document.
        /// </summary>
        private void WriteScene()
        {
            Grendgine_Collada_Scene scene = new Grendgine_Collada_Scene();
            Grendgine_Collada_Instance_Visual_Scene visualScene = new Grendgine_Collada_Instance_Visual_Scene();
            visualScene.URL = "#Scene";
            visualScene.Name = "Scene";
            scene.Visual_Scene = visualScene;
            daeObject.Scene = scene;

        }

        private void CalculateTransform(ChunkNode node)
        {
            // Calculate the LocalTransform matrix.
            // Node transform matrices are different than joint.  Translation and scale are reversed.

            Vector3 localTranslation;
            Matrix33 localRotation;
            Vector3 localScale;

            //if (node.ParentNodeID != ~0)
            //{
            //localRotation = node.ParentNode.Transform.GetRotation().ConjugateTransposeThisAndMultiply(node.Transform.GetRotation());  // Might need to multiply by inverse instead.
            //localTranslation = node.LocalRotation * (node.LocalTranslation - node.ParentNode.Transform.GetScale());
            //localScale = node.LocalTranslation - node.ParentNode.LocalTranslation;
            /*localRotation = node.ParentNode.Transform.GetRotation().ConjugateTransposeThisAndMultiply(node.Transform.GetRotation());*/  // Might need to multiply by inverse instead.
                                                                                                                                          //localRotation = node.ParentNode.Transform.GetRotation().Inverse() * node.Transform.GetRotation();
                                                                                                                                          //localRotation = node.Transform.GetRotation();
                                                                                                                                          //localRotation = node.ParentNode.Transform.GetRotation() * node.Transform.GetRotation();
                                                                                                                                          //localTranslation = node.ParentNode.Transform.GetRotation() * node.Transform.GetScale();
                                                                                                                                          //localScale = node.Transform.GetTranslation() - node.ParentNode.Transform.GetTranslation();
                                                                                                                                          //}
                                                                                                                                          //else
                                                                                                                                          //{
            localTranslation = node.Transform.GetScale();
            localRotation = node.Transform.GetRotation();
            localScale = node.Transform.GetTranslation();
            //}

            //localTranslation = node.Transform.GetScale();
            //localRotation = node.RotSoFar;
            //localScale = node.TransformSoFar;

            node.LocalTranslation = localTranslation;
            node.LocalScale = localScale;
            node.LocalRotation = localRotation;
            node.LocalTransform = node.LocalTransform.GetTransformFromParts(localScale, localRotation, localTranslation);
        }

        #region XML Validation
        private void ValidateXml()  // For testing
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

        /// <summary>
        /// This method will check all the URLs used in the Collada object and see if any reference IDs that don't exist.  It will
        /// also check for duplicate IDs
        /// </summary>
        private void ValidateDoc()  // NYI
        {
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
        #endregion

        private string CreateStringFromMatrix44(Matrix44 matrix)
        {
            StringBuilder matrixValues = new StringBuilder();
            //matrixValues.AppendFormat("\n\t\t\t{0,11:F6} {1,11:F6} {2,11:F6} {3,11:F6}\n\t\t\t{4,11:F6} {5,11:F6} {6,11:F6} {7,11:F6}\n\t\t\t{8,11:F6} {9,11:F6} {10,11:F6} {11,11:F6}\n\t\t\t0 0 0 1",
            matrixValues.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} {12:F6} {13:F6} {14:F6} {15:F6}",
                matrix.m11,
                matrix.m12,
                matrix.m13,
                matrix.m14,
                matrix.m21,
                matrix.m22,
                matrix.m23,
                matrix.m24,
                matrix.m31,
                matrix.m32,
                matrix.m33,
                matrix.m34,
                matrix.m41,
                matrix.m42,
                matrix.m43,
                matrix.m44
                );
            CleanNumbers(matrixValues);
            return matrixValues.ToString();
        }

        /// <summary>Takes the Material file name and returns just the file name with no extension</summary>
        private string CleanMtlFileName(string cleanMe)
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
                result = cleanMe.Split(periodSep, StringSplitOptions.None);
                cleanMe = result[0];
                //Console.WriteLine("Cleanme is {0}", cleanMe);
            }
            return cleanMe;
        }

        private Double safe(Double value)
        {
            if (value == Double.NegativeInfinity)
                return Double.MinValue;

            if (value == Double.PositiveInfinity)
                return Double.MaxValue;

            if (value == Double.NaN)
                return 0;

            return value;
        }

        private void CleanNumbers(StringBuilder sb)
        {
            sb.Replace("0.000000", "0");
            sb.Replace("-0.000000", "0");
            sb.Replace("1.000000", "1");
            sb.Replace("-1.000000", "-1");
        }
        #endregion
    }
}