using CgfConverter.Collada;
using CgfConverter.CryEngineCore;
using CgfConverter.Models.Materials;
using CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Surfaces;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Lighting;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Effects;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Materials;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.COMMON;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Technique_Common;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Collada.Collada.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using static Extensions.FileHandlingExtensions;
using static DDSUnsplitter.Library.DDSUnsplitter;

namespace CgfConverter.Renderers.Collada;

/// <summary>
/// ColladaModelRenderer partial class - Material creation and texture handling
/// </summary>
public partial class ColladaModelRenderer
{
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
                    Log.D($"Combining texture file {textureFile}");
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

    private string GetMaterialName(string matKey, string submatName)
    {
        // material name is <mtlChunkName>_mtl_<submatName>
        var matfileName = Path.GetFileNameWithoutExtension(matKey);

        return $"{matfileName}_mtl_{submatName}".Replace(' ', '_');
    }

    /// <summary>
    /// Safely gets the submaterial name from a node's materials, with fallback for null or missing materials.
    /// </summary>
    private string GetSafeSubmaterialName(ChunkNode node, int matId)
    {
        if (node.Materials?.SubMaterials is null)
        {
            Log.W($"Node '{node.Name}' has no materials assigned, using default material name for MatID {matId}");
            return $"default_mat_{matId}";
        }

        if (matId < 0 || matId >= node.Materials.SubMaterials.Length)
        {
            Log.W($"Node '{node.Name}' has MatID {matId} out of bounds (SubMaterials count: {node.Materials.SubMaterials.Length}), using default material name");
            return $"default_mat_{matId}";
        }

        var submat = node.Materials.SubMaterials[matId];
        if (submat is null)
        {
            Log.W($"Node '{node.Name}' has null submaterial at MatID {matId}, using default material name");
            return $"default_mat_{matId}";
        }

        return submat.Name ?? $"unnamed_mat_{matId}";
    }
}
