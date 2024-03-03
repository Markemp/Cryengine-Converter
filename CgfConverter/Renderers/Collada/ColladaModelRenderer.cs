using CgfConverter.Collada;
using CgfConverter.CryEngineCore;
using CgfConverter.Models.Materials;
using CgfConverter.Renderers.Collada.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Surfaces;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Animation;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Controller;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Lighting;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Scene;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Transform;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Effects;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Materials;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.COMMON;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Technique_Common;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Collada.Collada.Types;
using Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static CgfConverter.Utilities;
using static Extensions.FileHandlingExtensions;

namespace CgfConverter.Renderers.Collada;

public class ColladaModelRenderer : IRenderer
{
    protected readonly ArgsHandler _args;
    protected readonly CryEngine _cryData;
    public readonly ColladaDoc DaeObject = new();

    private readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
    private const string colladaVersion = "1.4.1";
    private readonly XmlSerializer serializer = new(typeof(ColladaDoc));
    private readonly FileInfo daeOutputFile;

    private readonly Dictionary<int, string> controllerIdToBoneName = new();

    public ColladaModelRenderer(ArgsHandler argsHandler, CryEngine cryEngine)
    {
        _args = argsHandler;
        _cryData = cryEngine;

        daeOutputFile = _args.FormatOutputFileName(".dae", _cryData.InputFile);
    }

    public int Render()
    {
        GenerateDaeObject();

        // At this point, we should have a cryData.Asset object, fully populated.
        Log(LogLevelEnum.Debug);
        Log(LogLevelEnum.Debug, "*** Starting WriteCOLLADA() ***");
        Log(LogLevelEnum.Debug);

        TextWriter writer = new StreamWriter(daeOutputFile.FullName);
        serializer.Serialize(writer, DaeObject);

        writer.Close();
        Log(LogLevelEnum.Debug, "End of Write Collada.  Export complete.");
        return 1;
    }

    public void GenerateDaeObject()
    {
        Log(LogLevelEnum.Debug, "Number of models: {0}", _cryData.Models.Count);
        for (int i = 0; i < _cryData.Models.Count; i++)
        {
            Log(LogLevelEnum.Debug, "\tNumber of nodes in model: {0}", _cryData.Models[i].NodeMap.Count);
        }

        WriteColladaRoot(colladaVersion);
        WriteAsset();
        WriteScene();

        DaeObject.Library_Materials = new();
        DaeObject.Library_Images = new();
        DaeObject.Library_Effects = new();

        CreateMaterials();
        WriteLibrary_Geometries();

        // If there is Skinning info, create the controller library and set up visual scene to refer to it.  Otherwise just write the Visual Scene
        if (_cryData.SkinningInfo?.HasSkinningInfo ?? false)
        {
            WriteLibrary_Controllers();
            WriteLibrary_VisualScenesWithSkeleton();
        }
        else
            WriteLibrary_VisualScenes();

        // Write animations
        //if (_cryData.Animations is not null)
        //    WriteLibrary_Animations();
    }

    protected void WriteColladaRoot(string version)
    {
        // Blender doesn't like 1.5. :(
        DaeObject.Collada_Version = version;
    }

    protected void WriteAsset()
    {
        var fileCreated = DateTime.Now;
        var fileModified = DateTime.Now;
        ColladaAsset asset = new()
        {
            Revision = Assembly.GetExecutingAssembly().GetName().Version.ToString()
        };
        ColladaAssetContributor[] contributors = new ColladaAssetContributor[1];
        // Get the actual file creators from the Source Chunk
        contributors[0] = new ColladaAssetContributor();
        foreach (ChunkSourceInfo sourceInfo in _cryData.Chunks.Where(a => a.ChunkType == ChunkType.SourceInfo))
        {
            contributors[0].Author = sourceInfo.Author;
            contributors[0].Source_Data = sourceInfo.SourceFile;
            contributors[0].Source_Data = _cryData.RootNode?.Name ?? Path.GetFileNameWithoutExtension(_cryData.InputFile);
        }
        asset.Created = fileCreated;
        asset.Modified = fileModified;
        asset.Up_Axis = "Z_UP";
        asset.Unit = new ColladaAssetUnit()
        {
            Meter = 1.0,
            Name = "meter"
        };
        asset.Title = _cryData.RootNode?.Name ?? Path.GetFileNameWithoutExtension(_cryData.InputFile);
        DaeObject.Asset = asset;
        DaeObject.Asset.Contributor = contributors;
    }

    public void WriteLibrary_Animations()
    {
        var animationLibrary = new ColladaLibraryAnimations();
        // Find all the 905 controller chunks
        foreach (var animChunk in _cryData.Animations
            .SelectMany(x => x.ChunkMap.Values.OfType<ChunkController_905>())
            .ToList())
        {
            var names = animChunk?.Animations?.Select(x => Path.GetFileNameWithoutExtension(x.Name)).ToArray();
            animationLibrary.Animation = new ColladaAnimation[animChunk?.Animations?.Count ?? 0];
            if (animationLibrary.Animation.Length == 0)
                continue;

            for (int i = 0; i < animChunk.Animations.Count; i++)
            {
                var animation = animChunk.Animations[i];
                var animationName = Path.GetFileNameWithoutExtension(animation.Name);

                var colladaAnimation = new ColladaAnimation  // Root animation object. Controller animations go here.
                {
                    Name = animationName,
                    ID = $"{animationName}_animation",
                    Animation = new ColladaAnimation[animation.Controllers.Count]
                };

                // Add an animation for each controller
                var controllerAnimations = new List<ColladaAnimation>();
                for (int j = 0; j < animation.Controllers.Count; j++)
                {
                    // Each controller can have up to 2 sub Animations, one for position and one for rotation
                    var controllerInfo = animation.Controllers[j];
                    var controllerBoneName = controllerIdToBoneName[controllerInfo.ControllerID];
                    var controllerIdBase = $"{controllerBoneName}_{controllerInfo.ControllerID}";

                    if (animation.Controllers[j].HasPosTrack)
                        controllerAnimations.Add(CreateAnimation(controllerInfo, "translate", animChunk));

                    if (animation.Controllers[j].HasRotTrack)
                        controllerAnimations.Add(CreateAnimation(controllerInfo, "rotate", animChunk));

                    colladaAnimation.Animation[j] = new ColladaAnimation
                    {
                        Name = controllerIdBase,
                        ID = $"{controllerIdBase}_animation",
                        Animation = controllerAnimations.ToArray()
                    };
                    controllerAnimations.Clear();
                }
                animationLibrary.Animation[i] = colladaAnimation;
            }
        }

        DaeObject.Library_Animations = animationLibrary;
    }

    private ColladaAnimation CreateAnimation(
        ChunkController_905.CControllerInfo controllerInfo,
        string animType,
        ChunkController_905 animationChunk)
    {
        var controllerBoneName = controllerIdToBoneName[controllerInfo.ControllerID];
        var controllerIdBase = $"{controllerBoneName}_{controllerInfo.ControllerID}_{animType}";

        var numberOfTimeFrames = animType == "rotate"
                ? animationChunk.KeyTimes[controllerInfo.RotKeyTimeTrack].Count
                : animationChunk.KeyTimes[controllerInfo.PosKeyTimeTrack].Count;

        var pathName = animType == "rotate" ? "rotation" : "location";

        var inputSource = new ColladaSource   // Time
        {
            ID = $"{controllerIdBase}_input",
            Name = $"{controllerIdBase}_input"
        };
        var outputSource = new ColladaSource     // transform
        {
            ID = $"{controllerIdBase}_output",
            Name = $"{controllerIdBase}_output"
        };
        var interpolationSource = new ColladaSource  // interpolation
        {
            ID = $"{controllerIdBase}_interpolation",
            Name = $"{controllerIdBase}_interpolation"
        };
        var sampler = new ColladaSampler
        {
            ID = $"{controllerIdBase}_sampler_{animType}",
            Input = new ColladaInputUnshared[3]
            {
                new ColladaInputUnshared
                {
                    Semantic = ColladaInputSemantic.INPUT,
                    source = $"#{controllerIdBase}_input"
                },
                new ColladaInputUnshared
                {
                    Semantic = ColladaInputSemantic.OUTPUT,
                    source = $"#{controllerIdBase}_output"
                },
                new ColladaInputUnshared
                {
                    Semantic = ColladaInputSemantic.INTERPOLATION,
                    source = $"#{controllerIdBase}_interpolation"
                }
            }
        };
        var channel = new ColladaChannel
        {
            Source = $"#{controllerIdBase}_sampler_{animType}",
            Target = $"{controllerBoneName}/matrix"
        };
        var controllerAnimation = new ColladaAnimation
        {
            Name = controllerIdBase,
            ID = $"{controllerIdBase}_animation",
            Source = new ColladaSource[3] { inputSource, outputSource, interpolationSource },
            Channel = new ColladaChannel[1] { channel },
            Sampler = new ColladaSampler[1] { sampler },
        };

        // Create the time source
        var timeArray = new ColladaFloatArray
        {
            ID = $"{controllerBoneName}_{controllerInfo.ControllerID}_{animType}_time_array",
            Count = numberOfTimeFrames,
            Value_As_String = string.Join(" ",
                animType == "rotate"
                    ? animationChunk.KeyTimes[controllerInfo.RotKeyTimeTrack].Select(x => x / 30f)
                    : animationChunk.KeyTimes[controllerInfo.PosKeyTimeTrack].Select(x => x / 30f))
        };

        inputSource.Float_Array = timeArray;
        inputSource.Technique_Common = new ColladaTechniqueCommonSource
        {
            Accessor = new ColladaAccessor
            {
                Source = $"#{controllerBoneName}_{controllerInfo.ControllerID}_{animType}_time_array",
                Count = (uint)timeArray.Count,
                Stride = 1,
                Param = new ColladaParam[1] { new ColladaParam { Name = "TIME", Type = "float" } }
            }
        };

        // Create the transform source
        var numberOfElements = animType == "rotate" ? 4 : 3;

        var colladaParams = new ColladaParam[1];
        if (animType == "rotate")
            colladaParams[0] = new ColladaParam { Name = "AXISANGLE", Type = "vector4" };
        else
            colladaParams[0] = new ColladaParam { Name = "TRANSLATE", Type = "vector3" };

        var positionArray = new ColladaFloatArray
        {
            ID = $"{controllerBoneName}_{controllerInfo.ControllerID}_{animType}_array",
            Count = numberOfTimeFrames * numberOfElements,
            Value_As_String = string.Join(" ",
                animType == "rotate"
                    ? animationChunk.KeyRotations[controllerInfo.RotTrack].Select(x => CreateStringFromVector4(x.ToAxisAngle()))
                    : animationChunk.KeyPositions[controllerInfo.PosTrack].Select(x => CreateStringFromVector3(x)))
        };
        outputSource.Float_Array = positionArray;
        //var paramType = animType == "rotate" ? "vector4" : "vector3";
        outputSource.Technique_Common = new ColladaTechniqueCommonSource
        {
            Accessor = new ColladaAccessor
            {
                Source = $"#{controllerBoneName}_{controllerInfo.ControllerID}_{animType}_array",
                Count = (uint)numberOfTimeFrames,
                Stride = (uint)numberOfElements,
                Param = colladaParams
            }
        };

        // Create the interpolation source (all LINEAR)
        var interpolationArray = new ColladaNameArray
        {
            ID = $"{controllerBoneName}_{controllerInfo.ControllerID}_{animType}_interpolation_array",
            Count = numberOfTimeFrames,
            Value_Pre_Parse = string.Join(' ', Enumerable.Repeat("LINEAR", numberOfTimeFrames))
        };
        interpolationSource.Name_Array = interpolationArray;
        interpolationSource.Technique_Common = new ColladaTechniqueCommonSource
        {
            Accessor = new ColladaAccessor
            {
                Source = $"#{controllerBoneName}_{controllerInfo.ControllerID}_{animType}_interpolation_array",
                Count = (uint)numberOfTimeFrames,
                Stride = 1,
                Param = new ColladaParam[1] { new ColladaParam { Name = "INTERPOLATION", Type = "name" } }
            }
        };

        return controllerAnimation;
    }

    public ColladaEffect CreateColladaEffect(string matKey, Material subMat)
    {
        //TODO: Change this so that it creates an effect for each Shader type.  Parameters will need to be passed in from the material.
        var effectName = GetMaterialName(matKey, subMat.Name);

        ColladaEffect colladaEffect = new()
        {
            ID = effectName + "-effect",
            Name = effectName
        };

        // create the profile_common for the effect
        List<ColladaProfileCOMMON> profiles = new();
        ColladaProfileCOMMON profile = new();
        profile.Technique = new() { sID = effectName + "-technique" };
        profiles.Add(profile);

        // Create a list for the new_params
        List<ColladaNewParam> newparams = new();
        for (int j = 0; j < subMat.Textures.Length; j++)
        {
            // Add the Surface node
            ColladaNewParam texSurface = new()
            {
                sID = effectName + "_" + subMat.Textures[j].Map + "-surface"
            };
            ColladaSurface surface = new();
            texSurface.Surface = surface;
            surface.Init_From = new ColladaInitFrom();
            texSurface.Surface.Type = "2D";
            texSurface.Surface.Init_From = new ColladaInitFrom
            {
                Uri = effectName + "_" + subMat.Textures[j].Map
            };

            // Add the Sampler node
            ColladaNewParam texSampler = new()
            {
                sID = effectName + "_" + subMat.Textures[j].Map + "-sampler"
            };
            ColladaSampler2D sampler2D = new();
            texSampler.Sampler2D = sampler2D;
            texSampler.Sampler2D.Source = texSurface.sID;

            newparams.Add(texSurface);
            newparams.Add(texSampler);
        }

        #region Create the Technique
        // Make the techniques for the profile
        ColladaEffectTechniqueCOMMON technique = new();
        ColladaPhong phong = new();
        technique.Phong = phong;
        technique.sID = effectName + "-technique";
        profile.Technique = technique;

        phong.Diffuse = new ColladaFXCommonColorOrTextureType();
        phong.Specular = new ColladaFXCommonColorOrTextureType();

        // Add all the emissive, etc features to the phong
        // Need to check if a texture exists.  If so, refer to the sampler.  Should be a <Texture Map="Diffuse" line if there is a map.
        bool diffuseFound = false;
        bool specularFound = false;

        foreach (var texture in subMat.Textures)
        {
            if (texture.Map == Texture.MapTypeEnum.Diffuse)
            {
                diffuseFound = true;
                phong.Diffuse.Texture = new ColladaTexture
                {
                    // Texcoord is the ID of the UV source in geometries.  Not needed.
                    Texture = effectName + "_" + texture.Map + "-sampler",
                    TexCoord = ""
                };
            }

            if (texture.Map == Texture.MapTypeEnum.Specular)
            {
                specularFound = true;
                phong.Specular.Texture = new ColladaTexture
                {
                    Texture = effectName + "_" + texture.Map + "-sampler",
                    TexCoord = ""
                };
            }

            if (texture.Map == Texture.MapTypeEnum.Normals)
            {
                // Bump maps go in an extra node.
                ColladaExtra[] extras = new ColladaExtra[1];
                ColladaExtra extra = new();
                extras[0] = extra;

                technique.Extra = extras;

                // Create the technique for the extra
                ColladaTechnique[] extraTechniques = new ColladaTechnique[1];
                ColladaTechnique extraTechnique = new();
                extra.Technique = extraTechniques;

                extraTechniques[0] = extraTechnique;
                extraTechnique.profile = "FCOLLADA";

                ColladaBumpMap bumpMap = new() { Textures = new ColladaTexture[1] };
                bumpMap.Textures[0] = new ColladaTexture
                {
                    Texture = effectName + "_" + texture.Map + "-sampler"
                };
                extraTechnique.Data = new XmlElement[1] { bumpMap };
            }
        }

        if (diffuseFound == false)
        {
            phong.Diffuse.Color = new ColladaColor
            {
                Value_As_String = subMat.Diffuse ?? string.Empty,
                sID = "diffuse"
            };
        }
        if (specularFound == false)
        {
            phong.Specular.Color = new ColladaColor { sID = "specular" };
            if (subMat.Specular != null)
                phong.Specular.Color.Value_As_String = subMat.Specular ?? string.Empty;
            else
                phong.Specular.Color.Value_As_String = "1 1 1";
        }

        phong.Emission = new ColladaFXCommonColorOrTextureType
        {
            Color = new ColladaColor
            {
                sID = "emission",
                Value_As_String = subMat.Emissive ?? string.Empty
            }
        };
        phong.Shininess = new ColladaFXCommonFloatOrParamType { Float = new ColladaSIDFloat() };
        phong.Shininess.Float.sID = "shininess";
        phong.Shininess.Float.Value = (float)subMat.Shininess;
        phong.Index_Of_Refraction = new ColladaFXCommonFloatOrParamType { Float = new ColladaSIDFloat() };

        phong.Transparent = new ColladaFXCommonColorOrTextureType
        {
            Color = new ColladaColor(),
            Opaque = new ColladaFXOpaqueChannel()
        };
        phong.Transparent.Color.Value_As_String = (1 - double.Parse((subMat.Opacity == string.Empty ? "1" : subMat.Opacity) ?? "1")).ToString();  // Subtract from 1 for proper value.

        #endregion

        colladaEffect.Profile_COMMON = profiles.ToArray();
        profile.New_Param = new ColladaNewParam[newparams.Count];
        profile.New_Param = newparams.ToArray();

        return colladaEffect;
    }

    public void WriteLibrary_Geometries()
    {
        if (_cryData.Models.Count == 1)  // Single file model
            WriteGeometries(_cryData.Models[0]);
        else
            WriteGeometries(_cryData.Models[1]);
    }

    public void WriteGeometries(Model model)
    {
        ColladaLibraryGeometries libraryGeometries = new();

        // Make a list for all the geometries objects we will need. Will convert to array at end.  Define the array here as well
        // We have to define a Geometry for EACH meshsubset in the meshsubsets, since the mesh can contain multiple materials
        List<ColladaGeometry> geometryList = new();

        // For each of the nodes, we need to write the geometry.
        foreach (ChunkNode nodeChunk in model.ChunkMap.Values.Where(a => a.ChunkType == ChunkType.Node))
        {
            // Create a geometry object.  Use the chunk ID for the geometry ID
            // Create all the materials used by this chunk.
            // Will have to be careful with this, since with .cga/.cgam pairs will need to match by Name.
            // Make the mesh object.  This will have 3 or 4 sources, 1 vertices, and 1 or more Triangles (with material ID)
            // If the Object ID of Node chunk points to a Helper or a Controller though, place an empty.
            ChunkDataStream? normals = null;
            ChunkDataStream? uvs = null;
            ChunkDataStream? tmpVertices = null;
            ChunkDataStream? vertsUvs = null;
            ChunkDataStream? indices = null;
            ChunkDataStream? colors = null;
            ChunkDataStream? tangents = null;

            if (_args.IsNodeNameExcluded(nodeChunk.Name))
            {
                Log(LogLevelEnum.Debug, $"Excluding node {nodeChunk.Name}");
                continue;
            }

            if (nodeChunk.ObjectChunk is null)
            {
                Log(LogLevelEnum.Warning, "Skipped node with missing Object {0}", nodeChunk.Name);
                continue;
            }

            if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkType.Mesh)
            {
                // Create materials collection for this node. Index of collection in meshSubSets determines which mat to use.
                //CreateMaterialsFromNodeChunk(nodeChunk); // now being created from the crydata Materials object

                // Get the mesh chunk and submesh chunk for this node.
                var meshChunk = (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];

                // Check to see if the Mesh points to a PhysicsData mesh.  Don't want to write these.
                //if (meshChunk.MeshPhysicsData != 0)
                // TODO:  Implement this chunk

                if (meshChunk.MeshSubsetsData != 0)   // You can have Mesh chunks with no Mesh Subset.  Need to skip these.  They are in the .cga file and contain no geometry.
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

                    // geometry is a Geometry object for each meshsubset.  Name will be "Nodechunk name_matID".
                    ColladaGeometry geometry = new()
                    {
                        Name = nodeChunk.Name,
                        ID = nodeChunk.Name + "-mesh"
                    };
                    ColladaMesh colladaMesh = new();
                    geometry.Mesh = colladaMesh;

                    // TODO:  Move the source creation to a separate function.  Too much retyping.
                    ColladaSource[] source = new ColladaSource[4];   // 4 possible source types.
                    ColladaSource posSource = new();
                    ColladaSource normSource = new();
                    ColladaSource uvSource = new();
                    ColladaSource colorSource = new();
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

                    ColladaVertices vertices = new() { ID = nodeChunk.Name + "-vertices" };
                    geometry.Mesh.Vertices = vertices;
                    ColladaInputUnshared[] inputshared = new ColladaInputUnshared[4];
                    ColladaInputUnshared posInput = new() { Semantic = ColladaInputSemantic.POSITION };
                    vertices.Input = inputshared;

                    ColladaInputUnshared normInput = new() { Semantic = ColladaInputSemantic.NORMAL };
                    ColladaInputUnshared uvInput = new() { Semantic = ColladaInputSemantic.TEXCOORD };
                    ColladaInputUnshared colorInput = new() { Semantic = ColladaInputSemantic.COLOR };

                    posInput.source = "#" + posSource.ID;
                    normInput.source = "#" + normSource.ID;
                    uvInput.source = "#" + uvSource.ID;
                    colorInput.source = "#" + colorSource.ID;
                    inputshared[0] = posInput;

                    ColladaFloatArray floatArrayVerts = new();
                    ColladaFloatArray floatArrayNormals = new();
                    ColladaFloatArray floatArrayUVs = new();
                    ColladaFloatArray floatArrayColors = new();
                    ColladaFloatArray floatArrayTangents = new();

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
                        if (normals is not null)
                            floatArrayNormals.Count = (int)normals.NumElements * 3;
                        floatArrayColors.ID = colorSource.ID + "-array";
                        floatArrayColors.Digits = 6;
                        floatArrayColors.Magnitude = 38;
                        if (colors is not null)
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
                        if (vertsUvs.Colors is not null)
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
                        // TODO: Move this to the cryengine data.
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
                            if (!_cryData.InputFile.EndsWith("skin") && !_cryData.InputFile.EndsWith("chr"))
                                vertex = (vertex * multiplerVector) + boundaryBoxCenter;

                            vertString.AppendFormat("{0:F6} {1:F6} {2:F6} ", Safe(vertex.X), Safe(vertex.Y), Safe(vertex.Z));

                            // TODO:  This isn't right?  VertsUvs may always have color as the 3rd element.
                            // Normals depend on the data size.  16 byte structures have the normals in the Tangents.  20 byte structures are in the VertsUV.
                            Vector3 normal = new();
                            if (vertsUvs.Normals is not null)
                                normal = vertsUvs.Normals[j];
                            else if (tangents is not null && tangents.Normals is not null)
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
                    var triangles = new ColladaTriangles[meshSubsets.NumMeshSubset];
                    geometry.Mesh.Triangles = triangles;

                    for (uint j = 0; j < meshSubsets.NumMeshSubset; j++) // Need to make a new Triangles entry for each submesh.
                    {
                        // Find the material associated with this meshsubset and index.  Normally the nodechunk points to the mtlnamechunk, but
                        // in mwo models it can point to mechDefault.  First check to see if the material key for the nodechunk's mtlname chunk exists.
                        // If it does, use that material.  If not, assume just a single materialfile and use that.

                        //var mtlNameChunk = nodeChunk.MaterialID == 0
                        //    ? 
                        //    :
                        //    ;
                        var mtlNameChunk = (ChunkMtlName)_cryData.Models.Last().ChunkMap[nodeChunk.MaterialID];
                        var mtlFileName = mtlNameChunk.Name;
                        var key = Path.GetFileNameWithoutExtension(mtlFileName);
                        Material[] submats;
                        if (_cryData.Materials.ContainsKey(key))
                            submats = _cryData.Materials[key].SubMaterials;
                        else
                        {
                            submats = _cryData.Materials.FirstOrDefault().Value.SubMaterials;
                            mtlFileName = Path.GetFileNameWithoutExtension(_cryData.MaterialFiles.FirstOrDefault());
                        }

                        triangles[j] = new ColladaTriangles
                        {
                            Count = meshSubsets.MeshSubsets[j].NumIndices / 3,
                            Material = GetMaterialName(mtlFileName, submats[meshSubsets.MeshSubsets[j].MatID].Name) + "-material"
                            //Material = GetMaterialId(nodeChunk, meshSubsets, (int)j)
                        };

                        // Create the inputs.  vertex, normal, texcoord, color
                        int inputCount = 3;
                        if (colors != null || vertsUvs?.Colors != null)
                            inputCount++;

                        triangles[j].Input = new ColladaInputShared[inputCount];

                        triangles[j].Input[0] = new ColladaInputShared
                        {
                            Semantic = new ColladaInputSemantic()
                        };
                        triangles[j].Input[0].Semantic = ColladaInputSemantic.VERTEX;
                        triangles[j].Input[0].Offset = 0;
                        triangles[j].Input[0].source = "#" + vertices.ID;
                        triangles[j].Input[1] = new ColladaInputShared
                        {
                            Semantic = ColladaInputSemantic.NORMAL,
                            Offset = 1,
                            source = "#" + normSource.ID
                        };
                        triangles[j].Input[2] = new ColladaInputShared
                        {
                            Semantic = ColladaInputSemantic.TEXCOORD,
                            Offset = 2,
                            source = "#" + uvSource.ID
                        };

                        int nextInputID = 3;
                        if (colors != null || vertsUvs?.Colors != null)
                        {
                            triangles[j].Input[nextInputID] = new ColladaInputShared
                            {
                                Semantic = ColladaInputSemantic.COLOR,
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
                        triangles[j].P = new ColladaIntArrayString
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
                    posSource.Technique_Common = new ColladaTechniqueCommonSource
                    {
                        Accessor = new ColladaAccessor()
                    };
                    posSource.Technique_Common.Accessor.Source = "#" + floatArrayVerts.ID;
                    posSource.Technique_Common.Accessor.Stride = 3;
                    posSource.Technique_Common.Accessor.Count = (uint)meshChunk.NumVertices;
                    ColladaParam[] paramPos = new ColladaParam[3];
                    paramPos[0] = new ColladaParam();
                    paramPos[1] = new ColladaParam();
                    paramPos[2] = new ColladaParam();
                    paramPos[0].Name = "X";
                    paramPos[0].Type = "float";
                    paramPos[1].Name = "Y";
                    paramPos[1].Type = "float";
                    paramPos[2].Name = "Z";
                    paramPos[2].Type = "float";
                    posSource.Technique_Common.Accessor.Param = paramPos;

                    normSource.Technique_Common = new ColladaTechniqueCommonSource
                    {
                        Accessor = new ColladaAccessor
                        {
                            Source = "#" + floatArrayNormals.ID,
                            Stride = 3,
                            Count = (uint)meshChunk.NumVertices
                        }
                    };
                    ColladaParam[] paramNorm = new ColladaParam[3];
                    paramNorm[0] = new ColladaParam();
                    paramNorm[1] = new ColladaParam();
                    paramNorm[2] = new ColladaParam();
                    paramNorm[0].Name = "X";
                    paramNorm[0].Type = "float";
                    paramNorm[1].Name = "Y";
                    paramNorm[1].Type = "float";
                    paramNorm[2].Name = "Z";
                    paramNorm[2].Type = "float";
                    normSource.Technique_Common.Accessor.Param = paramNorm;

                    uvSource.Technique_Common = new ColladaTechniqueCommonSource
                    {
                        Accessor = new ColladaAccessor
                        {
                            Source = "#" + floatArrayUVs.ID,
                            Stride = 2
                        }
                    };

                    if (tmpVertices != null)
                        uvSource.Technique_Common.Accessor.Count = uvs.NumElements;
                    else
                        uvSource.Technique_Common.Accessor.Count = vertsUvs.NumElements;

                    ColladaParam[] paramUV = new ColladaParam[2];
                    paramUV[0] = new ColladaParam();
                    paramUV[1] = new ColladaParam();
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

                        colorSource.Technique_Common = new ColladaTechniqueCommonSource
                        {
                            Accessor = new ColladaAccessor()
                        };
                        colorSource.Technique_Common.Accessor.Source = "#" + floatArrayColors.ID;
                        colorSource.Technique_Common.Accessor.Stride = 4;
                        colorSource.Technique_Common.Accessor.Count = numberOfElements;
                        ColladaParam[] paramColor = new ColladaParam[4];
                        paramColor[0] = new ColladaParam();
                        paramColor[1] = new ColladaParam();
                        paramColor[2] = new ColladaParam();
                        paramColor[3] = new ColladaParam();
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

    private void CreateMaterials()
    {
        List<ColladaMaterial> colladaMaterials = new();
        List<ColladaEffect> colladaEffects = new();
        if (DaeObject.Library_Materials?.Material is null)
            DaeObject.Library_Materials.Material = Array.Empty<ColladaMaterial>();

        foreach (var matKey in _cryData.Materials.Keys)
        {
            foreach (var subMat in _cryData.Materials[matKey].SubMaterials)
            {
                // Check to see if the collada object already has a material with this name.  If not, add.
                var matNames = DaeObject.Library_Materials.Material.Select(c => c.Name);
                if (!matNames.Contains(subMat.Name))
                {
                    colladaMaterials.Add(AddMaterialToMaterialLibrary(matKey, subMat));
                    colladaEffects.Add(CreateColladaEffect(matKey, subMat));
                }
            }
        }

        int arraySize = DaeObject.Library_Materials.Material.Length;
        Array.Resize(ref DaeObject.Library_Materials.Material, DaeObject.Library_Materials.Material.Length + colladaMaterials.Count);
        colladaMaterials.CopyTo(DaeObject.Library_Materials.Material, arraySize);

        if (DaeObject.Library_Effects.Effect is null)
            DaeObject.Library_Effects.Effect = colladaEffects.ToArray();
        else
        {
            int effectsArraySize = DaeObject.Library_Effects.Effect.Length;
            Array.Resize(ref DaeObject.Library_Effects.Effect, DaeObject.Library_Effects.Effect.Length + colladaEffects.Count);
            colladaEffects.CopyTo(DaeObject.Library_Effects.Effect, effectsArraySize);
        }
    }

    private ColladaMaterial AddMaterialToMaterialLibrary(string matKey, Material submat)
    {
        var matName = GetMaterialName(matKey, submat?.Name ?? "unknown");
        ColladaMaterial material = new()
        {
            Instance_Effect = new ColladaInstanceEffect(),
            Name = matName,
            ID = matName + "-material"
        };
        material.Instance_Effect.URL = "#" + matName + "-effect";

        AddTexturesToTextureLibrary(matKey, submat);
        return material;
    }

    private void AddTexturesToTextureLibrary(string matKey, Material submat)
    {
        List<ColladaImage> imageList = new();
        int numberOfTextures = submat.Textures?.Length ?? 0;

        for (int i = 0; i < numberOfTextures; i++)
        {
            // For each texture in the material, we make a new <image> object and add it to the list.
            var name = GetMaterialName(matKey, submat.Name);
            ColladaImage image = new()
            {
                ID = name + "_" + submat.Textures[i].Map,
                Name = name + "_" + submat.Textures[i].Map,
                Init_From = new ColladaInitFrom()
            };
            // TODO: Refactor to use fully qualified path, and to use Pack File System.
            var textureFile = ResolveTextureFile(submat.Textures[i].File, _args.PackFileSystem, _args.DataDirs);

            if (_args.PngTextures && File.Exists(Path.ChangeExtension(textureFile, ".png")))
                textureFile = Path.ChangeExtension(textureFile, ".png");
            else if (_args.TgaTextures && File.Exists(Path.ChangeExtension(textureFile, ".tga")))
                textureFile = Path.ChangeExtension(textureFile, ".tga");
            else if (_args.TiffTextures && File.Exists(Path.ChangeExtension(textureFile, ".tif")))
                textureFile = Path.ChangeExtension(textureFile, ".tif");

            textureFile = Path.GetRelativePath(daeOutputFile.DirectoryName, textureFile);

            textureFile = textureFile.Replace(" ", @"%20");
            image.Init_From.Uri = textureFile;
            imageList.Add(image);
        }

        ColladaImage[] images = imageList.ToArray();
        if (DaeObject.Library_Images.Image is null)
            DaeObject.Library_Images.Image = images;
        else
        {
            int arraySize = DaeObject.Library_Images.Image.Length;
            Array.Resize(ref DaeObject.Library_Images.Image, DaeObject.Library_Images.Image.Length + images.Length);
            images.CopyTo(DaeObject.Library_Images.Image, arraySize);
        }
    }

    private void WriteLibrary_Controllers()
    {
        if (DaeObject.Library_Geometries.Geometry.Length != 0)
        {
            ColladaLibraryControllers libraryController = new();

            // There can be multiple controllers in the controller library.  But for Cryengine files, there is only one rig.
            // So if a rig exists, make that the controller.  This applies mostly to .chr files, which will have a rig and may have geometry.
            ColladaController controller = new() { ID = "Controller" };
            // Create the skin object and assign to the controller
            ColladaSkin skin = new()
            {
                source = "#" + DaeObject.Library_Geometries.Geometry[0].ID,
                Bind_Shape_Matrix = new ColladaFloatArrayString()
            };
            skin.Bind_Shape_Matrix.Value_As_String = CreateStringFromMatrix4x4(Matrix4x4.Identity);  // We will assume the BSM is the identity matrix for now

            // Create the 3 sources for this controller:  joints, bind poses, and weights
            skin.Source = new ColladaSource[3];

            // Populate the data.
            // Need to map the exterior vertices (geometry) to the int vertices.  Or use the Bone Map datastream if it exists (check HasBoneMapDatastream).
            #region Joints Source
            ColladaSource jointsSource = new()
            {
                ID = "Controller-joints",
                Name_Array = new ColladaNameArray()
                {
                    ID = "Controller-joints-array",
                    Count = _cryData.SkinningInfo.CompiledBones.Count,
                }
            };
            StringBuilder boneNames = new();
            for (int i = 0; i < _cryData.SkinningInfo.CompiledBones.Count; i++)
            {
                boneNames.Append(_cryData.SkinningInfo.CompiledBones[i].boneName.Replace(' ', '_') + " ");
            }
            jointsSource.Name_Array.Value_Pre_Parse = boneNames.ToString().TrimEnd();
            jointsSource.Technique_Common = new ColladaTechniqueCommonSource
            {
                Accessor = new ColladaAccessor
                {
                    Source = "#Controller-joints-array",
                    Count = (uint)_cryData.SkinningInfo.CompiledBones.Count,
                    Stride = 1
                }
            };
            skin.Source[0] = jointsSource;
            #endregion

            #region Bind Pose Array Source
            ColladaSource bindPoseArraySource = new()
            {
                ID = "Controller-bind_poses",
                Float_Array = new()
                {
                    ID = "Controller-bind_poses-array",
                    Count = _cryData.SkinningInfo.CompiledBones.Count * 16,
                    Value_As_String = GetBindPoseArray(_cryData.SkinningInfo.CompiledBones)
                },
                Technique_Common = new ColladaTechniqueCommonSource
                {
                    Accessor = new ColladaAccessor
                    {
                        Source = "#Controller-bind_poses-array",
                        Count = (uint)_cryData.SkinningInfo.CompiledBones.Count,
                        Stride = 16,
                    }
                }
            };
            bindPoseArraySource.Technique_Common.Accessor.Param = new ColladaParam[1];
            bindPoseArraySource.Technique_Common.Accessor.Param[0] = new ColladaParam
            {
                Name = "TRANSFORM",
                Type = "float4x4"
            };
            skin.Source[1] = bindPoseArraySource;
            #endregion

            #region Weights Source
            ColladaSource weightArraySource = new()
            {
                ID = "Controller-weights",
                Technique_Common = new ColladaTechniqueCommonSource()
            };
            ColladaAccessor accessor = weightArraySource.Technique_Common.Accessor = new ColladaAccessor();

            weightArraySource.Float_Array = new ColladaFloatArray()
            {
                ID = "Controller-weights-array",
            };
            StringBuilder weights = new();

            if (_cryData.SkinningInfo.IntVertices is null)       // This is a case where there are bones, and only Bone Mapping data from a datastream chunk.  Skin files.
            {
                weightArraySource.Float_Array.Count = _cryData.SkinningInfo.BoneMapping.Count;
                for (int i = 0; i < _cryData.SkinningInfo.BoneMapping.Count; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        weights.Append(((float)_cryData.SkinningInfo.BoneMapping[i].Weight[j] / 255).ToString() + " ");
                    }
                };
                accessor.Count = (uint)_cryData.SkinningInfo.BoneMapping.Count * 4;
            }
            else                                                // Bones and int verts.  Will use int verts for weights, but this doesn't seem perfect either.
            {
                weightArraySource.Float_Array.Count = _cryData.SkinningInfo.Ext2IntMap.Count;
                for (int i = 0; i < _cryData.SkinningInfo.Ext2IntMap.Count; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        weights.Append(_cryData.SkinningInfo.IntVertices[_cryData.SkinningInfo.Ext2IntMap[i]].Weights[j] + " ");
                    }
                    accessor.Count = (uint)_cryData.SkinningInfo.Ext2IntMap.Count * 4;
                };
            }
            CleanNumbers(weights);
            weightArraySource.Float_Array.Value_As_String = weights.ToString().TrimEnd();
            // Add technique_common part.
            accessor.Source = "#Controller-weights-array";
            accessor.Stride = 1;
            accessor.Param = new ColladaParam[1];
            accessor.Param[0] = new ColladaParam
            {
                Name = "WEIGHT",
                Type = "float"
            };
            skin.Source[2] = weightArraySource;

            #endregion

            #region Joints
            skin.Joints = new ColladaJoints
            {
                Input = new ColladaInputUnshared[2]
            };
            skin.Joints.Input[0] = new ColladaInputUnshared
            {
                Semantic = new ColladaInputSemantic()
            };
            skin.Joints.Input[0].Semantic = ColladaInputSemantic.JOINT;
            skin.Joints.Input[0].source = "#Controller-joints";
            skin.Joints.Input[1] = new ColladaInputUnshared
            {
                Semantic = new ColladaInputSemantic()
            };
            skin.Joints.Input[1].Semantic = ColladaInputSemantic.INV_BIND_MATRIX;
            skin.Joints.Input[1].source = "#Controller-bind_poses";
            #endregion

            #region Vertex Weights
            ColladaVertexWeights vertexWeights = skin.Vertex_Weights = new();
            vertexWeights.Count = _cryData.SkinningInfo.BoneMapping.Count;
            skin.Vertex_Weights.Input = new ColladaInputShared[2];
            ColladaInputShared jointSemantic = skin.Vertex_Weights.Input[0] = new();
            jointSemantic.Semantic = ColladaInputSemantic.JOINT;
            jointSemantic.source = "#Controller-joints";
            jointSemantic.Offset = 0;
            ColladaInputShared weightSemantic = skin.Vertex_Weights.Input[1] = new();
            weightSemantic.Semantic = ColladaInputSemantic.WEIGHT;
            weightSemantic.source = "#Controller-weights";
            weightSemantic.Offset = 1;
            StringBuilder vCount = new();
            //for (int i = 0; i < CryData.Models[0].SkinningInfo.IntVertices.Count; i++)
            for (int i = 0; i < _cryData.SkinningInfo.BoneMapping.Count; i++)
            {
                vCount.Append("4 ");
            };
            vertexWeights.VCount = new ColladaIntArrayString
            {
                Value_As_String = vCount.ToString().TrimEnd()
            };
            StringBuilder vertices = new();
            //for (int i = 0; i < CryData.Models[0].SkinningInfo.IntVertices.Count * 4; i++)
            int index = 0;
            if (!_cryData.SkinningInfo.HasIntToExtMapping)
            {
                for (int i = 0; i < _cryData.SkinningInfo.BoneMapping.Count; i++)
                {
                    vertices.Append(_cryData.SkinningInfo.BoneMapping[i].BoneIndex[0] + " " + index + " ");
                    vertices.Append(_cryData.SkinningInfo.BoneMapping[i].BoneIndex[1] + " " + (index + 1) + " ");
                    vertices.Append(_cryData.SkinningInfo.BoneMapping[i].BoneIndex[2] + " " + (index + 2) + " ");
                    vertices.Append(_cryData.SkinningInfo.BoneMapping[i].BoneIndex[3] + " " + (index + 3) + " ");
                    index += 4;
                }
            }
            else
            {
                for (int i = 0; i < _cryData.SkinningInfo.Ext2IntMap.Count; i++)
                {
                    vertices.Append(_cryData.SkinningInfo.IntVertices[_cryData.SkinningInfo.Ext2IntMap[i]].BoneIDs[0] + " " + index + " ");
                    vertices.Append(_cryData.SkinningInfo.IntVertices[_cryData.SkinningInfo.Ext2IntMap[i]].BoneIDs[1] + " " + (index + 1) + " ");
                    vertices.Append(_cryData.SkinningInfo.IntVertices[_cryData.SkinningInfo.Ext2IntMap[i]].BoneIDs[2] + " " + (index + 2) + " ");
                    vertices.Append(_cryData.SkinningInfo.IntVertices[_cryData.SkinningInfo.Ext2IntMap[i]].BoneIDs[3] + " " + (index + 3) + " ");

                    index += 4;
                }
            }
            vertexWeights.V = new ColladaIntArrayString { Value_As_String = vertices.ToString().TrimEnd() };
            #endregion

            // create the extra element for the FCOLLADA profile
            controller.Extra = new ColladaExtra[1];
            controller.Extra[0] = new ColladaExtra
            {
                Technique = new ColladaTechnique[1]
            };
            controller.Extra[0].Technique[0] = new ColladaTechnique
            {
                profile = "FCOLLADA",
                UserProperties = "SkinController"
            };

            // Add the parts to their parents
            controller.Skin = skin;
            libraryController.Controller = new ColladaController[1];
            libraryController.Controller[0] = controller;
            DaeObject.Library_Controllers = libraryController;
        }
    }

    public void WriteLibrary_VisualScenes()
    {
        ColladaLibraryVisualScenes libraryVisualScenes = new();

        List<ColladaVisualScene> visualScenes = new();
        ColladaVisualScene visualScene = new();
        List<ColladaNode> nodes = new();

        // THERE CAN BE MULTIPLE ROOT NODES IN EACH FILE!  Check to see if the parentnodeid ~0 and be sure to add a node for it.
        List<ColladaNode> positionNodes = new();
        List<ChunkNode> positionRoots = _cryData.Models[0].NodeMap.Values.Where(a => a.ParentNodeID == ~0).ToList();
        foreach (ChunkNode root in positionRoots)
        {
            positionNodes.Add(CreateNode(root, false));
        }
        nodes.AddRange(positionNodes.ToArray());

        visualScene.Node = nodes.ToArray();
        visualScene.ID = "Scene";
        visualScenes.Add(visualScene);

        libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
        DaeObject.Library_Visual_Scene = libraryVisualScenes;
    }

    public void WriteLibrary_VisualScenesWithSkeleton()
    {
        ColladaLibraryVisualScenes libraryVisualScenes = new();

        List<ColladaVisualScene> visualScenes = new();
        ColladaVisualScene visualScene = new();
        List<ColladaNode> nodes = new();

        // Check to see if there is a CompiledBones chunk.  If so, add a Node.
        if (_cryData.Chunks.Any(a => a.ChunkType == ChunkType.CompiledBones ||
            a.ChunkType == ChunkType.CompiledBonesSC ||
            a.ChunkType == ChunkType.CompiledBonesIvo ||
            a.ChunkType == ChunkType.CompiledBonesIvo320))
        {
            ColladaNode boneNode = new();
            boneNode = CreateJointNode(_cryData.Bones.RootBone);
            nodes.Add(boneNode);
        }

        var hasGeometry = _cryData.Models.Any(x => x.HasGeometry);

        if (hasGeometry)
        {
            var modelIndex = _cryData.Models.First().IsIvoFile ? 1 : 0;
            var allParentNodes = _cryData.Models[modelIndex].NodeMap.Values.Where(n => n.ParentNodeID != ~1);

            foreach (var node in allParentNodes)
            {
                var colladaNode = CreateNode(node, true);
                colladaNode.Instance_Controller = new ColladaInstanceController[1];
                colladaNode.Instance_Controller[0] = new ColladaInstanceController
                {
                    URL = "#Controller",
                    Skeleton = new ColladaSkeleton[1]
                };

                var skeleton = colladaNode.Instance_Controller[0].Skeleton[0] = new ColladaSkeleton();
                skeleton.Value = $"#{_cryData.Bones.RootBone.boneName}".Replace(' ', '_');
                colladaNode.Instance_Controller[0].Bind_Material = new ColladaBindMaterial[1];
                ColladaBindMaterial bindMaterial = colladaNode.Instance_Controller[0].Bind_Material[0] = new ColladaBindMaterial();

                // Create an Instance_Material for each material
                bindMaterial.Technique_Common = new ColladaTechniqueCommonBindMaterial();
                colladaNode.Instance_Controller[0].Bind_Material[0].Technique_Common.Instance_Material = CreateInstanceMaterials(node);

                foreach (ChunkNode child in node.AllChildNodes)
                    CreateChildNodes(child, true);

                nodes.Add(colladaNode);
            }
        }

        visualScene.Node = nodes.ToArray();
        visualScene.ID = "Scene";
        visualScenes.Add(visualScene);

        libraryVisualScenes.Visual_Scene = visualScenes.ToArray();
        DaeObject.Library_Visual_Scene = libraryVisualScenes;
    }

    private ColladaInstanceMaterialGeometry[] CreateInstanceMaterials(ChunkNode node)
    {
        // For each mesh subset, we want to create an instance material and add it to instanceMaterials list.
        List<ColladaInstanceMaterialGeometry> instanceMaterials = new();
        ChunkMesh meshNode;
        ChunkMeshSubsets submeshNode;

        //ChunkMtlName mtlNameChunk = (ChunkMtlName)_cryData.Models[0].ChunkMap[node.MaterialID];
        
        if (_cryData.Models.Count > 1)
        {
            // Find the node in model[1] that has the same name as the node.
            var geoNode = _cryData.Models[1].NodeMap.Values.Where(a => a.Name == node.Name).FirstOrDefault();
            meshNode = (ChunkMesh)_cryData.Models.Last().ChunkMap[geoNode.ObjectNodeID];
            submeshNode = (ChunkMeshSubsets)_cryData.Models.Last().ChunkMap[meshNode.MeshSubsetsData];
        }
        else
        {
            meshNode = (ChunkMesh)_cryData.Models[0].ChunkMap[node.ObjectNodeID];
            submeshNode = (ChunkMeshSubsets)_cryData.Models[0].ChunkMap[meshNode.MeshSubsetsData];
        }

        for (int i = 0; i < submeshNode.MeshSubsets.Length; i++)
        {
            var matName = GetMaterialId(node, submeshNode, i);

            ColladaInstanceMaterialGeometry instanceMaterial = new();
            instanceMaterial.Target = $"#{matName}-material";
            instanceMaterial.Symbol = $"{matName}-material";
            instanceMaterials.Add(instanceMaterial);
        }

        return instanceMaterials.ToArray();
    }

    private ColladaNode CreateNode(ChunkNode nodeChunk, bool isControllerNode)
    {
        List<ColladaNode> childNodes = new();
        ColladaNode colladaNode = new();

        // Check to see if there is a second model file, and if the mesh chunk is actually there.
        if (_cryData.Models.Count > 1)
        {
            // Geometry file pair.  Get the Node and Mesh chunks from the geometry file, unless it's a Proxy node.
            string nodeName = nodeChunk.Name;
            int nodeID = nodeChunk.ID;

            // make sure there is a geometry node in the geometry file.  Or with Ivo files, just use the geometry file
            var modelIndex = nodeChunk._model.IsIvoFile ? 1 : 0;
            if (_cryData.Models[modelIndex].ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkType.Helper)
                colladaNode = CreateSimpleNode(nodeChunk, isControllerNode);
            else
            {
                ChunkNode geometryNode = _cryData.Models[1].NodeMap.Values.Where(a => a.Name == nodeChunk.Name).FirstOrDefault();
                ColladaGeometry geometryLibraryObject = DaeObject.Library_Geometries.Geometry.Where(a => a.Name == nodeChunk.Name).FirstOrDefault();
                if (geometryNode is null || geometryLibraryObject is null)
                    colladaNode = CreateSimpleNode(nodeChunk, isControllerNode);  // Can't find geometry for given node.
                else
                {
                    ChunkMesh geometryMesh = (ChunkMesh)_cryData.Models[1].ChunkMap[geometryNode.ObjectNodeID];
                    colladaNode = CreateGeometryNode(geometryNode, geometryMesh, isControllerNode);
                }
            }
        }
        else
        {
            if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkType.Mesh)
            {
                var meshChunk = (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                if (meshChunk.MeshSubsetsData == 0 || meshChunk.NumVertices == 0)  // Can have a node with a mesh and meshsubset, but no vertices.  Write as simple node.
                    colladaNode = CreateSimpleNode(nodeChunk, isControllerNode);
                else
                {
                    if (nodeChunk._model.ChunkMap[meshChunk.MeshSubsetsData].ID != 0)
                        colladaNode = CreateGeometryNode(nodeChunk, (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID], isControllerNode);
                    else
                        colladaNode = CreateSimpleNode(nodeChunk, isControllerNode);
                }
            }
            else
                colladaNode = CreateSimpleNode(nodeChunk, isControllerNode);
        }

        colladaNode.node = CreateChildNodes(nodeChunk, isControllerNode);
        return colladaNode;
    }

    /// <summary>This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics </summary>
    private ColladaNode CreateSimpleNode(ChunkNode nodeChunk, bool isControllerNode)
    {
        // This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics
        ColladaNodeType nodeType = ColladaNodeType.NODE;
        ColladaNode colladaNode = new()
        {
            Type = nodeType,
            Name = nodeChunk.Name,
            ID = nodeChunk.Name
        };

        ColladaMatrix matrix = new()
        {
            sID = "transform",
            Value_As_String = CreateStringFromMatrix4x4(nodeChunk.LocalTransform)
        };
        colladaNode.Matrix = new ColladaMatrix[1] { matrix };

        colladaNode.node = CreateChildNodes(nodeChunk, isControllerNode);
        return colladaNode;
    }

    /// <summary>Used by CreateNode and CreateSimpleNodes to create all the child nodes for the given node.</summary>
    private ColladaNode[]? CreateChildNodes(ChunkNode nodeChunk, bool isControllerNode)
    {
        List<ColladaNode> childNodes = new();
        foreach (ChunkNode childNodeChunk in nodeChunk.AllChildNodes)
        {
            if (_args.IsNodeNameExcluded(childNodeChunk.Name))
            {
                Log(LogLevelEnum.Debug, $"Excluding child node {childNodeChunk.Name}");
                continue;
            }

            ColladaNode childNode = CreateNode(childNodeChunk, isControllerNode); ;
            childNodes.Add(childNode);
        }
        return childNodes.ToArray();
    }

    private ColladaNode CreateJointNode(CompiledBone bone)
    {
        var boneName = bone.boneName.Replace(' ', '_');

        ColladaNode tmpNode = new()
        {
            ID = boneName,      // ID, name and sID must be the same or the controller can't seem to find the bone.
            Name = boneName,
            sID = boneName,
            Type = ColladaNodeType.JOINT
        };
        if (bone.ControllerID != -1)
            controllerIdToBoneName.Add(bone.ControllerID, bone.boneName);

        Matrix4x4 localMatrix = bone.LocalTransformMatrix.ConvertToTransformMatrix();

        // Rotate and translate code.
        //ColladaRotate rotate = new()
        //{
        //    sID = "rotate",
        //    Value_As_String = CreateStringFromVector4(localMatrix.ToAxisAngleDegrees())
        //};
        //ColladaTranslate translate = new()
        //{
        //    sID = "translate",
        //    Value_As_String = CreateStringFromVector3(localMatrix.GetTranslation())
        //};
        //tmpNode.Rotate = new ColladaRotate[1] { rotate };
        //tmpNode.Translate = new ColladaTranslate[1] { translate };

        // Matrix code
        ColladaMatrix matrix = new();
        List<ColladaMatrix> matrices = new();
        matrix.Value_As_String = CreateStringFromMatrix4x4(localMatrix);
        matrix.sID = "matrix";
        matrices.Add(matrix);
        tmpNode.Matrix = matrices.ToArray();

        // Recursively call this for each of the child bones to this bone.
        if (bone.numChildren > 0)
        {
            List<ColladaNode> childNodes = new();
            var allChildBones = _cryData.Bones.GetChildBones(bone);
            foreach (CompiledBone childBone in allChildBones)
            {
                childNodes.Add(CreateJointNode(childBone));
            }
            tmpNode.node = childNodes.ToArray();
        }
        return tmpNode;
    }

    private ColladaNode CreateGeometryNode(ChunkNode nodeChunk, ChunkMesh tmpMeshChunk, bool isControllerNode)
    {
        ColladaNode colladaNode = new();
        var meshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsetsData];
        var nodeType = ColladaNodeType.NODE;
        colladaNode.Type = nodeType;
        colladaNode.Name = nodeChunk.Name;
        colladaNode.ID = nodeChunk.Name;

        // Make the lists necessary for this Node.
        List<ColladaBindMaterial> bindMaterials = new();
        List<ColladaMatrix> matrices = new();
        ColladaMatrix matrix = new()
        {
            Value_As_String = CreateStringFromMatrix4x4(nodeChunk.LocalTransform),
            sID = "transform"
        };

        matrices.Add(matrix);          // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
        colladaNode.Matrix = matrices.ToArray();

        // Each node will have one instance geometry, although it could be a list.
        if (!isControllerNode)
        {
            List<ColladaInstanceGeometry> instanceGeometries = new();
            ColladaInstanceGeometry instanceGeometry = new()
            {
                Name = nodeChunk.Name,
                URL = "#" + nodeChunk.Name + "-mesh"  // this is the ID of the geometry.
            };
            ColladaBindMaterial bindMaterial = new()
            {
                Technique_Common = new ColladaTechniqueCommonBindMaterial
                {
                    Instance_Material = new ColladaInstanceMaterialGeometry[meshSubsets.NumMeshSubset]
                }
            };
            bindMaterials.Add(bindMaterial);
            instanceGeometry.Bind_Material = bindMaterials.ToArray();
            instanceGeometries.Add(instanceGeometry);

            colladaNode.Instance_Geometry = instanceGeometries.ToArray();
            colladaNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material = CreateInstanceMaterials(nodeChunk);
        }

        return colladaNode;
    }

    /// <summary>Retrieves the worldtobone (bind pose matrix) for the bone.</summary>
    private static string GetBindPoseArray(List<CompiledBone> compiledBones)
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
        ColladaScene scene = new();
        ColladaInstanceVisualScene visualScene = new()
        {
            URL = "#Scene",
            Name = "Scene"
        };
        scene.Visual_Scene = visualScene;
        DaeObject.Scene = scene;
    }

    /// <summary>Get the material name for a given submesh.</summary>
    private string? GetMaterialId(ChunkNode nodeChunk, ChunkMeshSubsets meshSubsets, int index)
    {
        // material id is <node.MaterialFileName>_mtl_<submatName>
        var materialFileName = nodeChunk.MaterialFileName;  // also the key in Materials
        if (string.IsNullOrWhiteSpace(materialFileName) && _cryData.Models.Count == 2)
        {
            var geometryNodeChunk = _cryData.Models[1].NodeMap.Values.Where(a => a.Name == nodeChunk.Name).FirstOrDefault();
            materialFileName = geometryNodeChunk?.MaterialFileName ?? string.Empty;
        }
        var matindex = meshSubsets.MeshSubsets[index].MatID;

        var materialName = _cryData.Materials[materialFileName]?.SubMaterials?[matindex].Name;
        return GetMaterialName(materialFileName, materialName ?? "unknown");
    }

    private string GetMaterialName(string matKey, string submatName)
    {
        // material name is <mtlChunkName>_mtl_<submatName>
        var matfileName = Path.GetFileNameWithoutExtension(matKey);

        return $"{matfileName}_mtl_{submatName}".Replace(' ', '_');
    }

    private static string CreateStringFromVector3(Vector3 vector)
    {
        StringBuilder vectorValues = new();
        vectorValues.AppendFormat("{0:F6} {1:F6} {2:F6}", vector.X, vector.Y, vector.Z);
        CleanNumbers(vectorValues);
        return vectorValues.ToString();
    }

    private static string CreateStringFromVector4(Vector4 vector)
    {
        StringBuilder vectorValues = new();
        vectorValues.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6}", vector.X, vector.Y, vector.Z, vector.W);
        CleanNumbers(vectorValues);
        return vectorValues.ToString();
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
}
