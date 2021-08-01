﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.CryEngineCore;
using static Extensions.FileHandlingExtensions;
using grendgine_collada;

namespace CgfConverter
{
    public class COLLADA : BaseRenderer // class to export to .dae format (COLLADA)
    {
        private readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
        private const string colladaVersion = "1.4.1";

        public Grendgine_Collada DaeObject { get; private set; } = new Grendgine_Collada();  // This is the serializable class.
        readonly XmlSerializer mySerializer = new XmlSerializer(typeof(Grendgine_Collada));

        public COLLADA(ArgsHandler argsHandler, CryEngine cryEngine) : base(argsHandler, cryEngine) { }

        public override void Render(string outputDir = null, bool preservePath = true)
        {
            GenerateDaeObject();

            // At this point, we should have a cryData.Asset object, fully populated.
            Utils.Log(LogLevelEnum.Debug);
            Utils.Log(LogLevelEnum.Debug, "*** Starting WriteCOLLADA() ***");
            Utils.Log(LogLevelEnum.Debug);

            // File name will be "object name.dae"
            var daeOutputFile = new FileInfo(GetOutputFile("dae", outputDir, preservePath));

            if (!daeOutputFile.Directory.Exists)
                daeOutputFile.Directory.Create();
            TextWriter writer = new StreamWriter(daeOutputFile.FullName);   // Makes the Textwriter object for the output
            mySerializer.Serialize(writer, DaeObject);                      // Serializes the daeObject and writes to the writer

            writer.Close();
            Utils.Log(LogLevelEnum.Debug, "End of Write Collada.  Export complete.");
        }

        public void GenerateDaeObject()
        {
            Utils.Log(LogLevelEnum.Debug, "Number of models: {0}", CryData.Models.Count);
            for (int i = 0; i < CryData.Models.Count; i++)
            {
                Utils.Log(LogLevelEnum.Debug, "\tNumber of nodes in model: {0}", CryData.Models[i].NodeMap.Count);
            }
            
            WriteRootNode(colladaVersion);
            WriteAsset();
            if (!Args.NoTextures)
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
                WriteLibrary_VisualScenes();
        }

        protected void WriteRootNode(string version)
        {
            // Blender doesn't like 1.5. :(
            if (version == "1.4.1")
                DaeObject.Collada_Version = "1.4.1";
            else if (version == "1.5.0")
                DaeObject.Collada_Version = "1.5.0";
        }

        protected void WriteAsset()
        {
            // Writes the Asset element in a Collada XML doc
            DateTime fileCreated = DateTime.Now;
            DateTime fileModified = DateTime.Now;           // since this only creates, both times should be the same
            Grendgine_Collada_Asset asset = new Grendgine_Collada_Asset
            {
                Revision = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };
            Grendgine_Collada_Asset_Contributor[] contributors = new Grendgine_Collada_Asset_Contributor[2];
            contributors[0] = new Grendgine_Collada_Asset_Contributor
            {
                Author = "Heffay",
                Author_Website = "https://github.com/Markemp/Cryengine-Converter",
                Author_Email = "markemp@gmail.com",
                Source_Data = CryData.RootNode.Name                    // The cgf/cga/skin/whatever file we read
            };
            // Get the actual file creators from the Source Chunk
            contributors[1] = new Grendgine_Collada_Asset_Contributor();
            foreach (ChunkSourceInfo tmpSource in CryData.Chunks.Where(a => a.ChunkType == ChunkType.SourceInfo))
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
            asset.Extra = new Grendgine_Collada_Extra[]
            {
                new Grendgine_Collada_Extra()
                {
                    Name = CryData.MtlFile ?? "",
                    ID = "material_file_name",
                    Type = "mtl_file"
                }
            };
            asset.Title = CryData.RootNode.Name;
            DaeObject.Asset = asset;
            DaeObject.Asset.Contributor = contributors;
        }

        protected void WriteLibrary_Images()
        {
            // List of all the images used by the asset.
            Grendgine_Collada_Library_Images libraryImages = new Grendgine_Collada_Library_Images();
            DaeObject.Library_Images = libraryImages;
            List<Grendgine_Collada_Image> imageList = new();
            
            for (int k = 0; k < CryData.Materials.Count; k++)
            {
                int numTextures = CryData.Materials[k].Textures.Length;
                for (int i = 0; i < numTextures; i++)
                {
                    // For each texture in the material, we make a new <image> object and add it to the list. 
                    Grendgine_Collada_Image tmpImage = new()
                    {
                        ID = CryData.Materials[k].Name + "_" + CryData.Materials[k].Textures[i].Map,
                        Name = CryData.Materials[k].Name + "_" + CryData.Materials[k].Textures[i].Map,
                        Init_From = new Grendgine_Collada_Init_From()
                    };
                    // Try to resolve the texture file to a file on disk
                    StringBuilder builder = new(ResolveTexFile(CryData.Materials[k].Textures[i].File, Args.DataDir));
                    
                    if (Args.PngTextures && File.Exists(builder.ToString().Replace(".dds", ".png")))
                        builder.Replace(".dds", ".png");
                    else if (Args.TgaTextures && File.Exists(builder.ToString().Replace(".dds", ".tga")))
                        builder.Replace(".dds", ".tga");
                    else if (Args.TiffTextures && File.Exists(builder.ToString().Replace(".dds", ".tif")))
                        builder.Replace(".dds", ".tif");
                    
                    if (Args.DataDir.ToString() != ".")
                        builder.Insert(0, "/");  // Path is absolute, preface with a "/"

                    builder.Replace(" ", @"%20");
                    // if 1.4.1, use URI.  If 1.5.0, use Ref.
                    _ = DaeObject.Collada_Version == "1.4.1" ? tmpImage.Init_From.Uri = builder.ToString() : tmpImage.Init_From.Ref = builder.ToString();
                    
                    imageList.Add(tmpImage);
                }
            }
            // images is the array of image (Gredgine_Collada_Image) objects
            Grendgine_Collada_Image[] images = imageList.ToArray();
            DaeObject.Library_Images.Image = images;
        }

        public void WriteLibrary_Materials()
        {
            // Create the list of materials used in this object
            // There is just one .mtl file we need to worry about.
            Grendgine_Collada_Library_Materials libraryMaterials = new Grendgine_Collada_Library_Materials();
            // We have our top level.
            DaeObject.Library_Materials = libraryMaterials;
            int numMaterials = CryData.Materials.Count;
            // Now create a material for each material in the object
            Utils.Log(LogLevelEnum.Info, "Number of materials: {0}", numMaterials);
            Grendgine_Collada_Material[] materials = new Grendgine_Collada_Material[numMaterials];
            for (int i = 0; i < numMaterials; i++)
            {
                Grendgine_Collada_Material tmpMaterial = new Grendgine_Collada_Material
                {
                    Instance_Effect = new Grendgine_Collada_Instance_Effect()
                };
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
                    string MatName = CryData.Materials[i].Name;
                    if (Args.PrefixMaterialNames)
                        MatName = CryData.Materials[i].SourceFileName + "_" + MatName;

                    tmpMaterial.Name = MatName;
                    tmpMaterial.ID = MatName + "-material";          // this is the order the materials appear in the .mtl file.  Needed for geometries.
                    tmpMaterial.Instance_Effect.URL = "#" + MatName + "-effect";
                }

                materials[i] = tmpMaterial;
            }
            libraryMaterials.Material = materials;
        }

        public void WriteLibrary_Effects()
        {
            // The Effects library.  This is actual material stuff.
            Grendgine_Collada_Library_Effects libraryEffects = new Grendgine_Collada_Library_Effects();
            // Like materials.  We will need one effect for each material.
            int numEffects = CryData.Materials.Count;
            Grendgine_Collada_Effect[] effects = new Grendgine_Collada_Effect[numEffects];
            for (int i = 0; i < numEffects; i++)
            {
                string MatName = CryData.Materials[i].Name;
                if (Args.PrefixMaterialNames)
                    MatName = CryData.Materials[i].SourceFileName + "_" + MatName;

                Grendgine_Collada_Effect tmpEffect = new Grendgine_Collada_Effect
                {
                    //tmpEffect.Name = CryData.Materials[i].Name;
                    ID = MatName + "-effect",
                    Name = MatName
                };
                effects[i] = tmpEffect;

                // create the profile_common for the effect
                List<Grendgine_Collada_Profile_COMMON> profiles = new List<Grendgine_Collada_Profile_COMMON>();
                Grendgine_Collada_Profile_COMMON profile = new Grendgine_Collada_Profile_COMMON();
                profiles.Add(profile);

                // Create a list for the new_params
                List<Grendgine_Collada_New_Param> newparams = new List<Grendgine_Collada_New_Param>();
                #region set up the sampler and surface for the materials. 
                // Check to see if the texture exists, and if so make a sampler and surface.
                for (int j = 0; j < CryData.Materials[i].Textures.Length; j++)
                {
                    // Add the Surface node
                    Grendgine_Collada_New_Param texSurface = new Grendgine_Collada_New_Param
                    {
                        sID = CleanTexFileName(CryData.Materials[i].Textures[j].File) + "-surface"
                    };
                    Grendgine_Collada_Surface surface = new Grendgine_Collada_Surface();
                    texSurface.Surface = surface;
                    surface.Init_From = new Grendgine_Collada_Init_From();
                    //Grendgine_Collada_Surface surface2D = new Grendgine_Collada_Surface();
                    //texSurface.sID = CleanName(CryData.Materials[i].Textures[j].File) + CryData.Materials[i].Textures[j].Map + "-surface";
                    texSurface.Surface.Type = "2D";
                    texSurface.Surface.Init_From = new Grendgine_Collada_Init_From
                    {
                        //texSurface.Surface.Init_From.Uri = CleanName(texture.File);
                        Uri = CryData.Materials[i].Name + "_" + CryData.Materials[i].Textures[j].Map
                    };

                    // Add the Sampler node
                    Grendgine_Collada_New_Param texSampler = new Grendgine_Collada_New_Param
                    {
                        sID = CleanTexFileName(CryData.Materials[i].Textures[j].File) + "-sampler"
                    };
                    Grendgine_Collada_Sampler2D sampler2D = new Grendgine_Collada_Sampler2D();
                    texSampler.Sampler2D = sampler2D;
                    //Grendgine_Collada_Source samplerSource = new Grendgine_Collada_Source();
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
                    if (texture.Map == CryEngineCore.Material.Texture.MapTypeEnum.Diffuse)
                    {
                        diffound = true;
                        phong.Diffuse.Texture = new Grendgine_Collada_Texture
                        {
                            // Texcoord is the ID of the UV source in geometries.  Not needed.
                            Texture = CleanTexFileName(texture.File) + "-sampler",
                            TexCoord = ""
                        };
                    }
                    if (texture.Map == CryEngineCore.Material.Texture.MapTypeEnum.Specular)
                    {
                        specfound = true;
                        phong.Specular.Texture = new Grendgine_Collada_Texture
                        {
                            Texture = CleanTexFileName(texture.File) + "-sampler",
                            TexCoord = ""
                        };

                    }
                    if (texture.Map == CryEngineCore.Material.Texture.MapTypeEnum.Bumpmap)
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

                        Grendgine_Collada_BumpMap bumpMap = new Grendgine_Collada_BumpMap
                        {
                            Textures = new Grendgine_Collada_Texture[1]
                        };
                        bumpMap.Textures[0] = new Grendgine_Collada_Texture
                        {
                            Texture = CleanTexFileName(texture.File) + "-sampler"
                        };
                        extraTechnique.Data = new XmlElement[1];
                        extraTechnique.Data[0] = bumpMap;
                    }
                }
                if (diffound == false)
                {
                    phong.Diffuse.Color = new Grendgine_Collada_Color
                    {
                        Value_As_String = CryData.Materials[i].__Diffuse?.Replace(",", " ") ?? string.Empty,
                        sID = "diffuse"
                    };
                }
                if (specfound == false)
                {
                    phong.Specular.Color = new Grendgine_Collada_Color
                    {
                        sID = "specular"
                    };
                    if (CryData.Materials[i].__Specular != null)
                        phong.Specular.Color.Value_As_String = CryData.Materials[i].__Specular?.Replace(",", " ") ?? string.Empty;
                    else
                        phong.Specular.Color.Value_As_String = "1 1 1";
                }

                phong.Emission = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type
                {
                    Color = new Grendgine_Collada_Color
                    {
                        sID = "emission",
                        Value_As_String = CryData.Materials[i].__Emissive?.Replace(",", " ") ?? string.Empty
                    }
                };
                phong.Shininess = new Grendgine_Collada_FX_Common_Float_Or_Param_Type
                {
                    Float = new Grendgine_Collada_SID_Float()
                };
                phong.Shininess.Float.sID = "shininess";
                phong.Shininess.Float.Value = (float)CryData.Materials[i].Shininess;
                phong.Index_Of_Refraction = new Grendgine_Collada_FX_Common_Float_Or_Param_Type
                {
                    Float = new Grendgine_Collada_SID_Float()
                };

                phong.Transparent = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type
                {
                    Color = new Grendgine_Collada_Color(),
                    Opaque = new Grendgine_Collada_FX_Opaque_Channel()
                };
                phong.Transparent.Color.Value_As_String = (1 - CryData.Materials[i].Opacity).ToString();  // Subtract from 1 for proper value.

                #endregion

                tmpEffect.Profile_COMMON = profiles.ToArray();
                profile.New_Param = new Grendgine_Collada_New_Param[newparams.Count];
                profile.New_Param = newparams.ToArray();
            }

            libraryEffects.Effect = effects;
            DaeObject.Library_Effects = libraryEffects;
            // libraryEffects contains a number of effects objects.  One effects object for each material.
        }

        /// <summary> Write the Library_Geometries element.  These won't be instantiated except through the visual scene or controllers. </summary>
        public void WriteLibrary_Geometries()
        {
            Grendgine_Collada_Library_Geometries libraryGeometries = new Grendgine_Collada_Library_Geometries();

            // Make a list for all the geometries objects we will need. Will convert to array at end.  Define the array here as well
            // We have to define a Geometry for EACH meshsubset in the meshsubsets, since the mesh can contain multiple materials
            List<Grendgine_Collada_Geometry> geometryList = new List<Grendgine_Collada_Geometry>();

            // For each of the nodes, we need to write the geometry.
            // Use a foreach statement to get all the node chunks.  This will get us the meshes, which will contain the vertex, UV and normal info.
            foreach (ChunkNode nodeChunk in CryData.Chunks.Where(a => a.ChunkType == ChunkType.Node))
            {
                // Create a geometry object.  Use the chunk ID for the geometry ID
                // Will have to be careful with this, since with .cga/.cgam pairs will need to match by Name.
                // Make the mesh object.  This will have 3 or 4 sources, 1 vertices, and 1 or more Triangles (with material ID)
                // If the Object ID of Node chunk points to a Helper or a Controller though, place an empty.
                // need to make a list of the sources and triangles to add to tmpGeo.Mesh
                ChunkDataStream tmpNormals = null;
                ChunkDataStream tmpUVs = null;
                ChunkDataStream tmpVertices = null;
                ChunkDataStream tmpVertsUVs = null;
                ChunkDataStream tmpIndices = null;
                ChunkDataStream tmpColors = null;
                ChunkDataStream tmpTangents = null;

                // Don't render shields if skip flag enabled
                if (Args.SkipShieldNodes && nodeChunk.Name.StartsWith("$shield"))
                {
                    Utils.Log(LogLevelEnum.Debug, "Skipped shields node {0}", nodeChunk.Name);
                    continue;
                }

                // Don't render proxies if skip flag enabled
                if (Args.SkipProxyNodes && nodeChunk.Name.StartsWith("proxy"))
                {
                    Utils.Log(LogLevelEnum.Debug, "Skipped proxy node {0}", nodeChunk.Name);
                    continue;
                }

                if (nodeChunk.ObjectChunk == null)
                {
                    Utils.Log(LogLevelEnum.Warning, "Skipped node with missing Object {0}", nodeChunk.Name);
                    continue;
                }

                if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkType.Mesh)
                {
                    // Get the mesh chunk and submesh chunk for this node.
                    ChunkMesh tmpMeshChunk = (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                    // Check to see if the Mesh points to a PhysicsData mesh.  Don't want to write these.
                    if (tmpMeshChunk.MeshPhysicsData != 0)
                    {
                        // TODO:  Implement this chunk
                    }
                    if (tmpMeshChunk.MeshSubsetsData != 0)   // For the SC files, you can have Mesh chunks with no Mesh Subset.  Need to skip these.  They are in the .cga file and contain no geometry.
                    {
                        ChunkMeshSubsets tmpMeshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsetsData];  // Listed as Object ID for the Node

                        if (tmpMeshChunk.VerticesData != 0)
                            tmpVertices = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.VerticesData];

                        if (tmpMeshChunk.NormalsData != 0)
                            tmpNormals = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.NormalsData];

                        if (tmpMeshChunk.UVsData != 0)
                            tmpUVs = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.UVsData];

                        if (tmpMeshChunk.VertsUVsData != 0)
                            tmpVertsUVs = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.VertsUVsData];

                        if (tmpMeshChunk.IndicesData != 0)
                            tmpIndices = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.IndicesData];

                        if (tmpMeshChunk.ColorsData != 0)
                            tmpColors = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.ColorsData];

                        if (tmpMeshChunk.TangentsData != 0)
                            tmpTangents = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.TangentsData];

                        if (tmpVertices == null && tmpVertsUVs == null) // There is no vertex data for this node.  Skip.
                            continue;

                        // tmpGeo is a Geometry object for each meshsubset.  Name will be "Nodechunk name_matID".  Hopefully there is only one matID used per submesh
                        Grendgine_Collada_Geometry tmpGeo = new()
                        {
                            Name = nodeChunk.Name,
                            ID = nodeChunk.Name + "-mesh"
                        };
                        Grendgine_Collada_Mesh tmpMesh = new();
                        tmpGeo.Mesh = tmpMesh;

                        // TODO:  Move the source creation to a separate function.  Too much retyping.
                        Grendgine_Collada_Source[] source = new Grendgine_Collada_Source[4];   // 4 possible source types.
                        Grendgine_Collada_Source posSource = new();
                        Grendgine_Collada_Source normSource = new();
                        Grendgine_Collada_Source uvSource = new();
                        Grendgine_Collada_Source colorSource = new();
                        source[0] = posSource;
                        source[1] = normSource;
                        source[2] = uvSource;
                        source[3] = colorSource;
                        posSource.ID = nodeChunk.Name + "-mesh-pos";
                        posSource.Name = nodeChunk.Name + "-pos";
                        normSource.ID = nodeChunk.Name + "-mesh-norm";
                        normSource.Name = nodeChunk.Name + "-norm";
                        uvSource.ID = nodeChunk.Name + "-mesh-UV"; 
                        uvSource.Name = nodeChunk.Name + "-UV";
                        colorSource.ID = nodeChunk.Name + "-mesh-color";
                        colorSource.Name = nodeChunk.Name + "-color";

                        #region Create vertices node.  For Triangles will just have VERTEX.
                        Grendgine_Collada_Vertices vertices = new();
                        vertices.ID = nodeChunk.Name + "-vertices";
                        tmpGeo.Mesh.Vertices = vertices;
                        Grendgine_Collada_Input_Unshared[] inputshared = new Grendgine_Collada_Input_Unshared[4];
                        Grendgine_Collada_Input_Unshared posInput = new()
                        {
                            Semantic = Grendgine_Collada_Input_Semantic.POSITION
                        };
                        vertices.Input = inputshared;
                        #endregion

                        Grendgine_Collada_Input_Unshared normInput = new();
                        Grendgine_Collada_Input_Unshared uvInput = new();
                        Grendgine_Collada_Input_Unshared colorInput = new();

                        normInput.Semantic = Grendgine_Collada_Input_Semantic.NORMAL;
                        uvInput.Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD;
                        colorInput.Semantic = Grendgine_Collada_Input_Semantic.COLOR;

                        posInput.source = "#" + posSource.ID;
                        normInput.source = "#" + normSource.ID;
                        uvInput.source = "#" + uvSource.ID;
                        colorInput.source = "#" + colorSource.ID;
                        inputshared[0] = posInput;

                        Grendgine_Collada_Float_Array floatArrayVerts = new();
                        Grendgine_Collada_Float_Array floatArrayNormals = new();
                        Grendgine_Collada_Float_Array floatArrayUVs = new();
                        Grendgine_Collada_Float_Array floatArrayColors = new();
                        Grendgine_Collada_Float_Array floatArrayTangents = new();

                        StringBuilder vertString = new();
                        StringBuilder normString = new();
                        StringBuilder uvString = new();
                        StringBuilder colorString = new();

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
                            if (tmpNormals != null)
                                floatArrayNormals.Count = (int)tmpNormals.NumElements * 3;
                            floatArrayColors.ID = colorSource.ID + "-array";
                            floatArrayColors.Digits = 6;
                            floatArrayColors.Magnitude = 38;
                            if (tmpColors != null)
                            {
                                floatArrayColors.Count = (int)tmpColors.NumElements * 4;
                                for (uint j = 0; j < tmpColors.NumElements; j++)  // Create Colors string
                                {
                                    colorString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} {3:F6} ",
                                        tmpColors.Colors[j].r / 255.0,
                                        tmpColors.Colors[j].g / 255.0,
                                        tmpColors.Colors[j].b / 255.0,
                                        tmpColors.Colors[j].a / 255.0);
                                }
                            }

                            // Create Vertices and normals string
                            for (uint j = 0; j < tmpMeshChunk.NumVertices; j++)
                            {
                                Vector3 vertex = (tmpVertices.Vertices[j]);
                                vertString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} ", vertex.X, vertex.Y, vertex.Z);
                                Vector3 normal = tmpNormals?.Normals[j] ?? tmpTangents?.Normals[j] ?? new Vector3(0.0f, 0.0f, 0.0f);
                                normString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} ", safe(normal.X), safe(normal.Y), safe(normal.Z));
                            }
                            for (uint j = 0; j < tmpUVs.NumElements; j++)     // Create UV string
                            {
                                uvString.AppendFormat(culture, "{0:F6} {1:F6} ", safe(tmpUVs.UVs[j].U), 1 - safe(tmpUVs.UVs[j].V));
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
                            floatArrayColors.ID = colorSource.ID + "-array";
                            floatArrayColors.Digits = 6;
                            floatArrayColors.Magnitude = 38;
                            if (tmpVertsUVs.Colors != null)
                            {
                                floatArrayColors.Count = tmpVertsUVs.Colors.Length * 4;
                                for (uint j = 0; j < tmpVertsUVs.Colors.Length; j++)  // Create Colors string
                                {
                                    colorString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} {3:F6} ",
                                        tmpVertsUVs.Colors[j].r / 255.0,
                                        tmpVertsUVs.Colors[j].g / 255.0,
                                        tmpVertsUVs.Colors[j].b / 255.0,
                                        tmpVertsUVs.Colors[j].a / 255.0);
                                }
                            }

                            // Create Vertices, normals and colors string
                            for (uint j = 0; j < tmpMeshChunk.NumVertices; j++)
                            {
                                // Rotate/translate the vertex
                                // Dymek's code to rescale by bounding box.  Only apply to geometry (cga or cgf), and not skin or chr objects.
                                if (!CryData.InputFile.EndsWith("skin") && !CryData.InputFile.EndsWith("chr"))
                                {
                                    float multiplerX = Math.Abs(tmpMeshChunk.MinBound.X - tmpMeshChunk.MaxBound.X) / 2f;
                                    float multiplerY = Math.Abs(tmpMeshChunk.MinBound.Y - tmpMeshChunk.MaxBound.Y) / 2f;
                                    float multiplerZ = Math.Abs(tmpMeshChunk.MinBound.Z - tmpMeshChunk.MaxBound.Z) / 2f;
                                    if (multiplerX < 1) { multiplerX = 1; }
                                    if (multiplerY < 1) { multiplerY = 1; }
                                    if (multiplerZ < 1) { multiplerZ = 1; }
                                    tmpVertsUVs.Vertices[j].X = tmpVertsUVs.Vertices[j].X * multiplerX + (tmpMeshChunk.MaxBound.X + tmpMeshChunk.MinBound.X) / 2;
                                    tmpVertsUVs.Vertices[j].Y = tmpVertsUVs.Vertices[j].Y * multiplerY + (tmpMeshChunk.MaxBound.Y + tmpMeshChunk.MinBound.Y) / 2;
                                    tmpVertsUVs.Vertices[j].Z = tmpVertsUVs.Vertices[j].Z * multiplerZ + (tmpMeshChunk.MaxBound.Z + tmpMeshChunk.MinBound.Z) / 2;
                                }

                                Vector3 vertex = tmpVertsUVs.Vertices[j];
                                vertString.AppendFormat("{0:F6} {1:F6} {2:F6} ", vertex.X, vertex.Y, vertex.Z);

                                // TODO:  This isn't right?  VertsUvs may always have color as the 3rd element.
                                // Normals depend on the data size.  16 byte structures have the normals in the Tangents.  20 byte structures are in the VertsUV.
                                Vector3 normal = new(); 
                                if (tmpVertsUVs.Normals != null)
                                {
                                    normal = tmpVertsUVs.Normals[j];
                                }
                                else if (tmpTangents != null && tmpTangents.Normals != null)
                                {
                                    normal = tmpTangents.Normals[j];
                                }
                                normString.AppendFormat("{0:F6} {1:F6} {2:F6} ", safe(normal.X), safe(normal.Y), safe(normal.Z));
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
                        CleanNumbers(colorString);

                        #region Create the triangles node.
                        Grendgine_Collada_Triangles[] triangles = new Grendgine_Collada_Triangles[tmpMeshSubsets.NumMeshSubset];
                        tmpGeo.Mesh.Triangles = triangles;

                        for (uint j = 0; j < tmpMeshSubsets.NumMeshSubset; j++) // Need to make a new Triangles entry for each submesh.
                        {
                            triangles[j] = new Grendgine_Collada_Triangles
                            {
                                Count = tmpMeshSubsets.MeshSubsets[j].NumIndices / 3
                            };

                            if (CryData.Materials.Count != 0)
                            {
                                if (tmpMeshSubsets.MeshSubsets[j].MatID > CryData.Materials.Count - 1)
                                {
                                    //TODO:  This isn't right, but prevents crashing. Looks like for some
                                    // models it's the index - 8 (some Sonic Boom for example)
                                    tmpMeshSubsets.MeshSubsets[j].MatID = 0;  
                                }
                                string MatName = CryData.Materials[tmpMeshSubsets.MeshSubsets[j].MatID].Name;
                                if (Args.PrefixMaterialNames)
                                    MatName = CryData.Materials[tmpMeshSubsets.MeshSubsets[j].MatID].SourceFileName + "_" + MatName;

                                triangles[j].Material = MatName + "-material";
                            }
                            // Create the inputs.  vertex, normal, texcoord, color
                            int inputCount = 3;
                            if (tmpColors != null || tmpVertsUVs?.Colors != null)
                            {
                                inputCount++;
                            }
                            triangles[j].Input = new Grendgine_Collada_Input_Shared[inputCount];

                            triangles[j].Input[0] = new Grendgine_Collada_Input_Shared
                            {
                                Semantic = new Grendgine_Collada_Input_Semantic()
                            };
                            triangles[j].Input[0].Semantic = Grendgine_Collada_Input_Semantic.VERTEX;
                            triangles[j].Input[0].Offset = 0;
                            triangles[j].Input[0].source = "#" + vertices.ID;
                            triangles[j].Input[1] = new Grendgine_Collada_Input_Shared
                            {
                                Semantic = Grendgine_Collada_Input_Semantic.NORMAL,
                                Offset = 1,
                                source = "#" + normSource.ID
                            };
                            triangles[j].Input[2] = new Grendgine_Collada_Input_Shared
                            {
                                Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD,
                                Offset = 2,
                                source = "#" + uvSource.ID
                            };

                            int nextInputID = 3;
                            if (tmpColors != null || tmpVertsUVs?.Colors != null)
                            {
                                triangles[j].Input[nextInputID] = new Grendgine_Collada_Input_Shared
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.COLOR,
                                    Offset = nextInputID,
                                    source = "#" + colorSource.ID
                                };
                                nextInputID++;
                            }

                            // Create the vcount list.  All triangles, so the subset number of indices.
                            StringBuilder vc = new StringBuilder();
                            for (var k = tmpMeshSubsets.MeshSubsets[j].FirstIndex; k < (tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices); k++)
                            {
                                int ccount = 3;

                                if (tmpColors != null || tmpVertsUVs?.Colors != null)
                                    ccount++;

                                vc.AppendFormat(culture, String.Format("{0} ", ccount));
                                k += 2;
                            }

                            // Create the P node for the Triangles.
                            StringBuilder p = new StringBuilder();
                            for (var k = tmpMeshSubsets.MeshSubsets[j].FirstIndex; k < (tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices); k++)
                            {
                                int values = 0;
                                if (tmpColors != null || tmpVertsUVs?.Colors != null)
                                {
                                    values++;
                                }

                                List<string> formatlist = new List<string>();
                                formatlist.Add("{0} {0} {0} ");
                                formatlist.Add("{1} {1} {1} ");
                                formatlist.Add("{2} {2} {2} ");
                                for (var valuecount = 0; valuecount < values; valuecount++)
                                {
                                    formatlist[0] += "{0} ";
                                    formatlist[1] += "{1} ";
                                    formatlist[2] += "{2} ";
                                }
                                string finalformat = String.Join("",formatlist);
                                p.AppendFormat(finalformat, tmpIndices.Indices[k], tmpIndices.Indices[k + 1], tmpIndices.Indices[k + 2]);
                                k += 2;
                            }
                            triangles[j].P = new Grendgine_Collada_Int_Array_String
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

                        source[0].Float_Array = floatArrayVerts;
                        source[1].Float_Array = floatArrayNormals;
                        source[2].Float_Array = floatArrayUVs;
                        source[3].Float_Array = floatArrayColors;
                        tmpGeo.Mesh.Source = source;

                        // create the technique_common for each of these
                        posSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
                        {
                            Accessor = new Grendgine_Collada_Accessor()
                        };
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

                        normSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
                        {
                            Accessor = new Grendgine_Collada_Accessor
                            {
                                Source = "#" + floatArrayNormals.ID,
                                Stride = 3,
                                Count = (uint)tmpMeshChunk.NumVertices
                            }
                        };
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

                        uvSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
                        {
                            Accessor = new Grendgine_Collada_Accessor
                            {
                                Source = "#" + floatArrayUVs.ID,
                                Stride = 2
                            }
                        };

                        if (tmpVertices != null)
                            uvSource.Technique_Common.Accessor.Count = tmpUVs.NumElements;
                        else
                            uvSource.Technique_Common.Accessor.Count = tmpVertsUVs.NumElements;

                        Grendgine_Collada_Param[] paramUV = new Grendgine_Collada_Param[2];
                        paramUV[0] = new Grendgine_Collada_Param();
                        paramUV[1] = new Grendgine_Collada_Param();
                        paramUV[0].Name = "S";
                        paramUV[0].Type = "float";
                        paramUV[1].Name = "T";
                        paramUV[1].Type = "float";
                        uvSource.Technique_Common.Accessor.Param = paramUV;

                        if (tmpColors != null || tmpVertsUVs?.Colors != null)
                        {
                            uint numberOfElements;
                            if (tmpColors != null)
                                numberOfElements = tmpColors.NumElements;
                            else
                                numberOfElements = (uint)tmpVertsUVs.Colors.Length;

                            colorSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                            };
                            colorSource.Technique_Common.Accessor.Source = "#" + floatArrayColors.ID;
                            colorSource.Technique_Common.Accessor.Stride = 4;
                            colorSource.Technique_Common.Accessor.Count = numberOfElements;
                            Grendgine_Collada_Param[] paramColor = new Grendgine_Collada_Param[4];
                            paramColor[0] = new Grendgine_Collada_Param();
                            paramColor[1] = new Grendgine_Collada_Param();
                            paramColor[2] = new Grendgine_Collada_Param();
                            paramColor[3] = new Grendgine_Collada_Param();
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

                        geometryList.Add(tmpGeo);

                        #endregion
                    }
                }
                // There is no geometry for a helper or controller node.  Can skip the rest.
            }
            libraryGeometries.Geometry = geometryList.ToArray();
            DaeObject.Library_Geometries = libraryGeometries;
        }

        public void WriteLibrary_Controllers()
        {
            if (DaeObject.Library_Geometries.Geometry.Length != 0)
            {
                // Set up the controller library.
                Grendgine_Collada_Library_Controllers libraryController = new Grendgine_Collada_Library_Controllers();

                // There can be multiple controllers in the controller library.  But for Cryengine files, there is only one rig.
                // So if a rig exists, make that the controller.  This applies mostly to .chr files, which will have a rig and may have geometry.
                Grendgine_Collada_Controller controller = new Grendgine_Collada_Controller
                {
                    ID = "Controller"
                };          // just need the one.
                // Create the skin object and assign to the controller
                Grendgine_Collada_Skin skin = new Grendgine_Collada_Skin
                {
                    source = "#" + DaeObject.Library_Geometries.Geometry[0].ID,
                    Bind_Shape_Matrix = new Grendgine_Collada_Float_Array_String()
                };
                skin.Bind_Shape_Matrix.Value_As_String = CreateStringFromMatrix4x4(Matrix4x4.Identity);  // We will assume the BSM is the identity matrix for now
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
                jointsSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
                {
                    Accessor = new Grendgine_Collada_Accessor
                    {
                        Source = "#Controller-joints-array",
                        Count = (uint)CryData.SkinningInfo.CompiledBones.Count,
                        Stride = 1
                    }
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
                accessor.Param[0] = new Grendgine_Collada_Param
                {
                    Name = "WEIGHT",
                    Type = "float"
                };
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
                vertexWeights.VCount = new Grendgine_Collada_Int_Array_String
                {
                    Value_As_String = vCount.ToString().TrimEnd()
                };
                StringBuilder vertices = new StringBuilder();
                //for (int i = 0; i < CryData.Models[0].SkinningInfo.IntVertices.Count * 4; i++)
                int index = 0;
                if (!CryData.Models[0].SkinningInfo.HasIntToExtMapping)
                {
                    for (int i = 0; i < CryData.SkinningInfo.BoneMapping.Count; i++)
                    {
                        int wholePart = (int)i / 4;
                        vertices.Append(CryData.SkinningInfo.BoneMapping[i].BoneIndex[0] + " " + index + " ");
                        vertices.Append(CryData.SkinningInfo.BoneMapping[i].BoneIndex[1] + " " + (index + 1) + " ");
                        vertices.Append(CryData.SkinningInfo.BoneMapping[i].BoneIndex[2] + " " + (index + 2) + " ");
                        vertices.Append(CryData.SkinningInfo.BoneMapping[i].BoneIndex[3] + " " + (index + 3) + " ");
                        index += 4;
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

                        index += 4;
                    }
                }
                vertexWeights.V = new Grendgine_Collada_Int_Array_String
                {
                    Value_As_String = vertices.ToString().TrimEnd()
                };
                #endregion

                // create the extra element for the FCOLLADA profile
                controller.Extra = new Grendgine_Collada_Extra[1];
                controller.Extra[0] = new Grendgine_Collada_Extra
                {
                    Technique = new Grendgine_Collada_Technique[1]
                };
                controller.Extra[0].Technique[0] = new Grendgine_Collada_Technique
                {
                    profile = "FCOLLADA",
                    UserProperties = "SkinController"
                };


                // Add the parts to their parents
                controller.Skin = skin;
                libraryController.Controller = new Grendgine_Collada_Controller[1];
                libraryController.Controller[0] = controller;
                DaeObject.Library_Controllers = libraryController;
            }
        }

        /// <summary> Provides a library in which to place visual_scene elements. </summary>
        public void WriteLibrary_VisualScenes()
        {
            // Set up the library
            Grendgine_Collada_Library_Visual_Scenes libraryVisualScenes = new Grendgine_Collada_Library_Visual_Scenes();

            // There can be multiple visual scenes.  Will just have one (World) for now.  All node chunks go under Nodes for that visual scene
            List<Grendgine_Collada_Visual_Scene> visualScenes = new List<Grendgine_Collada_Visual_Scene>();
            List<Grendgine_Collada_Node> nodes = new List<Grendgine_Collada_Node>();

            // Check to see if there is a CompiledBones chunk.  If so, add a Node.
            if (CryData.Chunks.Any(a => a.ChunkType == ChunkType.CompiledBones || 
                a.ChunkType == ChunkType.CompiledBonesSC ||
                a.ChunkType == ChunkType.CompiledBonesIvo))
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
            DaeObject.Library_Visual_Scene = libraryVisualScenes;
        }

        /// <summary> Provides a library in which to place visual_scene elements for chr files (rigs + geometry). </summary>
        public void WriteLibrary_VisualScenesWithSkeleton()
        {
            // Set up the library
            Grendgine_Collada_Library_Visual_Scenes libraryVisualScenes = new Grendgine_Collada_Library_Visual_Scenes();

            // There can be multiple visual scenes.  Will just have one (World) for now.  All node chunks go under Nodes for that visual scene
            List<Grendgine_Collada_Visual_Scene> visualScenes = new List<Grendgine_Collada_Visual_Scene>();
            List<Grendgine_Collada_Node> nodes = new List<Grendgine_Collada_Node>();

            // Check to see if there is a CompiledBones chunk.  If so, add a Node.  
            if (CryData.Chunks.Any(a => a.ChunkType == ChunkType.CompiledBones || 
                a.ChunkType == ChunkType.CompiledBonesSC ||
                a.ChunkType == ChunkType.CompiledBonesIvo))
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
                Value_As_String = CreateStringFromMatrix4x4(Matrix4x4.Identity)
            };
            rootNode.Instance_Controller = new Grendgine_Collada_Instance_Controller[1];
            rootNode.Instance_Controller[0] = new Grendgine_Collada_Instance_Controller
            {
                URL = "#Controller",
                Skeleton = new Grendgine_Collada_Skeleton[1]
            };
            Grendgine_Collada_Skeleton skeleton = rootNode.Instance_Controller[0].Skeleton[0] = new Grendgine_Collada_Skeleton();
            skeleton.Value = "#Armature";
            rootNode.Instance_Controller[0].Bind_Material = new Grendgine_Collada_Bind_Material[1];
            Grendgine_Collada_Bind_Material bindMaterial = rootNode.Instance_Controller[0].Bind_Material[0] = new Grendgine_Collada_Bind_Material();

            // Create an Instance_Material for each material
            bindMaterial.Technique_Common = new Grendgine_Collada_Technique_Common_Bind_Material();
            List<Grendgine_Collada_Instance_Material_Geometry> instanceMaterials = new List<Grendgine_Collada_Instance_Material_Geometry>();
            bindMaterial.Technique_Common.Instance_Material = new Grendgine_Collada_Instance_Material_Geometry[CryData.Materials.Count];
            // This gets complicated.  We need to make one instance_material for each material used in this node chunk.  The mat IDs used in this
            // node chunk are stored in meshsubsets, so for each subset we need to grab the mat, get the target (id), and make an instance_material for it.
            for (int i = 0; i < CryData.Materials.Count; i++)
            {
                string MatName = CryData.Materials[i].Name;
                if (Args.PrefixMaterialNames)
                    MatName = CryData.Materials[i].SourceFileName + "_" + MatName;

                // For each mesh subset, we want to create an instance material and add it to instanceMaterials list.
                Grendgine_Collada_Instance_Material_Geometry tmpInstanceMat = new Grendgine_Collada_Instance_Material_Geometry
                {
                    //tmpInstanceMat.Target = "#" + tmpMeshSubsets.MeshSubsets[i].MatID;
                    Target = "#" + MatName + "-material",
                    //tmpInstanceMat.Symbol = CryData.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name;
                    Symbol = MatName + "-material"
                };
                instanceMaterials.Add(tmpInstanceMat);
            }
            rootNode.Instance_Controller[0].Bind_Material[0].Technique_Common.Instance_Material = instanceMaterials.ToArray();

            nodes.Add(rootNode);
            visualScene.Node = nodes.ToArray();
            visualScene.ID = "Scene";
            visualScenes.Add(visualScene);

            libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
            DaeObject.Library_Visual_Scene = libraryVisualScenes;
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
                int nodeID = nodeChunk.ID;

                // make sure there is a geometry node in the geometry file
                if (CryData.Models[0].ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkType.Helper)
                {
                    tmpNode = CreateSimpleNode(nodeChunk);
                }
                else
                {
                    ChunkNode geometryNode = CryData.Models[1].NodeMap.Values.Where(a => a.Name == nodeChunk.Name).FirstOrDefault();
                    if (geometryNode == null)
                    {
                        tmpNode = CreateSimpleNode(nodeChunk);  // Can't find geometry for given node.
                    }
                    else
                    {
                        ChunkMesh geometryMesh = (ChunkMesh)CryData.Models[1].ChunkMap[geometryNode.ObjectNodeID];
                        tmpNode = CreateGeometryNode(geometryNode, geometryMesh);
                    }
                }
            }
            else                    // Regular Cryengine file.
            {
                if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkType.Mesh)
                {
                    ChunkMesh tmpMeshChunk = (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                    if (tmpMeshChunk.MeshSubsetsData == 0 || tmpMeshChunk.NumVertices == 0)  // Can have a node with a mesh and meshsubset, but no vertices.  Write as simple node.
                    {
                        // The mesh points to a meshphysics chunk.  Process as a simple node.
                        tmpNode = CreateSimpleNode(nodeChunk);
                    }
                    else
                    {
                        if (nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsetsData].ID != 0)
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

        /// <summary>This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics </summary>
        private Grendgine_Collada_Node CreateSimpleNode(ChunkNode nodeChunk)
        {
            // This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics
            Grendgine_Collada_Node_Type nodeType = Grendgine_Collada_Node_Type.NODE;
            Grendgine_Collada_Node tmpNode = new Grendgine_Collada_Node
            {
                Type = nodeType,
                Name = nodeChunk.Name,
                ID = nodeChunk.Name
            };
            // Make the lists necessary for this Node.
            List<Grendgine_Collada_Matrix> matrices = new List<Grendgine_Collada_Matrix>();

            Grendgine_Collada_Matrix matrix = new Grendgine_Collada_Matrix();
            StringBuilder matrixString = new StringBuilder();
            matrixString.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} {12:F6} {13:F6} {14:F6} {15:F6}",
                nodeChunk.LocalTransform.M11, nodeChunk.LocalTransform.M12, nodeChunk.LocalTransform.M13, nodeChunk.LocalTransform.M14,
                nodeChunk.LocalTransform.M21, nodeChunk.LocalTransform.M22, nodeChunk.LocalTransform.M23, nodeChunk.LocalTransform.M24,
                nodeChunk.LocalTransform.M31, nodeChunk.LocalTransform.M32, nodeChunk.LocalTransform.M33, nodeChunk.LocalTransform.M34,
                nodeChunk.LocalTransform.M41, nodeChunk.LocalTransform.M42, nodeChunk.LocalTransform.M43, nodeChunk.LocalTransform.M44);

            matrix.Value_As_String = matrixString.ToString();
            matrix.sID = "transform";
            matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
            tmpNode.Matrix = matrices.ToArray();

            // Add childnodes
            tmpNode.node = CreateChildNodes(nodeChunk);
            return tmpNode;
        }

        /// <summary>Used by CreateNode and CreateSimpleNodes to create all the child nodes for the given node.</summary>
        private Grendgine_Collada_Node[] CreateChildNodes(ChunkNode nodeChunk)
        {
            if (nodeChunk.__NumChildren != 0)
            {
                List<Grendgine_Collada_Node> childNodes = new List<Grendgine_Collada_Node>();
                foreach (ChunkNode childNodeChunk in nodeChunk.AllChildNodes.ToList())
                {
                    Grendgine_Collada_Node childNode = CreateNode(childNodeChunk); ;
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
            var localTransform = bone.LocalTransform;
            matrixValues.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} 0 0 0 1",
                localTransform.M11, localTransform.M12, localTransform.M13, localTransform.M14,
                localTransform.M21, localTransform.M22, localTransform.M23, localTransform.M24,
                localTransform.M31, localTransform.M32, localTransform.M33, localTransform.M34);

            CleanNumbers(matrixValues);
            matrix.Value_As_String = matrixValues.ToString();
            matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
            tmpNode.Matrix = matrices.ToArray();

            // Recursively call this for each of the child bones to this bone.
            if (bone.childIDs.Count > 0)
            {
                Grendgine_Collada_Node[] childNodes = new Grendgine_Collada_Node[bone.childIDs.Count];
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
            ChunkMeshSubsets tmpMeshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsetsData];  // Listed as Object ID for the Node
            Grendgine_Collada_Node_Type nodeType = Grendgine_Collada_Node_Type.NODE; 
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
            matrixString.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} {12:F6} {13:F6} {14:F6} {15:F6}",
                nodeChunk.LocalTransform.M11, nodeChunk.LocalTransform.M12, nodeChunk.LocalTransform.M13, nodeChunk.LocalTransform.M14,
                nodeChunk.LocalTransform.M21, nodeChunk.LocalTransform.M22, nodeChunk.LocalTransform.M23, nodeChunk.LocalTransform.M24,
                nodeChunk.LocalTransform.M31, nodeChunk.LocalTransform.M32, nodeChunk.LocalTransform.M33, nodeChunk.LocalTransform.M34,
                nodeChunk.LocalTransform.M41, nodeChunk.LocalTransform.M42, nodeChunk.LocalTransform.M43, nodeChunk.LocalTransform.M44);
            matrix.Value_As_String = matrixString.ToString();
            matrix.sID = "transform";
            matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
            tmpNode.Matrix = matrices.ToArray();

            // Each node will have one instance geometry, although it could be a list.
            Grendgine_Collada_Instance_Geometry instanceGeometry = new Grendgine_Collada_Instance_Geometry
            {
                Name = nodeChunk.Name,
                URL = "#" + nodeChunk.Name + "-mesh"  // this is the ID of the geometry.
            };

            Grendgine_Collada_Bind_Material bindMaterial = new Grendgine_Collada_Bind_Material
            {
                Technique_Common = new Grendgine_Collada_Technique_Common_Bind_Material
                {
                    Instance_Material = new Grendgine_Collada_Instance_Material_Geometry[tmpMeshSubsets.NumMeshSubset]
                }
            };
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
                if (CryData.Materials.Count > 0)
                {
                    string MatName = CryData.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name;
                    if (Args.PrefixMaterialNames)
                        MatName = CryData.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].SourceFileName + "_" + MatName;

                    tmpInstanceMat.Target = "#" + MatName + "-material";
                    //tmpInstanceMat.Symbol = MatName;
                    tmpInstanceMat.Symbol = MatName + "-material";
                }

                instanceMaterials.Add(tmpInstanceMat);
            }
            tmpNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material = instanceMaterials.ToArray();
            return tmpNode;
        }

        /// <summary>Creates the Collada Source element for a given datastream).</summary>
        private Grendgine_Collada_Source GetMeshSource(ChunkDataStream vertices, ChunkNode nodeChunk)
        {
            Grendgine_Collada_Source source = new Grendgine_Collada_Source
            {
                ID = nodeChunk.Name + "-mesh-pos",
                Name = nodeChunk.Name + "-pos"
            };

            // TODO:  Implement this.

            return source;
        }

        /// <summary>Retrieves the worldtobone (bind pose matrix) for the bone.</summary>
        private string GetBindPoseArray(List<CompiledBone> compiledBones)
        {
            StringBuilder value = new StringBuilder();
            for (int i = 0; i < compiledBones.Count; i++)
            {
                value.Append(CreateStringFromMatrix4x4(compiledBones[i].BindPoseMatrix) + " ");
            }
            return value.ToString().TrimEnd();
        }

        /// <summary> Adds the scene element to the Collada document. </summary>
        private void WriteScene()
        {
            Grendgine_Collada_Scene scene = new Grendgine_Collada_Scene();
            Grendgine_Collada_Instance_Visual_Scene visualScene = new Grendgine_Collada_Instance_Visual_Scene
            {
                URL = "#Scene",
                Name = "Scene"
            };
            scene.Visual_Scene = visualScene;
            DaeObject.Scene = scene;

        }

        private static string CreateStringFromMatrix4x4(Matrix4x4 matrix)
        {
            StringBuilder matrixValues = new StringBuilder();
            matrixValues.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} {12:F6} {13:F6} {14:F6} {15:F6}",
                matrix.M11,
                matrix.M12,
                matrix.M13,
                matrix.M14,
                matrix.M21,
                matrix.M22,
                matrix.M23,
                matrix.M24,
                matrix.M31,
                matrix.M32,
                matrix.M33,
                matrix.M34,
                matrix.M41,
                matrix.M42,
                matrix.M43,
                matrix.M44
                );
            CleanNumbers(matrixValues);
            return matrixValues.ToString();
        }
        
        private static double safe(double value)
        {
            if (value == double.NegativeInfinity)
                return double.MinValue;

            if (value == double.PositiveInfinity)
                return double.MaxValue;

            if (double.IsNaN(value))
                return 0;

            return value;
        }

        private static void CleanNumbers(StringBuilder sb)
        {
            sb.Replace("0.000000", "0");
            sb.Replace("-0.000000", "0");
            sb.Replace("1.000000", "1");
            sb.Replace("-1.000000", "-1");
        }
        #endregion
    }
}