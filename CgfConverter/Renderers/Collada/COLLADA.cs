using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using CgfConverter.CryEngineCore;
using static Extensions.FileHandlingExtensions;
using grendgine_collada;
using CgfConverter.Materials;
using static System.Net.Mime.MediaTypeNames;

namespace CgfConverter;

public class Collada : BaseRenderer
{
    private readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
    private const string colladaVersion = "1.4.1";

    public Grendgine_Collada DaeObject { get; private set; } = new Grendgine_Collada();  // This is the serializable class.
    readonly XmlSerializer mySerializer = new(typeof(Grendgine_Collada));

    public Collada(ArgsHandler argsHandler, CryEngine cryEngine) : base(argsHandler, cryEngine) { }

    public override void Render(string outputDir = null, bool preservePath = true)
    {
        GenerateDaeObject();

        // At this point, we should have a cryData.Asset object, fully populated.
        Utils.Log(LogLevelEnum.Debug);
        Utils.Log(LogLevelEnum.Debug, "*** Starting WriteCOLLADA() ***");
        Utils.Log(LogLevelEnum.Debug);

        // File name will be "<object name>.dae"
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
        WriteScene();

        // Create Material, Effects and Textures libraries
        Grendgine_Collada_Library_Materials libraryMaterials = new();
        DaeObject.Library_Materials = libraryMaterials;
        Grendgine_Collada_Library_Images libraryImages = new();
        DaeObject.Library_Images = libraryImages;
        Grendgine_Collada_Library_Effects libraryEffects = new();
        DaeObject.Library_Effects = libraryEffects;

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
        Grendgine_Collada_Asset asset = new()
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
        asset.Title = CryData.RootNode.Name;
        DaeObject.Asset = asset;
        DaeObject.Asset.Contributor = contributors;
    }

    public void WriteLibrary_Materials()
    {
        // Create the list of materials used in this object


        //int numMaterials = CryData.Materials.Count;
        //// Now create a material for each material in the object
        //Utils.Log(LogLevelEnum.Debug, "Number of materials: {0}", numMaterials);
        //Grendgine_Collada_Material[] materials = new Grendgine_Collada_Material[numMaterials];
        //for (int i = 0; i < numMaterials; i++)
        //{
        //    Grendgine_Collada_Material tmpMaterial = new()
        //    {
        //        Instance_Effect = new Grendgine_Collada_Instance_Effect()
        //    };
        //    // Name is blank if it's a material file with no submats.  Set to file name.
        //    // Need material ID here, so the meshes can reference it.  Use the chunk ID.
        //    if (CryData.Materials[i].Name == null)
        //    {
        //        tmpMaterial.Name = CryData.RootNode.Name;
        //        tmpMaterial.ID = CryData.RootNode.Name;
        //        tmpMaterial.Instance_Effect.URL = "#" + CryData.RootNode.Name + "-effect";
        //    }
        //    else
        //    {
        //        var MatName = CryData.Materials[i].Name;
        //        if (Args.PrefixMaterialNames)
        //            MatName = CryData.Materials[i].SourceFileName + "_" + MatName;

        //        tmpMaterial.Name = MatName;
        //        tmpMaterial.ID = MatName + "-material";          // this is the order the materials appear in the .mtl file.  Needed for geometries.
        //        tmpMaterial.Instance_Effect.URL = "#" + MatName + "-effect";
        //    }

        //    materials[i] = tmpMaterial;
        //}
        //libraryMaterials.Material = materials;
    }

    public void WriteLibrary_Effects()
    {
        // The Effects library.  This is actual material stuff.


        //int numEffects = CryData.Materials.Count;
        //Grendgine_Collada_Effect[] effects = new Grendgine_Collada_Effect[numEffects];
        //for (int i = 0; i < numEffects; i++)
        //{
        //    string MatName = CryData.Materials[i].Name;
        //    if (Args.PrefixMaterialNames)
        //        MatName = CryData.Materials[i].SourceFileName + "_" + MatName;

        //    Grendgine_Collada_Effect tmpEffect = new()
        //    {
        //        //tmpEffect.Name = CryData.Materials[i].Name;
        //        ID = MatName + "-effect",
        //        Name = MatName
        //    };
        //    effects[i] = tmpEffect;

        //    // create the profile_common for the effect
        //    List<Grendgine_Collada_Profile_COMMON> profiles = new();
        //    Grendgine_Collada_Profile_COMMON profile = new();
        //    profiles.Add(profile);

        //    // Create a list for the new_params
        //    List<Grendgine_Collada_New_Param> newparams = new();
        //    #region set up the sampler and surface for the materials. 
        //    // Check to see if the texture exists, and if so make a sampler and surface.
        //    for (int j = 0; j < CryData.Materials[i].Textures.Length; j++)
        //    {
        //        // Add the Surface node
        //        Grendgine_Collada_New_Param texSurface = new()
        //        {
        //            sID = CleanTexFileName(CryData.Materials[i].Textures[j].File) + "-surface"
        //        };
        //        Grendgine_Collada_Surface surface = new();
        //        texSurface.Surface = surface;
        //        surface.Init_From = new Grendgine_Collada_Init_From();
        //        //Grendgine_Collada_Surface surface2D = new Grendgine_Collada_Surface();
        //        //texSurface.sID = CleanName(CryData.Materials[i].Textures[j].File) + CryData.Materials[i].Textures[j].Map + "-surface";
        //        texSurface.Surface.Type = "2D";
        //        texSurface.Surface.Init_From = new Grendgine_Collada_Init_From
        //        {
        //            //texSurface.Surface.Init_From.Uri = CleanName(texture.File);
        //            Uri = CryData.Materials[i].Name + "_" + CryData.Materials[i].Textures[j].Map
        //        };

        //        // Add the Sampler node
        //        Grendgine_Collada_New_Param texSampler = new()
        //        {
        //            sID = CleanTexFileName(CryData.Materials[i].Textures[j].File) + "-sampler"
        //        };
        //        Grendgine_Collada_Sampler2D sampler2D = new();
        //        texSampler.Sampler2D = sampler2D;
        //        //Grendgine_Collada_Source samplerSource = new Grendgine_Collada_Source();
        //        texSampler.Sampler2D.Source = texSurface.sID;

        //        newparams.Add(texSurface);
        //        newparams.Add(texSampler);
        //    }
        //    #endregion

        //    #region Create the Technique
        //    // Make the techniques for the profile
        //    Grendgine_Collada_Effect_Technique_COMMON technique = new();
        //    Grendgine_Collada_Phong phong = new();
        //    technique.Phong = phong;
        //    technique.sID = "common";
        //    profile.Technique = technique;

        //    phong.Diffuse = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type();
        //    phong.Specular = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type();

        //    // Add all the emissive, etc features to the phong
        //    // Need to check if a texture exists.  If so, refer to the sampler.  Should be a <Texture Map="Diffuse" line if there is a map.
        //    bool diffuseFound = false;
        //    bool specularFound = false;

        //    foreach (var texture in CryData.Materials[i].Textures)
        //    {
        //        if (texture.Map == Texture.MapTypeEnum.Diffuse)
        //        {
        //            diffuseFound = true;
        //            phong.Diffuse.Texture = new Grendgine_Collada_Texture
        //            {
        //                // Texcoord is the ID of the UV source in geometries.  Not needed.
        //                Texture = CleanTexFileName(texture.File) + "-sampler",
        //                TexCoord = ""
        //            };
        //        }
        //        if (texture.Map == Texture.MapTypeEnum.Specular)
        //        {
        //            specularFound = true;
        //            phong.Specular.Texture = new Grendgine_Collada_Texture
        //            {
        //                Texture = CleanTexFileName(texture.File) + "-sampler",
        //                TexCoord = ""
        //            };

        //        }
        //        if (texture.Map == Texture.MapTypeEnum.Bumpmap)
        //        {
        //            // Bump maps go in an extra node.
        //            // bump maps are added to an extra node.
        //            Grendgine_Collada_Extra[] extras = new Grendgine_Collada_Extra[1];
        //            Grendgine_Collada_Extra extra = new();
        //            extras[0] = extra;

        //            technique.Extra = extras;

        //            // Create the technique for the extra

        //            Grendgine_Collada_Technique[] extraTechniques = new Grendgine_Collada_Technique[1];
        //            Grendgine_Collada_Technique extraTechnique = new();
        //            extra.Technique = extraTechniques;
        //            //extraTechnique.Data[0] = new XmlElement();

        //            extraTechniques[0] = extraTechnique;
        //            extraTechnique.profile = "FCOLLADA";

        //            Grendgine_Collada_BumpMap bumpMap = new()
        //            {
        //                Textures = new Grendgine_Collada_Texture[1]
        //            };
        //            bumpMap.Textures[0] = new Grendgine_Collada_Texture
        //            {
        //                Texture = CleanTexFileName(texture.File) + "-sampler"
        //            };
        //            extraTechnique.Data = new XmlElement[1];
        //            extraTechnique.Data[0] = bumpMap;
        //        }
        //    }
        //    if (diffuseFound == false)
        //    {
        //        phong.Diffuse.Color = new Grendgine_Collada_Color
        //        {
        //            Value_As_String = CryData.Materials[i].Diffuse?.ToString() ?? string.Empty,
        //            sID = "diffuse"
        //        };
        //    }
        //    if (specularFound == false)
        //    {
        //        phong.Specular.Color = new Grendgine_Collada_Color { sID = "specular" };
        //        if (CryData.Materials[i].Specular != null)
        //            phong.Specular.Color.Value_As_String = CryData.Materials[i].Specular ?? string.Empty;
        //        else
        //            phong.Specular.Color.Value_As_String = "1 1 1";
        //    }

        //    phong.Emission = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type
        //    {
        //        Color = new Grendgine_Collada_Color
        //        {
        //            sID = "emission",
        //            Value_As_String = CryData.Materials[i].Emissive ?? string.Empty
        //        }
        //    };
        //    phong.Shininess = new Grendgine_Collada_FX_Common_Float_Or_Param_Type { Float = new Grendgine_Collada_SID_Float() };
        //    phong.Shininess.Float.sID = "shininess";
        //    phong.Shininess.Float.Value = (float)CryData.Materials[i].Shininess;
        //    phong.Index_Of_Refraction = new Grendgine_Collada_FX_Common_Float_Or_Param_Type { Float = new Grendgine_Collada_SID_Float() };

        //    phong.Transparent = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type
        //    {
        //        Color = new Grendgine_Collada_Color(),
        //        Opaque = new Grendgine_Collada_FX_Opaque_Channel()
        //    };
        //    phong.Transparent.Color.Value_As_String = (1 - double.Parse(CryData.Materials[i].Opacity ?? "1")).ToString();  // Subtract from 1 for proper value.

        //    #endregion

        //    tmpEffect.Profile_COMMON = profiles.ToArray();
        //    profile.New_Param = new Grendgine_Collada_New_Param[newparams.Count];
        //    profile.New_Param = newparams.ToArray();
        //}

        //libraryEffects.Effect = effects;

    }

    /// <summary> Write the Library_Geometries element.  These won't be instantiated except through the visual scene or controllers. </summary>
    public void WriteLibrary_Geometries()
    {
        Grendgine_Collada_Library_Geometries libraryGeometries = new();

        // Make a list for all the geometries objects we will need. Will convert to array at end.  Define the array here as well
        // We have to define a Geometry for EACH meshsubset in the meshsubsets, since the mesh can contain multiple materials
        List<Grendgine_Collada_Geometry> geometryList = new();

        // For each of the nodes, we need to write the geometry.
        foreach (ChunkNode nodeChunk in CryData.Chunks.Where(a => a.ChunkType == ChunkType.Node))
        {
            // Create a geometry object.  Use the chunk ID for the geometry ID
            // Create all the materials used by this chunk.
            // Will have to be careful with this, since with .cga/.cgam pairs will need to match by Name.
            // Make the mesh object.  This will have 3 or 4 sources, 1 vertices, and 1 or more Triangles (with material ID)
            // If the Object ID of Node chunk points to a Helper or a Controller though, place an empty.
            ChunkDataStream normals = null;
            ChunkDataStream uvs = null;
            ChunkDataStream tmpVertices = null;
            ChunkDataStream vertsUvs = null;
            ChunkDataStream indices = null;
            ChunkDataStream colors = null;
            ChunkDataStream tangents = null;

            if (IsNodeNameExcluded(nodeChunk.Name))
            {
                Utils.Log(LogLevelEnum.Debug, $"Excluding node {nodeChunk.Name}");
                continue;
            }

            if (nodeChunk.ObjectChunk is null)
            {
                Utils.Log(LogLevelEnum.Warning, "Skipped node with missing Object {0}", nodeChunk.Name);
                continue;
            }

            if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkType.Mesh)
            {
                // Create materials collection for this node. Index of collection in meshSubSets determines which mat to use.
                // What do we do if multiple nodes access the same material file?
                CreateMaterialsFromNodeChunk(nodeChunk);

                // Get the mesh chunk and submesh chunk for this node.
                var meshChunk = (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];

                // Check to see if the Mesh points to a PhysicsData mesh.  Don't want to write these.
                if (meshChunk.MeshPhysicsData != 0)
                    // TODO:  Implement this chunk

                    if (meshChunk.MeshSubsetsData != 0)   // For the SC files, you can have Mesh chunks with no Mesh Subset.  Need to skip these.  They are in the .cga file and contain no geometry.
                    {
                        var meshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[meshChunk.MeshSubsetsData];  // Listed as Object ID for the Node

                        if (meshChunk.VerticesData != 0)
                            tmpVertices = (ChunkDataStream)nodeChunk._model.ChunkMap[meshChunk.VerticesData];

                        if (meshChunk.VertsUVsData != 0)
                            vertsUvs = (ChunkDataStream)nodeChunk._model.ChunkMap[meshChunk.VertsUVsData];

                        if (tmpVertices is null && vertsUvs is null) // There is no vertex data for this node.  Skip.
                            continue;

                        if (meshChunk.NormalsData != 0)
                            normals = (ChunkDataStream)nodeChunk._model.ChunkMap[meshChunk.NormalsData];

                        if (meshChunk.UVsData != 0)
                            uvs = (ChunkDataStream)nodeChunk._model.ChunkMap[meshChunk.UVsData];

                        if (meshChunk.IndicesData != 0)
                            indices = (ChunkDataStream)nodeChunk._model.ChunkMap[meshChunk.IndicesData];

                        if (meshChunk.ColorsData != 0)
                            colors = (ChunkDataStream)nodeChunk._model.ChunkMap[meshChunk.ColorsData];

                        if (meshChunk.TangentsData != 0)
                            tangents = (ChunkDataStream)nodeChunk._model.ChunkMap[meshChunk.TangentsData];

                        // tmpGeo is a Geometry object for each meshsubset.  Name will be "Nodechunk name_matID".
                        Grendgine_Collada_Geometry geometry = new()
                        {
                            Name = nodeChunk.Name,
                            ID = nodeChunk.Name + "-mesh"
                        };
                        Grendgine_Collada_Mesh tmpMesh = new();
                        geometry.Mesh = tmpMesh;

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

                        Grendgine_Collada_Vertices vertices = new() { ID = nodeChunk.Name + "-vertices" };
                        geometry.Mesh.Vertices = vertices;
                        Grendgine_Collada_Input_Unshared[] inputshared = new Grendgine_Collada_Input_Unshared[4];
                        Grendgine_Collada_Input_Unshared posInput = new() { Semantic = Grendgine_Collada_Input_Semantic.POSITION };
                        vertices.Input = inputshared;

                        Grendgine_Collada_Input_Unshared normInput = new() { Semantic = Grendgine_Collada_Input_Semantic.NORMAL };
                        Grendgine_Collada_Input_Unshared uvInput = new() { Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD };
                        Grendgine_Collada_Input_Unshared colorInput = new() { Semantic = Grendgine_Collada_Input_Semantic.COLOR };

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

                        if (tmpVertices is not null)  // Will be null if it's using VertsUVs.
                        {
                            floatArrayVerts.ID = posSource.ID + "-array";
                            floatArrayVerts.Digits = 6;
                            floatArrayVerts.Magnitude = 38;
                            floatArrayVerts.Count = (int)tmpVertices.NumElements * 3;
                            floatArrayUVs.ID = uvSource.ID + "-array";
                            floatArrayUVs.Digits = 6;
                            floatArrayUVs.Magnitude = 38;
                            floatArrayUVs.Count = (int)uvs.NumElements * 2;
                            floatArrayNormals.ID = normSource.ID + "-array";
                            floatArrayNormals.Digits = 6;
                            floatArrayNormals.Magnitude = 38;
                            if (normals != null)
                                floatArrayNormals.Count = (int)normals.NumElements * 3;
                            floatArrayColors.ID = colorSource.ID + "-array";
                            floatArrayColors.Digits = 6;
                            floatArrayColors.Magnitude = 38;
                            if (colors != null)
                            {
                                floatArrayColors.Count = (int)colors.NumElements * 4;
                                for (uint j = 0; j < colors.NumElements; j++)  // Create Colors string
                                {
                                    colorString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} {3:F6} ",
                                        colors.Colors[j].r / 255.0,
                                        colors.Colors[j].g / 255.0,
                                        colors.Colors[j].b / 255.0,
                                        colors.Colors[j].a / 255.0);
                                }
                            }

                            // Create Vertices and normals string
                            for (uint j = 0; j < meshChunk.NumVertices; j++)
                            {
                                Vector3 vertex = tmpVertices.Vertices[j];
                                vertString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} ", vertex.X, vertex.Y, vertex.Z);
                                Vector3 normal = normals?.Normals[j] ?? tangents?.Normals[j] ?? new Vector3(0.0f, 0.0f, 0.0f);
                                normString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} ", Safe(normal.X), Safe(normal.Y), Safe(normal.Z));
                            }
                            for (uint j = 0; j < uvs.NumElements; j++)     // Create UV string
                            {
                                uvString.AppendFormat(culture, "{0:F6} {1:F6} ", Safe(uvs.UVs[j].U), 1 - Safe(uvs.UVs[j].V));
                            }
                        }
                        else                // VertsUV structure.  Pull out verts and UVs from tmpVertsUVs.
                        {
                            floatArrayVerts.ID = posSource.ID + "-array";
                            floatArrayVerts.Digits = 6;
                            floatArrayVerts.Magnitude = 38;
                            floatArrayVerts.Count = (int)vertsUvs.NumElements * 3;
                            floatArrayUVs.ID = uvSource.ID + "-array";
                            floatArrayUVs.Digits = 6;
                            floatArrayUVs.Magnitude = 38;
                            floatArrayUVs.Count = (int)vertsUvs.NumElements * 2;
                            floatArrayNormals.ID = normSource.ID + "-array";
                            floatArrayNormals.Digits = 6;
                            floatArrayNormals.Magnitude = 38;
                            floatArrayNormals.Count = (int)vertsUvs.NumElements * 3;
                            floatArrayColors.ID = colorSource.ID + "-array";
                            floatArrayColors.Digits = 6;
                            floatArrayColors.Magnitude = 38;
                            if (vertsUvs.Colors != null)
                            {
                                floatArrayColors.Count = vertsUvs.Colors.Length * 4;
                                for (uint j = 0; j < vertsUvs.Colors.Length; j++)  // Create Colors string
                                {
                                    colorString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} {3:F6} ",
                                        vertsUvs.Colors[j].r / 255.0,
                                        vertsUvs.Colors[j].g / 255.0,
                                        vertsUvs.Colors[j].b / 255.0,
                                        vertsUvs.Colors[j].a / 255.0);
                                }
                            }

                            // Dymek's code to rescale by bounding box.  Only apply to geometry (cga or cgf), and not skin or chr objects.
                            var multiplerVector = Vector3.Abs((meshChunk.MinBound - meshChunk.MaxBound) / 2f);
                            if (multiplerVector.X < 1) { multiplerVector.X = 1; }
                            if (multiplerVector.Y < 1) { multiplerVector.Y = 1; }
                            if (multiplerVector.Z < 1) { multiplerVector.Z = 1; }
                            var boundaryBoxCenter = (meshChunk.MinBound + meshChunk.MaxBound) / 2f;

                            // Create Vertices, normals and colors string
                            for (uint j = 0; j < meshChunk.NumVertices; j++)
                            {
                                Vector3 vertex = vertsUvs.Vertices[j];
                                // Rotate/translate the vertex
                                if (!CryData.InputFile.EndsWith("skin") && !CryData.InputFile.EndsWith("chr"))
                                    vertex = (vertex * multiplerVector) + boundaryBoxCenter;

                                vertString.AppendFormat("{0:F6} {1:F6} {2:F6} ", Safe(vertex.X), Safe(vertex.Y), Safe(vertex.Z));

                                // TODO:  This isn't right?  VertsUvs may always have color as the 3rd element.
                                // Normals depend on the data size.  16 byte structures have the normals in the Tangents.  20 byte structures are in the VertsUV.
                                Vector3 normal = new();
                                if (vertsUvs.Normals != null)
                                    normal = vertsUvs.Normals[j];
                                else if (tangents != null && tangents.Normals != null)
                                    normal = tangents.Normals[j];

                                normString.AppendFormat("{0:F6} {1:F6} {2:F6} ", Safe(normal.X), Safe(normal.Y), Safe(normal.Z));
                            }
                            // Create UV string
                            for (uint j = 0; j < vertsUvs.NumElements; j++)
                            {
                                uvString.AppendFormat("{0:F6} {1:F6} ", Safe(vertsUvs.UVs[j].U), Safe(1 - vertsUvs.UVs[j].V));
                            }
                        }
                        CleanNumbers(vertString);
                        CleanNumbers(normString);
                        CleanNumbers(uvString);
                        CleanNumbers(colorString);

                        #region Create the triangles node.
                        var triangles = new Grendgine_Collada_Triangles[meshSubsets.NumMeshSubset];
                        geometry.Mesh.Triangles = triangles;

                        for (uint j = 0; j < meshSubsets.NumMeshSubset; j++) // Need to make a new Triangles entry for each submesh.
                        {
                            triangles[j] = new Grendgine_Collada_Triangles
                            {
                                Count = meshSubsets.MeshSubsets[j].NumIndices / 3,
                                Material = GetMaterialName(nodeChunk, meshSubsets, (int)j)
                            };

                            // Create the inputs.  vertex, normal, texcoord, color
                            int inputCount = 3;
                            if (colors != null || vertsUvs?.Colors != null)
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
                            if (colors != null || vertsUvs?.Colors != null)
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
                            StringBuilder vc = new();
                            for (var k = meshSubsets.MeshSubsets[j].FirstIndex; k < (meshSubsets.MeshSubsets[j].FirstIndex + meshSubsets.MeshSubsets[j].NumIndices); k++)
                            {
                                int ccount = 3;

                                if (colors != null || vertsUvs?.Colors != null)
                                    ccount++;

                                vc.AppendFormat(culture, String.Format("{0} ", ccount));
                                k += 2;
                            }

                            // Create the P node for the Triangles.
                            StringBuilder p = new();
                            for (var k = meshSubsets.MeshSubsets[j].FirstIndex; k < (meshSubsets.MeshSubsets[j].FirstIndex + meshSubsets.MeshSubsets[j].NumIndices); k++)
                            {
                                int values = 0;
                                if (colors != null || vertsUvs?.Colors != null)
                                {
                                    values++;
                                }

                                List<string> formatlist = new();
                                formatlist.Add("{0} {0} {0} ");
                                formatlist.Add("{1} {1} {1} ");
                                formatlist.Add("{2} {2} {2} ");
                                for (var valuecount = 0; valuecount < values; valuecount++)
                                {
                                    formatlist[0] += "{0} ";
                                    formatlist[1] += "{1} ";
                                    formatlist[2] += "{2} ";
                                }
                                string finalformat = String.Join("", formatlist);
                                p.AppendFormat(finalformat, indices.Indices[k], indices.Indices[k + 1], indices.Indices[k + 2]);
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
                        geometry.Mesh.Source = source;

                        // create the technique_common for each of these
                        posSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
                        {
                            Accessor = new Grendgine_Collada_Accessor()
                        };
                        posSource.Technique_Common.Accessor.Source = "#" + floatArrayVerts.ID;
                        posSource.Technique_Common.Accessor.Stride = 3;
                        posSource.Technique_Common.Accessor.Count = (uint)meshChunk.NumVertices;
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
                                Count = (uint)meshChunk.NumVertices
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
                            uvSource.Technique_Common.Accessor.Count = uvs.NumElements;
                        else
                            uvSource.Technique_Common.Accessor.Count = vertsUvs.NumElements;

                        Grendgine_Collada_Param[] paramUV = new Grendgine_Collada_Param[2];
                        paramUV[0] = new Grendgine_Collada_Param();
                        paramUV[1] = new Grendgine_Collada_Param();
                        paramUV[0].Name = "S";
                        paramUV[0].Type = "float";
                        paramUV[1].Name = "T";
                        paramUV[1].Type = "float";
                        uvSource.Technique_Common.Accessor.Param = paramUV;

                        if (colors != null || vertsUvs?.Colors != null)
                        {
                            uint numberOfElements;
                            if (colors != null)
                                numberOfElements = colors.NumElements;
                            else
                                numberOfElements = (uint)vertsUvs.Colors.Length;

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

                        geometryList.Add(geometry);

                        #endregion
                    }
            }
            // There is no geometry for a helper or controller node.  Can skip the rest.
        }
        libraryGeometries.Geometry = geometryList.ToArray();
        DaeObject.Library_Geometries = libraryGeometries;
    }

    /// <summary>Get the material name for a given submesh.</summary>
    /// <param name="nodeChunk">The node chunk with the material</param>
    /// <param name="meshSubsets"></param>
    /// <returns>Material name with '-material' appended</returns>
    private string GetMaterialName(ChunkNode nodeChunk, ChunkMeshSubsets meshSubsets, int index)
    {
        var materialLibraryChunk = (ChunkMtlName)CryData.Chunks[nodeChunk.MatID];
        var materialLibraryIndex = meshSubsets.MeshSubsets[index].MatID;
        var materialChunk = (ChunkMtlName)CryData.Chunks[(int)materialLibraryChunk.ChildIDs[materialLibraryIndex]];

        return materialChunk.Name + "-material";
    }

    private void CreateMaterialsFromNodeChunk(ChunkNode nodeChunk)
    {
        List<Grendgine_Collada_Material> colladaMaterials = new();

        var matChunkForNode = (ChunkMtlName)CryData.Chunks.Where(c => c.ID == nodeChunk.MatID).FirstOrDefault();
        
        if (matChunkForNode.MatType == MtlNameType.Library || matChunkForNode.MatType == MtlNameType.Single)
        {
            // Create a material in DaeObject for each Submaterial.
            foreach (var mat in nodeChunk.Materials.SubMaterials)
            {
                colladaMaterials.Add(AddMaterialToMaterialLibrary(mat));
            }
        }
        else if (matChunkForNode.MatType == MtlNameType.Basic)
            colladaMaterials.Add(AddMaterialToMaterialLibrary(nodeChunk.Materials));

        if (DaeObject.Library_Materials.Material is null)
            DaeObject.Library_Materials.Material = colladaMaterials.ToArray();
        else
        {
            int arraySize = DaeObject.Library_Materials.Material.Length;
            Array.Resize(ref DaeObject.Library_Materials.Material, DaeObject.Library_Materials.Material.Length + colladaMaterials.Count);
            colladaMaterials.CopyTo(DaeObject.Library_Materials.Material, arraySize);
        }

        //AddEffectsToEffectsLibrary(nodeChunk);
    }

    private void AddEffectsToEffectsLibrary(ChunkNode nodeChunk)
    {
        throw new NotImplementedException();
    }

    private Grendgine_Collada_Material AddMaterialToMaterialLibrary(Material mat)
    {
        Grendgine_Collada_Material material = new()
        {
            Instance_Effect = new Grendgine_Collada_Instance_Effect(),
            Name = mat.Name,
            ID = mat.Name + "-material"
        };
        material.Instance_Effect.URL = "#" + mat.Name + "-effect";

        AddImagesToImagesLibrary(mat);
        return material;
    }

    private void AddImagesToImagesLibrary(Material mat)
    {
        List<Grendgine_Collada_Image> imageList = new();
        int numberOfTextures = mat.Textures.Length;

        for (int i = 0; i < numberOfTextures; i++)
        {
            // For each texture in the material, we make a new <image> object and add it to the list. 
            Grendgine_Collada_Image image = new()
            {
                ID = mat.Name + "_" + mat.Textures[i].Map,
                Name = mat.Name + "_" + mat.Textures[i].Map,
                Init_From = new Grendgine_Collada_Init_From()
            };
            // Try to resolve the texture file to a file on disk. Texture are always based on DataDir.
            StringBuilder builder = new(ResolveTextureFile(mat.Textures[i].File, Args.DataDir));

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
            _ = DaeObject.Collada_Version == "1.4.1" ? image.Init_From.Uri = builder.ToString() : image.Init_From.Ref = builder.ToString();

            imageList.Add(image);
        }

        Grendgine_Collada_Image[] images = imageList.ToArray();
        if (DaeObject.Library_Images.Image is null)
            DaeObject.Library_Images.Image = images;
        else
        {
            int arraySize = DaeObject.Library_Images.Image.Length;
            Array.Resize(ref DaeObject.Library_Images.Image, DaeObject.Library_Images.Image.Length + images.Length);
            images.CopyTo(DaeObject.Library_Images.Image, arraySize);
        }
    }

    public void WriteLibrary_Controllers()
    {
        if (DaeObject.Library_Geometries.Geometry.Length != 0)
        {
            Grendgine_Collada_Library_Controllers libraryController = new();

            // There can be multiple controllers in the controller library.  But for Cryengine files, there is only one rig.
            // So if a rig exists, make that the controller.  This applies mostly to .chr files, which will have a rig and may have geometry.
            Grendgine_Collada_Controller controller = new() { ID = "Controller" };
            // Create the skin object and assign to the controller
            Grendgine_Collada_Skin skin = new()
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
            Grendgine_Collada_Source jointsSource = new()
            {
                ID = "Controller-joints"
            };
            jointsSource.Name_Array = new Grendgine_Collada_Name_Array()
            {
                ID = "Controller-joints-array",
                Count = CryData.SkinningInfo.CompiledBones.Count,
            };
            StringBuilder boneNames = new();
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
            Grendgine_Collada_Source bindPoseArraySource = new()
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
            Grendgine_Collada_Source weightArraySource = new()
            {
                ID = "Controller-weights"
            };
            weightArraySource.Technique_Common = new Grendgine_Collada_Technique_Common_Source();
            Grendgine_Collada_Accessor accessor = weightArraySource.Technique_Common.Accessor = new Grendgine_Collada_Accessor();

            weightArraySource.Float_Array = new Grendgine_Collada_Float_Array()
            {
                ID = "Controller-weights-array",
            };
            StringBuilder weights = new();

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
            StringBuilder vCount = new();
            //for (int i = 0; i < CryData.Models[0].SkinningInfo.IntVertices.Count; i++)
            for (int i = 0; i < CryData.SkinningInfo.BoneMapping.Count; i++)
            {
                vCount.Append("4 ");
            };
            vertexWeights.VCount = new Grendgine_Collada_Int_Array_String
            {
                Value_As_String = vCount.ToString().TrimEnd()
            };
            StringBuilder vertices = new();
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
        Grendgine_Collada_Library_Visual_Scenes libraryVisualScenes = new();

        List<Grendgine_Collada_Visual_Scene> visualScenes = new();
        Grendgine_Collada_Visual_Scene visualScene = new();
        List<Grendgine_Collada_Node> nodes = new();

        // Check to see if there is a CompiledBones chunk.  If so, add a Node.
        if (CryData.Chunks.Any(a => a.ChunkType == ChunkType.CompiledBones ||
            a.ChunkType == ChunkType.CompiledBonesSC ||
            a.ChunkType == ChunkType.CompiledBonesIvo))
        {
            Grendgine_Collada_Node boneNode = new();
            boneNode = CreateJointNode(CryData.Bones.RootBone);
            nodes.Add(boneNode);
        }

        // THERE CAN BE MULTIPLE ROOT NODES IN EACH FILE!  Check to see if the parentnodeid ~0 and be sure to add a node for it.
        List<Grendgine_Collada_Node> positionNodes = new();
        List<ChunkNode> positionRoots = CryData.Models[0].NodeMap.Values.Where(a => a.ParentNodeID == ~0).ToList();
        foreach (ChunkNode root in positionRoots)
        {
            positionNodes.Add(CreateNode(root));
        }
        nodes.AddRange(positionNodes.ToArray());

        visualScene.Node = nodes.ToArray();
        visualScene.ID = "Scene";
        visualScenes.Add(visualScene);

        libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
        DaeObject.Library_Visual_Scene = libraryVisualScenes;
    }

    /// <summary> Provides a library in which to place visual_scene elements for chr files (rigs + geometry). </summary>
    public void WriteLibrary_VisualScenesWithSkeleton()
    {
        Grendgine_Collada_Library_Visual_Scenes libraryVisualScenes = new();

        List<Grendgine_Collada_Visual_Scene> visualScenes = new();
        Grendgine_Collada_Visual_Scene visualScene = new();
        List<Grendgine_Collada_Node> nodes = new();

        // Check to see if there is a CompiledBones chunk.  If so, add a Node.  
        if (CryData.Chunks.Any(a => a.ChunkType == ChunkType.CompiledBones ||
            a.ChunkType == ChunkType.CompiledBonesSC ||
            a.ChunkType == ChunkType.CompiledBonesIvo))
        {
            Grendgine_Collada_Node boneNode = new();
            boneNode = CreateJointNode(CryData.Bones.RootBone);
            nodes.Add(boneNode);
        }

        if (CryData.Models[0].HasGeometry)
        {
            var allParentNodes = CryData.NodeMap.Values.Where(n => n.ParentNodeID != ~1);

            foreach (var node in allParentNodes)
            {
                Grendgine_Collada_Node colladaNode = new()
                {
                    ID = node.Name,
                    Name = node.Name,
                    Type = Grendgine_Collada_Node_Type.NODE,
                    Matrix = new Grendgine_Collada_Matrix[1]
                };
                colladaNode.Matrix[0] = new Grendgine_Collada_Matrix { Value_As_String = CreateStringFromMatrix4x4(Matrix4x4.Identity) };
                colladaNode.Instance_Controller = new Grendgine_Collada_Instance_Controller[1];
                colladaNode.Instance_Controller[0] = new Grendgine_Collada_Instance_Controller
                {
                    URL = "#Controller",
                    Skeleton = new Grendgine_Collada_Skeleton[1]
                };

                var skeleton = colladaNode.Instance_Controller[0].Skeleton[0] = new Grendgine_Collada_Skeleton();
                skeleton.Value = "#Armature";
                colladaNode.Instance_Controller[0].Bind_Material = new Grendgine_Collada_Bind_Material[1];
                Grendgine_Collada_Bind_Material bindMaterial = colladaNode.Instance_Controller[0].Bind_Material[0] = new Grendgine_Collada_Bind_Material();

                // Create an Instance_Material for each material
                bindMaterial.Technique_Common = new Grendgine_Collada_Technique_Common_Bind_Material();
                List<Grendgine_Collada_Instance_Material_Geometry> instanceMaterials = new();

                // The material library has all the materials in the model. The node


                //bindMaterial.Technique_Common.Instance_Material = new Grendgine_Collada_Instance_Material_Geometry[CryData.Materials.Count];
                //// This gets complicated.  We need to make one instance_material for each material used in this node chunk.  The mat IDs used in this
                //// node chunk are stored in meshsubsets, so for each subset we need to grab the mat, get the target (id), and make an instance_material for it.

                //for (int i = 0; i < CryData.Materials.Count; i++)
                //{
                //    string MatName = CryData.Materials[i].Name;

                //    // For each mesh subset, we want to create an instance material and add it to instanceMaterials list.
                //    Grendgine_Collada_Instance_Material_Geometry tmpInstanceMat = new()
                //    {
                //        Target = "#" + MatName + "-material",
                //        Symbol = MatName + "-material"
                //    };
                //    instanceMaterials.Add(tmpInstanceMat);
                //}

                colladaNode.Instance_Controller[0].Bind_Material[0].Technique_Common.Instance_Material = instanceMaterials.ToArray();
                if (node.AllChildNodes is not null)
                {
                    foreach (var child in node.AllChildNodes)
                    {
                        colladaNode.node = CreateChildNodes(child);
                    }
                }

                nodes.Add(colladaNode);
                
            }
        }

        visualScene.Node = nodes.ToArray();
        visualScene.ID = "Scene";
        visualScenes.Add(visualScene);

        libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
        DaeObject.Library_Visual_Scene = libraryVisualScenes;
    }

    private Grendgine_Collada_Node CreateNode(ChunkNode nodeChunk)
    {
        List<Grendgine_Collada_Node> childNodes = new();

        Grendgine_Collada_Node colladaNode = new();
        // Check to see if there is a second model file, and if the mesh chunk is actually there.
        if (CryData.Models.Count > 1)
        {
            // Star Citizen pair.  Get the Node and Mesh chunks from the geometry file, unless it's a Proxy node.
            string nodeName = nodeChunk.Name;
            int nodeID = nodeChunk.ID;

            // make sure there is a geometry node in the geometry file
            if (CryData.Models[0].ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkType.Helper)
                colladaNode = CreateSimpleNode(nodeChunk);
            else
            {
                ChunkNode geometryNode = CryData.Models[1].NodeMap.Values.Where(a => a.Name == nodeChunk.Name).FirstOrDefault();
                Grendgine_Collada_Geometry geometryLibraryObject = DaeObject.Library_Geometries.Geometry.Where(a => a.Name == nodeChunk.Name).FirstOrDefault();
                if (geometryNode == null || geometryLibraryObject == null)
                    colladaNode = CreateSimpleNode(nodeChunk);  // Can't find geometry for given node.
                else
                {
                    ChunkMesh geometryMesh = (ChunkMesh)CryData.Models[1].ChunkMap[geometryNode.ObjectNodeID];
                    colladaNode = CreateGeometryNode(geometryNode, geometryMesh);
                }
            }
        }
        else
        {
            if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkType.Mesh)
            {
                var meshChunk = (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                if (meshChunk.MeshSubsetsData == 0 || meshChunk.NumVertices == 0)  // Can have a node with a mesh and meshsubset, but no vertices.  Write as simple node.
                    colladaNode = CreateSimpleNode(nodeChunk);
                else
                {
                    if (nodeChunk._model.ChunkMap[meshChunk.MeshSubsetsData].ID != 0)
                        colladaNode = CreateGeometryNode(nodeChunk, (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID]);
                    else
                        colladaNode = CreateSimpleNode(nodeChunk);
                }
            }
            else
                colladaNode = CreateSimpleNode(nodeChunk);
        }

        // Add childnodes
        colladaNode.node = CreateChildNodes(nodeChunk);
        return colladaNode;
    }

    /// <summary>This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics </summary>
    private Grendgine_Collada_Node CreateSimpleNode(ChunkNode nodeChunk)
    {
        // This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics
        Grendgine_Collada_Node_Type nodeType = Grendgine_Collada_Node_Type.NODE;
        Grendgine_Collada_Node colladaNode = new()
        {
            Type = nodeType,
            Name = nodeChunk.Name,
            ID = nodeChunk.Name
        };
        List<Grendgine_Collada_Matrix> matrices = new();

        Grendgine_Collada_Matrix matrix = new()
        {
            Value_As_String = CreateStringFromMatrix4x4(nodeChunk.LocalTransform),
            sID = "transform"
        };
        matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
        colladaNode.Matrix = matrices.ToArray();

        // Add childnodes
        colladaNode.node = CreateChildNodes(nodeChunk);
        return colladaNode;
    }

    /// <summary>Used by CreateNode and CreateSimpleNodes to create all the child nodes for the given node.</summary>
    private Grendgine_Collada_Node[]? CreateChildNodes(ChunkNode nodeChunk)
    {
        if (nodeChunk.__NumChildren != 0)
        {
            List<Grendgine_Collada_Node> childNodes = new();
            foreach (var childNodeChunk in nodeChunk.AllChildNodes.ToList())
            {
                if (IsNodeNameExcluded(childNodeChunk.Name))
                {
                    Utils.Log(LogLevelEnum.Debug, $"Excluding child node {childNodeChunk.Name}");
                    continue;
                }

                Grendgine_Collada_Node childNode = CreateNode(childNodeChunk); ;
                childNodes.Add(childNode);
            }
            return childNodes.ToArray();
        }
        else
            return null;
    }

    private Grendgine_Collada_Node CreateJointNode(CompiledBone bone)
    {
        // This will be used recursively to create a node object and return it to WriteLibrary_VisualScenes
        // If this is the root bone, set the node id to Armature.  Otherwise set to armature_<bonename>
        string id = "Armature";
        if (bone.parentID != 0)
            id += "_" + bone.boneName.Replace(' ', '_');
        Grendgine_Collada_Node tmpNode = new()
        {
            ID = id,
            Name = bone.boneName.Replace(' ', '_'),
            sID = bone.boneName.Replace(' ', '_'),
            Type = Grendgine_Collada_Node_Type.JOINT
        };

        Grendgine_Collada_Matrix matrix = new();
        List<Grendgine_Collada_Matrix> matrices = new();
        matrix.Value_As_String = CreateStringFromMatrix4x4(bone.LocalTransform);

        matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
        tmpNode.Matrix = matrices.ToArray();

        // Recursively call this for each of the child bones to this bone.
        if (bone.childIDs.Count > 0)
        {
            Grendgine_Collada_Node[] childNodes = new Grendgine_Collada_Node[bone.childIDs.Count];
            int counter = 0;

            foreach (CompiledBone childBone in CryData.Bones.GetAllChildBones(bone))
            {
                Grendgine_Collada_Node childNode = new();
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
        Grendgine_Collada_Node tmpNode = new();
        var meshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsetsData];
        var nodeType = Grendgine_Collada_Node_Type.NODE;
        tmpNode.Type = nodeType;
        tmpNode.Name = nodeChunk.Name;
        tmpNode.ID = nodeChunk.Name;
        // Make the lists necessary for this Node.
        List<Grendgine_Collada_Matrix> matrices = new();
        List<Grendgine_Collada_Instance_Geometry> instanceGeometries = new();
        List<Grendgine_Collada_Bind_Material> bindMaterials = new();
        List<Grendgine_Collada_Instance_Material_Geometry> instanceMaterials = new();

        Grendgine_Collada_Matrix matrix = new()
        {
            Value_As_String = CreateStringFromMatrix4x4(nodeChunk.LocalTransform),
            sID = "transform"
        };

        matrices.Add(matrix);                       // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
        tmpNode.Matrix = matrices.ToArray();

        // Each node will have one instance geometry, although it could be a list.
        Grendgine_Collada_Instance_Geometry instanceGeometry = new()
        {
            Name = nodeChunk.Name,
            URL = "#" + nodeChunk.Name + "-mesh"  // this is the ID of the geometry.
        };

        Grendgine_Collada_Bind_Material bindMaterial = new()
        {
            Technique_Common = new Grendgine_Collada_Technique_Common_Bind_Material
            {
                Instance_Material = new Grendgine_Collada_Instance_Material_Geometry[meshSubsets.NumMeshSubset]
            }
        };
        bindMaterials.Add(bindMaterial);
        instanceGeometry.Bind_Material = bindMaterials.ToArray();
        instanceGeometries.Add(instanceGeometry);

        tmpNode.Instance_Geometry = instanceGeometries.ToArray();

        // This gets complicated.  We need to make one instance_material for each material used in this node chunk.  The mat IDs used in this
        // node chunk are stored in meshsubsets, so for each subset we need to grab the mat, get the target (id), and make an instance_material for it.
        for (int i = 0; i < meshSubsets.NumMeshSubset; i++)
        {
            // For each mesh subset, we want to create an instance material and add it to instanceMaterials list.
            Grendgine_Collada_Instance_Material_Geometry instanceMaterial = new();
            string matName = GetMaterialName(nodeChunk, meshSubsets, i);
            instanceMaterial.Target = "#" + matName + "-material";
            instanceMaterial.Symbol = matName + "-material";

            instanceMaterials.Add(instanceMaterial);
        }
        tmpNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material = instanceMaterials.ToArray();
        return tmpNode;
    }

    /// <summary>Retrieves the worldtobone (bind pose matrix) for the bone.</summary>
    private string GetBindPoseArray(List<CompiledBone> compiledBones)
    {
        StringBuilder value = new();
        for (int i = 0; i < compiledBones.Count; i++)
        {
            value.Append(CreateStringFromMatrix4x4(compiledBones[i].BindPoseMatrix) + " ");
        }
        return value.ToString().TrimEnd();
    }

    /// <summary> Adds the scene element to the Collada document. </summary>
    private void WriteScene()
    {
        Grendgine_Collada_Scene scene = new();
        Grendgine_Collada_Instance_Visual_Scene visualScene = new()
        {
            URL = "#Scene",
            Name = "Scene"
        };
        scene.Visual_Scene = visualScene;
        DaeObject.Scene = scene;

    }

    private static string CreateStringFromMatrix4x4(Matrix4x4 matrix)
    {
        StringBuilder matrixValues = new();
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
            matrix.M44);
        CleanNumbers(matrixValues);
        return matrixValues.ToString();
    }

    private static double Safe(double value)
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
}