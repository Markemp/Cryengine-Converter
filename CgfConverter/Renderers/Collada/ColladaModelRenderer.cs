using CgfConverter.Collada;
using CgfConverter.CryEngineCore;
using CgfConverter.Models;
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
using CgfConverter.Utils;
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
using static CgfConverter.Utilities.HelperMethods;
using static DDSUnsplitter.Library.DDSUnsplitter;
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

    private static readonly Vector3 DefaultNormal = new(0.0f, 0.0f, 0.0f);
    private static readonly UV DefaultUV = new(0.0f, 0.0f);
    private static readonly IRGBA DefaultColor = new(1.0f, 1.0f, 1.0f, 1.0f);

    private readonly Dictionary<uint, string> controllerIdToBoneName = new();

    private readonly TaggedLogger Log;

    public ColladaModelRenderer(ArgsHandler argsHandler, CryEngine cryEngine)
    {
        _args = argsHandler;
        _cryData = cryEngine;
        Log = _cryData.Log;

        daeOutputFile = _args.FormatOutputFileName(".dae", _cryData.InputFile);
    }

    public int Render()
    {
        GenerateDaeObject();

        // At this point, we should have a cryData.Asset object, fully populated.
        Log.D();
        Log.D("*** Starting WriteCOLLADA() ***");
        Log.D();

        TextWriter writer = new StreamWriter(daeOutputFile.FullName);
        serializer.Serialize(writer, DaeObject);

        writer.Close();
        Log.D("End of Write Collada.  Export complete.");
        return 1;
    }

    public void GenerateDaeObject()
    {
        Log.D("Number of models: {0}", _cryData.Models.Count);
        for (int i = 0; i < _cryData.Models.Count; i++)
        {
            Log.D("\tNumber of nodes in model: {0}", _cryData.Models[i].NodeMap.Count);
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
        var extension = Path.GetExtension(_cryData.InputFile);
        if (extension == ".chr" || extension == ".skin")
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
            Input =
            [
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
            ]
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
                Param = [new ColladaParam { Name = "TIME", Type = "float" }]
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
                Param = [new ColladaParam { Name = "INTERPOLATION", Type = "name" }]
            }
        };

        return controllerAnimation;
    }

    public ColladaEffect? CreateColladaEffect(string matKey, Material subMat)
    {
        if (subMat is null) return null;
        //TODO: Change this so that it creates an effect for each Shader type.  Parameters will need to be passed in from the material.
        var effectName = GetMaterialName(matKey, subMat.Name ?? "unknown");

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
        if (subMat.Textures is not null && !_args.NoTextures)
        {
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

        if (subMat.Textures is not null && !_args.NoTextures)
        {
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
        WriteGeometries();
    }

    public void WriteGeometries()
    {
        ColladaLibraryGeometries libraryGeometries = new();

        // Make a list for all the geometries objects we will need. Will convert to array at end.  Define the array here as well
        // We have to define a Geometry for EACH meshsubset in the meshsubsets, since the mesh can contain multiple materials
        List<ColladaGeometry> geometryList = [];

        // For each of the nodes, we need to write the geometry.
        foreach (ChunkNode nodeChunk in _cryData.Nodes)
        {
            if (_args.IsNodeNameExcluded(nodeChunk.Name))
            {
                Log.D($"Excluding node {nodeChunk.Name}");
                continue;
            }

            if (nodeChunk.MeshData is not ChunkMesh meshChunk)
                continue;

            if (meshChunk.GeometryInfo is null)  // $physics node
                continue;

            // Create a geometry object.  Use the chunk ID for the geometry ID
            // Create all the materials used by this chunk.
            // Make the mesh object.  This will have 3 or 4 sources, 1 vertices, and 1 or more Triangles (with material ID)
            // If the Object ID of Node chunk points to a Helper or a Controller, place an empty.
            var subsets = meshChunk.GeometryInfo.GeometrySubsets;
            Datastream<uint>? indices = meshChunk.GeometryInfo.Indices;
            Datastream<UV>? uvs = meshChunk.GeometryInfo.UVs;
            Datastream<Vector3>? verts = meshChunk.GeometryInfo.Vertices;
            Datastream<VertUV>? vertsUvs = meshChunk.GeometryInfo.VertUVs;
            Datastream<Vector3>? normals = meshChunk.GeometryInfo.Normals;
            Datastream<IRGBA>? colors = meshChunk.GeometryInfo.Colors;

            if (verts is null && vertsUvs is null) // There is no vertex data for this node.  Skip.
                continue;

            // geometry is a Geometry object for each meshsubset.
            ColladaGeometry geometry = new()
            {
                Name = nodeChunk.Name,
                ID = nodeChunk.Name + "-mesh"
            };
            ColladaMesh colladaMesh = new();
            geometry.Mesh = colladaMesh;

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
            vertices.Input = inputshared;

            ColladaInputUnshared posInput = new() { Semantic = ColladaInputSemantic.POSITION };
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

            StringBuilder vertString = new();
            StringBuilder normString = new();
            StringBuilder uvString = new();
            StringBuilder colorString = new();

            var numberOfElements = nodeChunk.MeshData.GeometryInfo.GeometrySubsets.Sum(x => x.NumVertices);

            if (verts is not null)  // Will be null if it's using VertsUVs.
            {
                int numVerts = (int)verts.NumElements;

                floatArrayVerts.ID = posSource.ID + "-array";
                floatArrayVerts.Digits = 6;
                floatArrayVerts.Magnitude = 38;
                floatArrayVerts.Count = numVerts * 3;
                floatArrayUVs.ID = uvSource.ID + "-array";
                floatArrayUVs.Digits = 6;
                floatArrayUVs.Magnitude = 38;
                floatArrayUVs.Count = numVerts * 2;
                floatArrayNormals.ID = normSource.ID + "-array";
                floatArrayNormals.Digits = 6;
                floatArrayNormals.Magnitude = 38;
                floatArrayNormals.Count = numVerts * 3;
                floatArrayColors.ID = colorSource.ID + "-array";
                floatArrayColors.Digits = 6;
                floatArrayColors.Magnitude = 38;
                floatArrayColors.Count = numVerts * 4;

                var hasNormals = normals is not null;
                var hasUVs = uvs is not null;
                var hasColors = colors is not null;
                for (uint j = 0; j < numVerts; j++)
                {
                    var normal = hasNormals ? normals.Data[j] : DefaultNormal;
                    var uv = hasUVs ? uvs.Data[j] : DefaultUV;
                    var color = hasColors ? colors.Data[j] : DefaultColor;
                    vertString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} ", verts.Data[j].X, verts.Data[j].Y, verts.Data[j].Z);
                    normString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} ", Safe(normal.X), Safe(normal.Y), Safe(normal.Z));
                    colorString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} {3:F6} ", color.R, color.G, color.B, color.A);
                    uvString.AppendFormat(culture, "{0:F6} {1:F6} ", Safe(uv.U), 1 - Safe(uv.V));
                }
            }
            else    // VertsUV structure.  Pull out verts, colors and UVs from vertsUvs.
            {
                floatArrayVerts.ID = posSource.ID + "-array";
                floatArrayVerts.Digits = 6;
                floatArrayVerts.Magnitude = 38;
                floatArrayVerts.Count = numberOfElements * 3;
                floatArrayUVs.ID = uvSource.ID + "-array";
                floatArrayUVs.Digits = 6;
                floatArrayUVs.Magnitude = 38;
                floatArrayUVs.Count = numberOfElements * 2;
                floatArrayNormals.ID = normSource.ID + "-array";
                floatArrayNormals.Digits = 6;
                floatArrayNormals.Magnitude = 38;
                floatArrayNormals.Count = numberOfElements * 3;
                floatArrayColors.ID = colorSource.ID + "-array";
                floatArrayColors.Digits = 6;
                floatArrayColors.Magnitude = 38;
                floatArrayColors.Count = numberOfElements * 4;

                var multiplerVector = _cryData.IsIvoFile
                    ? Vector3.Abs((meshChunk.MinBound - meshChunk.MaxBound) / 2f)
                    : Vector3.One;

                if (multiplerVector.X < 1) multiplerVector.X = 1;
                if (multiplerVector.Y < 1) multiplerVector.Y = 1;
                if (multiplerVector.Z < 1) multiplerVector.Z = 1;
                Vector3 scalingVector = Vector3.One;

                if (meshChunk.ScalingVectors is not null)
                {
                    scalingVector = Vector3.Abs((meshChunk.ScalingVectors.Max - meshChunk.ScalingVectors.Min) / 2f);
                    if (scalingVector.X < 1) scalingVector.X = 1;
                    if (scalingVector.Y < 1) scalingVector.Y = 1;
                    if (scalingVector.Z < 1) scalingVector.Z = 1;
                }

                var boundaryBoxCenter = _cryData.IsIvoFile
                    ? (meshChunk.MinBound + meshChunk.MaxBound) / 2f
                    : Vector3.Zero;

                var scalingBoxCenter = meshChunk.ScalingVectors is not null ? (meshChunk.ScalingVectors.Max + meshChunk.ScalingVectors.Min) / 2f : Vector3.Zero;
                var hasNormals = normals is not null;
                var useScalingBox = _cryData.InputFile
                    .EndsWith("cga") || _cryData.InputFile.EndsWith("cgf")
                    && meshChunk.ScalingVectors is not null;

                // Create Vertices, UV, normals and colors string
                foreach (var subset in meshChunk.GeometryInfo.GeometrySubsets ?? [])
                {
                    for (int i = subset.FirstVertex; i < subset.NumVertices + subset.FirstVertex; i++)
                    {
                        Vector3 vert = vertsUvs.Data[i].Vertex;

                        if (!_cryData.InputFile.EndsWith("skin") && !_cryData.InputFile.EndsWith("chr"))
                        {
                            if (meshChunk.ScalingVectors is null)
                                vert = (vert * multiplerVector) + boundaryBoxCenter;
                            else
                                vert = (vert * scalingVector) + scalingBoxCenter;
                        }

                        vertString.AppendFormat("{0:F6} {1:F6} {2:F6} ", Safe(vert.X), Safe(vert.Y), Safe(vert.Z));
                        colorString.AppendFormat(culture, "{0:F6} {1:F6} {2:F6} {3:F6} ", vertsUvs.Data[i].Color.R, vertsUvs.Data[i].Color.G, vertsUvs.Data[i].Color.B, vertsUvs.Data[i].Color.A);
                        uvString.AppendFormat("{0:F6} {1:F6} ", Safe(vertsUvs.Data[i].UV.U), Safe(1 - vertsUvs.Data[i].UV.V));

                        var normal = hasNormals ? normals.Data[i] : DefaultNormal;
                        normString.AppendFormat("{0:F6} {1:F6} {2:F6} ", Safe(normal.X), Safe(normal.Y), Safe(normal.Z));
                    }
                }
            }

            CleanNumbers(vertString);
            CleanNumbers(normString);
            CleanNumbers(uvString);
            CleanNumbers(colorString);

            #region Create the triangles node.
            var numberOfMeshSubsets = subsets.Count;
            var triangles = new ColladaTriangles[numberOfMeshSubsets];
            geometry.Mesh.Triangles = triangles;

            for (int j = 0; j < numberOfMeshSubsets; j++) // Need to make a new Triangles entry for each submesh.
            {
                triangles[j] = new ColladaTriangles
                {
                    Count = subsets[j].NumIndices / 3,
                    Material = GetMaterialName(nodeChunk.MaterialFileName, nodeChunk.Materials.SubMaterials[subsets[j].MatID].Name) + "-material"
                };

                // Create the inputs.  vertex, normal, texcoord, color
                int inputCount = 3;
                if (colors is not null || vertsUvs is not null)
                    inputCount++;

                triangles[j].Input = new ColladaInputShared[inputCount];

                triangles[j].Input[0] = new ColladaInputShared
                {
                    Semantic = ColladaInputSemantic.VERTEX,
                    Offset = 0,
                    source = "#" + vertices.ID
                };
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
                if (colors is not null || vertsUvs is not null)
                {
                    triangles[j].Input[nextInputID] = new ColladaInputShared
                    {
                        Semantic = ColladaInputSemantic.COLOR,
                        Offset = nextInputID,
                        source = "#" + colorSource.ID
                    };
                    nextInputID++;
                }

                // Create the P node for the Triangles.
                StringBuilder p = new();
                string formatString;
                if (colors is not null || vertsUvs is not null)
                    formatString = "{0} {0} {0} {0} {1} {1} {1} {1} {2} {2} {2} {2} ";
                else
                    formatString = "{0} {0} {0} {1} {1} {1} {2} {2} {2} ";

                var offsetStart = 0;
                for (int q = 0; q < meshChunk.GeometryInfo.GeometrySubsets.IndexOf(subsets[j]); q++)
                {
                    offsetStart += meshChunk.GeometryInfo.GeometrySubsets[q].NumVertices;
                }

                for (var k = subsets[j].FirstIndex; k < (subsets[j].FirstIndex + subsets[j].NumIndices); k += 3)
                {
                    var firstGlobalIndex = indices.Data[subsets[j].FirstIndex];
                    uint localIndex0 = (uint)((indices.Data[k] - firstGlobalIndex) + offsetStart);
                    uint localIndex1 = (uint)((indices.Data[k + 1] - firstGlobalIndex) + offsetStart);
                    uint localIndex2 = (uint)((indices.Data[k + 2] - firstGlobalIndex) + offsetStart);

                    p.AppendFormat(formatString, localIndex0, localIndex1, localIndex2);
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
            posSource.Technique_Common.Accessor.Count = (uint)numberOfElements;
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
                    Count = (uint)numberOfElements
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

            uvSource.Technique_Common.Accessor.Count = (uint)numberOfElements;

            ColladaParam[] paramUV = new ColladaParam[2];
            paramUV[0] = new ColladaParam();
            paramUV[1] = new ColladaParam();
            paramUV[0].Name = "S";
            paramUV[0].Type = "float";
            paramUV[1].Name = "T";
            paramUV[1].Type = "float";
            uvSource.Technique_Common.Accessor.Param = paramUV;

            if (colors is not null || vertsUvs is not null)
            {
                colorSource.Technique_Common = new ColladaTechniqueCommonSource
                {
                    Accessor = new ColladaAccessor()
                };
                colorSource.Technique_Common.Accessor.Source = "#" + floatArrayColors.ID;
                colorSource.Technique_Common.Accessor.Stride = 4;
                colorSource.Technique_Common.Accessor.Count = (uint)numberOfElements;
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

            // There is no geometry for a helper or controller node.  Can skip the rest.
            // Sanity checks
            var vertcheck = vertString.ToString().TrimEnd().Split(' ');
            var normcheck = normString.ToString().TrimEnd().Split(' ');
            var colorcheck = colorString.ToString().TrimEnd().Split(' ');
            var uvcheck = uvString.ToString().TrimEnd().Split(' ');

        }
        libraryGeometries.Geometry = geometryList.ToArray();
        DaeObject.Library_Geometries = libraryGeometries;
    }

    private void CreateMaterials()
    {
        List<ColladaMaterial> colladaMaterials = [];
        List<ColladaEffect> colladaEffects = [];
        if (DaeObject.Library_Materials?.Material is null)
        {
            DaeObject.Library_Materials = new();
            DaeObject.Library_Materials.Material = [];
        }

        foreach (var matKey in _cryData.Materials.Keys)
        {
            foreach (var subMat in _cryData.Materials[matKey].SubMaterials ?? [])
            {
                // Check to see if the collada object already has a material with this name.  If not, add.
                var matNames = DaeObject.Library_Materials.Material.Select(c => c.Name);
                if (!matNames.Contains(subMat.Name))
                {
                    colladaMaterials.Add(AddMaterialToMaterialLibrary(matKey, subMat));
                    var effect = CreateColladaEffect(matKey, subMat);
                    if (effect is not null)
                        colladaEffects.Add(effect);
                }
                // If there are matlayers, add these too
                foreach (var matLayer in subMat.MatLayers?.Layers ?? [])
                {
                    int index = Array.IndexOf(subMat.MatLayers?.Layers ?? [], matLayer);
                    colladaMaterials.Add(AddMaterialToMaterialLibrary(matLayer.Path ?? "unknown mat layer", subMat.SubMaterials[index]));
                    var effect = CreateColladaEffect(matKey, subMat);
                    if (effect is not null)
                        colladaEffects.Add(CreateColladaEffect(matLayer.Path ?? "unknown mat layer", subMat.SubMaterials[index]));
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

        if (!_args.NoTextures)
            AddTexturesToTextureLibrary(matKey, submat);
        return material;
    }

    private void AddTexturesToTextureLibrary(string matKey, Material submat)
    {
        List<ColladaImage> imageList = [];
        int numberOfTextures = submat?.Textures?.Length ?? 0;

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
            var textureFile = ResolveTextureFile(submat.Textures[i].File, _args.PackFileSystem, [_cryData.ObjectDir]);

            if (_args.PngTextures && File.Exists(Path.ChangeExtension(textureFile, ".png")))
                textureFile = Path.ChangeExtension(textureFile, ".png");
            else if (_args.TgaTextures && File.Exists(Path.ChangeExtension(textureFile, ".tga")))
                textureFile = Path.ChangeExtension(textureFile, ".tga");
            else if (_args.TiffTextures && File.Exists(Path.ChangeExtension(textureFile, ".tif")))
                textureFile = Path.ChangeExtension(textureFile, ".tif");
            try
            {
                if (_args.UnsplitTextures)
                {
                    Log.I($"Combining texture file {textureFile}");
                    Combine(textureFile);
                }
            }
            catch (Exception ex)
            {
                Log.W($"Error combining texture {textureFile}: {ex.Message}");
            }

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
                boneNames.Append(_cryData.SkinningInfo.CompiledBones[i].BoneName.Replace(' ', '_') + " ");
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
            var skinningInfo = _cryData.SkinningInfo;
            var nodeChunk = _cryData.RootNode;

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

            var numberOfWeights = skinningInfo.IntVertices is null ? skinningInfo.BoneMappings.Count : skinningInfo.Ext2IntMap.Count;
            var boneInfluenceCount =
                nodeChunk.MeshData?.GeometryInfo?.BoneMappings?.Data[0].BoneInfluenceCount == 8
                    ? 8
                    : 4;

            var boneMappingData = skinningInfo.IntVertices is null
                ? nodeChunk.MeshData?.GeometryInfo?.BoneMappings.Data.ToList()
                : skinningInfo.Ext2IntMap
                    .Select(x => skinningInfo.IntVertices[x])
                    .Select(x => x.BoneMapping)
                    .ToList();

            if (boneMappingData is null) return;

            StringBuilder weights = new();
            weightArraySource.Float_Array.Count = numberOfWeights;
            for (int i = 0; i < numberOfWeights; i++)
            {
                for (int j = 0; j < boneInfluenceCount; j++)
                {
                    weights.Append(boneMappingData[i].Weight[j].ToString() + " ");
                }
            }
            ;
            accessor.Count = (uint)(numberOfWeights * boneInfluenceCount);

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
            vertexWeights.Count = (int)numberOfWeights;
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
            for (int i = 0; i < numberOfWeights; i++)
            {
                vCount.Append($"{boneInfluenceCount} ");
            }
            ;
            vertexWeights.VCount = new ColladaIntArrayString
            {
                Value_As_String = vCount.ToString().TrimEnd()
            };
            StringBuilder vertices = new();
            int index = 0;

            for (int i = 0; i < numberOfWeights; i++)
            {
                vertices.Append(boneMappingData[i].BoneIndex[0] + " " + index + " ");
                vertices.Append(boneMappingData[i].BoneIndex[1] + " " + (index + 1) + " ");
                vertices.Append(boneMappingData[i].BoneIndex[2] + " " + (index + 2) + " ");
                vertices.Append(boneMappingData[i].BoneIndex[3] + " " + (index + 3) + " ");
                if (boneInfluenceCount == 8)
                {
                    vertices.Append(boneMappingData[i].BoneIndex[4] + " " + (index + 4) + " ");
                    vertices.Append(boneMappingData[i].BoneIndex[5] + " " + (index + 5) + " ");
                    vertices.Append(boneMappingData[i].BoneIndex[6] + " " + (index + 6) + " ");
                    vertices.Append(boneMappingData[i].BoneIndex[7] + " " + (index + 7) + " ");
                    index += 4;
                }
                index += 4;
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

        List<ColladaVisualScene> visualScenes = [];
        ColladaVisualScene visualScene = new();
        List<ColladaNode> nodes = [];

        // THERE CAN BE MULTIPLE ROOT NODES IN EACH FILE!  Check to see if the parentnodeid ~0 and be sure to add a node for it.
        List<ColladaNode> positionNodes = [];
        List<ChunkNode> positionRoots = _cryData.Nodes.Where(a => a.ParentNodeID == ~0).ToList();
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

        List<ColladaVisualScene> visualScenes = [];
        ColladaVisualScene visualScene = new();
        List<ColladaNode> nodes = [];

        List<ChunkNode> positionRoots = _cryData.Nodes.Where(a => a.ParentNodeID == ~0).ToList();

        // Check to see if there is a CompiledBones chunk.  If so, add a Node.
        if (_cryData.Chunks.Any(a => a.ChunkType == ChunkType.CompiledBones ||
            a.ChunkType == ChunkType.CompiledBonesSC ||
            a.ChunkType == ChunkType.CompiledBones_Ivo2))
        {
            ColladaNode boneNode = CreateJointNode(_cryData.SkinningInfo.RootBone);
            nodes.Add(boneNode);
        }

        var hasGeometry = _cryData.Nodes.Any(x => x.MeshData is not null);

        if (hasGeometry)
        {
            foreach (var node in positionRoots)
            {
                var colladaNode = CreateNode(node, true);
                colladaNode.Instance_Controller = new ColladaInstanceController[1];
                colladaNode.Instance_Controller[0] = new ColladaInstanceController
                {
                    URL = "#Controller",
                    Skeleton = new ColladaSkeleton[1]
                };

                var skeleton = colladaNode.Instance_Controller[0].Skeleton[0] = new ColladaSkeleton();
                skeleton.Value = $"#{_cryData.SkinningInfo.CompiledBones[0].BoneName}".Replace(' ', '_');
                colladaNode.Instance_Controller[0].Bind_Material = new ColladaBindMaterial[1];
                ColladaBindMaterial bindMaterial = colladaNode.Instance_Controller[0].Bind_Material[0] = new ColladaBindMaterial();

                // Create an Instance_Material for each material
                bindMaterial.Technique_Common = new ColladaTechniqueCommonBindMaterial();
                colladaNode.Instance_Controller[0].Bind_Material[0].Technique_Common.Instance_Material = CreateInstanceMaterials(node);

                foreach (ChunkNode child in node.Children)
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
        List<ColladaInstanceMaterialGeometry> instanceMaterials = [];

        var matIndices = node.MeshData?.GeometryInfo?.GeometrySubsets?.Select(x => x.MatID) ?? [];

        foreach (var index in matIndices)
        {
            var matName = GetMaterialName(node.MaterialFileName, node.Materials.SubMaterials[index].Name);
            ColladaInstanceMaterialGeometry instanceMaterial = new();
            instanceMaterial.Target = $"#{matName}-material";
            instanceMaterial.Symbol = $"{matName}-material";
            instanceMaterials.Add(instanceMaterial);
        }

        return instanceMaterials.ToArray();
    }

    private ColladaNode CreateNode(ChunkNode nodeChunk, bool isControllerNode)
    {
        ColladaNode colladaNode = new();

        string nodeName = nodeChunk.Name;
        int nodeID = nodeChunk.ID;

        if (nodeChunk.ChunkHelper is not null || nodeChunk.MeshData?.GeometryInfo is null)
            colladaNode = CreateSimpleNode(nodeChunk, isControllerNode);
        else
        {
            ColladaGeometry geometryLibraryObject = DaeObject.Library_Geometries.Geometry.Where(a => a.Name == nodeChunk.Name).FirstOrDefault();
            ChunkMesh geometryMesh = nodeChunk.MeshData;
            colladaNode = CreateGeometryNode(nodeChunk, geometryMesh, isControllerNode);
        }

        colladaNode.node = CreateChildNodes(nodeChunk, isControllerNode);
        return colladaNode;
    }

    /// <summary>This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics </summary>
    private ColladaNode CreateSimpleNode(ChunkNode nodeChunk, bool isControllerNode)
    {
        // This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics
        ColladaNode colladaNode = new()
        {
            Type = ColladaNodeType.NODE,
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
        List<ColladaNode> childNodes = [];
        foreach (ChunkNode childNodeChunk in nodeChunk.Children)
        {
            if (_args.IsNodeNameExcluded(childNodeChunk.Name))
            {
                Log.D($"Excluding child node {childNodeChunk.Name}");
                continue;
            }

            ColladaNode childNode = CreateNode(childNodeChunk, isControllerNode);
            childNodes.Add(childNode);
        }
        return childNodes.ToArray();
    }

    private ColladaNode CreateJointNode(CompiledBone bone)
    {
        var boneName = bone.BoneName.Replace(' ', '_');

        ColladaNode tmpNode = new()
        {
            ID = boneName,      // ID, name and sID must be the same or the controller can't seem to find the bone.
            Name = boneName,
            sID = boneName,
            Type = ColladaNodeType.JOINT
        };
        if (bone.ControllerID != -1 && bone.ControllerID != uint.MaxValue)
            controllerIdToBoneName.Add(bone.ControllerID, bone.BoneName);

        Matrix4x4 localMatrix = bone.LocalTransformMatrix.ConvertToTransformMatrix();

        ColladaMatrix matrix = new();
        List<ColladaMatrix> matrices = [];
        matrix.Value_As_String = CreateStringFromMatrix4x4(localMatrix);
        matrix.sID = "matrix";
        matrices.Add(matrix);
        tmpNode.Matrix = matrices.ToArray();

        // Recursively call this for each of the child bones to this bone.
        if (bone.NumberOfChildren > 0)
        {
            List<ColladaNode> childNodes = [];
            var allChildBones = _cryData.SkinningInfo?.GetChildBones(bone) ?? [];
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
        var meshSubsets = nodeChunk.MeshData.GeometryInfo.GeometrySubsets;
        var nodeType = ColladaNodeType.NODE;
        colladaNode.Type = nodeType;
        colladaNode.Name = nodeChunk.Name;
        colladaNode.ID = nodeChunk.Name;

        // Make the lists necessary for this Node.
        List<ColladaBindMaterial> bindMaterials = [];
        List<ColladaMatrix> matrices = [];
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
            List<ColladaInstanceGeometry> instanceGeometries = [];
            ColladaInstanceGeometry instanceGeometry = new()
            {
                Name = nodeChunk.Name,
                URL = "#" + nodeChunk.Name + "-mesh"  // this is the ID of the geometry.
            };
            ColladaBindMaterial bindMaterial = new()
            {
                Technique_Common = new ColladaTechniqueCommonBindMaterial
                {
                    Instance_Material = new ColladaInstanceMaterialGeometry[meshSubsets.Count]
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
