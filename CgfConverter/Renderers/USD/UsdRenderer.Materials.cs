using CgfConverter.Models.Materials;
using CgfConverter.Models.Shaders;
using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static DDSUnsplitter.Library.DDSUnsplitter;
using static Extensions.FileHandlingExtensions;

namespace CgfConverter.Renderers.USD;

/// <summary>
/// UsdRenderer partial class - Material and shader creation
/// </summary>
public partial class UsdRenderer
{
    private UsdPrim CreateMaterials()
    {
        var scope = new UsdScope("_materials");
        var matList = new List<UsdMaterial>();
        var createdMaterialNames = new HashSet<string>();  // Track created materials to avoid duplicates

        foreach (var matKey in _cryData.Materials.Keys) // Each mtl file is a Key
        {
            var material = _cryData.Materials[matKey];
            if (material?.SubMaterials is null)
            {
                Log.D($"Skipping material '{matKey}' - no submaterials");
                continue;
            }

            foreach (var submat in material.SubMaterials)
            {
                var matName = GetMaterialName(matKey, submat.Name);
                var cleanMatName = CleanPathString(matName);

                // Skip if we've already created a material with this name
                if (createdMaterialNames.Contains(cleanMatName))
                {
                    Log.D($"Skipping duplicate material: {cleanMatName}");
                    continue;
                }
                createdMaterialNames.Add(cleanMatName);

                var usdMat = new UsdMaterial(cleanMatName);
                usdMat.Attributes.Add(new UsdToken<string>(
                    "outputs:surface.connect",
                    $"</{_rootPrimName}/_materials/{cleanMatName}/Principled_BSDF.outputs:surface>"));
                usdMat.Children.AddRange(CreateShaders(submat, matKey, cleanMatName));
                matList.Add(usdMat);
            }
        }
        scope.Children.AddRange(matList);
        return scope;
    }

    private IEnumerable<UsdShader> CreateShaders(Material submat, string matKey, string matName)
    {
        List<UsdShader> shaders = [];

        // When a material uses MatLayers, flatten to the Primary (first) layer.
        // The parent material's properties (Specular=1,1,1 etc.) are layer blending multipliers,
        // not real PBR values. The Primary layer has the actual surface properties and textures.
        Material effectiveMat = submat;
        Texture[]? textures = submat.Textures;

        if ((textures is null || textures.Length == 0)
            && submat.SubMaterials is { Length: > 0 }
            && submat.MatLayers is not null)
        {
            var primaryLayer = submat.SubMaterials[0];
            if (primaryLayer is not null)
            {
                effectiveMat = primaryLayer;
                textures = primaryLayer.Textures;
                Log.D($"Flattening to Primary layer for {submat.Name} (from {submat.MatLayers.Layers?[0]?.Path})");
            }
        }

        textures ??= effectiveMat.Textures;

        // Look up shader definition and generate rules
        ShaderDefinition? shaderDef = null;
        List<MaterialRule> materialRules = [];

        if (!string.IsNullOrEmpty(effectiveMat.Shader))
        {
            _shaderDefinitions.TryGetValue(effectiveMat.Shader, out shaderDef);
            materialRules = _shaderRules.GenerateRules(effectiveMat, shaderDef);
        }

        // Check for Nodraw shader - these materials should be invisible
        bool isNodraw = !string.IsNullOrEmpty(effectiveMat.Shader) &&
                        effectiveMat.Shader.Equals("Nodraw", StringComparison.OrdinalIgnoreCase);

        // Add the PrincipleBSDF shader
        var principleBSDF = new UsdShader($"Principled_BSDF");
        principleBSDF.Attributes.Add(new UsdToken<string>("info:id", "UsdPreviewSurface", true));

        // Check if material rules will override these properties
        bool hasAlphaGlow = materialRules.Any(r => r.PropertyName == "%ALPHAGLOW");

        // Material color properties
        if (effectiveMat.DiffuseValue is not null)
        {
            var diffuse = $"{effectiveMat.DiffuseValue.Red}, {effectiveMat.DiffuseValue.Green}, {effectiveMat.DiffuseValue.Blue}";
            principleBSDF.Attributes.Add(new UsdColor3f("inputs:diffuseColor", diffuse));
        }

        if (effectiveMat.SpecularValue is not null)
        {
            var specular = $"{effectiveMat.SpecularValue.Red}, {effectiveMat.SpecularValue.Green}, {effectiveMat.SpecularValue.Blue}";
            principleBSDF.Attributes.Add(new UsdColor3f("inputs:specularColor", specular));
        }

        // Set emissive color from material (even with %ALPHAGLOW)
        // Note: %ALPHAGLOW tries to route diffuse alpha to emissive, but USD doesn't support this
        // (UsdPreviewSurface emissiveColor is color3f, can't connect float alpha to it)
        if (effectiveMat.EmissiveValue is not null)
        {
            var emissive = $"{effectiveMat.EmissiveValue.Red}, {effectiveMat.EmissiveValue.Green}, {effectiveMat.EmissiveValue.Blue}";
            principleBSDF.Attributes.Add(new UsdColor3f("inputs:emissiveColor", emissive));
        }

        // Opacity.  Nodraw materials should be fully transparent/invisible
        if (isNodraw)
            principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:opacity", 0.0f));
        else if (effectiveMat.OpacityValue.HasValue)
            principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:opacity", effectiveMat.OpacityValue.Value));

        // Roughness - convert from Shininess weighted by specular intensity.
        // CryEngine Shininess is a Phong exponent (0-255) and Specular color controls reflection intensity.
        // In PBR, low-specular surfaces should appear matte regardless of shininess.
        // Formula: roughness = 1 - glossiness * sqrt(specularIntensity)
        // sqrt() expands CryEngine's low dielectric specular range (0.02-0.05) into a useful range.
        float glossiness = (float)effectiveMat.Shininess / 255.0f;
        float specIntensity = effectiveMat.SpecularValue is not null
            ? Math.Max(effectiveMat.SpecularValue.Red, Math.Max(effectiveMat.SpecularValue.Green, effectiveMat.SpecularValue.Blue))
            : 0.0f;
        float roughness = Math.Clamp(1.0f - glossiness * MathF.Sqrt(specIntensity), 0.0f, 1.0f);
        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:roughness", roughness));

        // Metallic - default to 0 (non-metallic)
        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:metallic", 0.0f));

        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:clearcoat", 0));
        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:clearcoatRoughness", 0.03f));
        principleBSDF.Attributes.Add(new UsdToken<string?>("outputs:surface", null, false));
        shaders.Add(principleBSDF);

        if (textures is null || textures.Length == 0)
            return shaders;

        // Track which textures we've already created to avoid duplicates
        var createdTextures = new HashSet<string>();

        foreach (var texture in textures)
        {
            // Skip environment maps (cubemaps cause crashes in Blender)
            if (texture.Map == Texture.MapTypeEnum.Env) continue;

            // Get the shader name we would create
            if (string.IsNullOrEmpty(texture.File)) continue;

            var shaderName = CleanPathString(Path.GetFileNameWithoutExtension(texture.File));

            // Skip if we've already created a shader for this texture
            if (createdTextures.Contains(shaderName)) continue;

            var imageTexture = CreateUsdImageTextureShader(texture, matName);
            if (imageTexture is not null)
            {
                createdTextures.Add(shaderName);
                shaders.Add(imageTexture);

                // Apply connections based on texture type and shader rules
                ApplyTextureConnections(imageTexture, texture, principleBSDF, matName, materialRules, effectiveMat);
            }
        }

        return shaders;
    }

    /// <summary>
    /// Apply texture connections based on texture type and active shader rules.
    /// </summary>
    private void ApplyTextureConnections(
        UsdShader imageTexture,
        Texture texture,
        UsdShader principleBSDF,
        string matName,
        List<MaterialRule> rules,
        Material submat)
    {
        switch (texture.Map)
        {
            case Texture.MapTypeEnum.Diffuse:
            case Texture.MapTypeEnum.TexSlot1:
            case Texture.MapTypeEnum.TexSlot9:
                imageTexture.Attributes.Add(new UsdFloat3f("outputs:rgb"));
                imageTexture.Attributes.Add(new UsdFloat("outputs:a"));

                // Connect RGB to diffuse color
                principleBSDF.Attributes.Add(new UsdColor3f(
                    $"inputs:diffuseColor.connect",
                    $"</{_rootPrimName}/_materials/{matName}/{imageTexture.Name}.outputs:rgb>"));

                // Check shader rules for alpha channel routing
                var alphaTarget = _shaderRules.GetChannelTarget(rules, "Diffuse", "alpha");

                if (alphaTarget == "emissiveColor")
                {
                    // %ALPHAGLOW: Cannot directly connect float alpha to color3f emissiveColor in USD
                    // UsdPreviewSurface doesn't have a separate emissive intensity parameter
                    Log.D($"  %ALPHAGLOW: Diffuse alpha controls glow (not directly supported in USD PreviewSurface)");
                }
                else if (!_shaderRules.ShouldOverrideDefaultAlphaConnection(rules))
                {
                    // Only connect diffuse alpha to opacity when the material actually needs transparency:
                    // - AlphaTest is set (cutout transparency)
                    // - Opacity < 1 (blended transparency)
                    // Otherwise the alpha channel is unused and connecting it causes false transparency.
                    bool needsAlpha = submat.AlphaTest > 0 || (submat.OpacityValue.HasValue && submat.OpacityValue.Value < 1.0f);
                    if (needsAlpha)
                    {
                        principleBSDF.Attributes.Add(new UsdFloat(
                            $"inputs:opacity.connect",
                            $"</{_rootPrimName}/_materials/{matName}/{imageTexture.Name}.outputs:a>"));
                    }
                }
                break;

            case Texture.MapTypeEnum.Normals:
            case Texture.MapTypeEnum.TexSlot2:
                imageTexture.Attributes.Add(new UsdFloat3f("outputs:rgb"));
                principleBSDF.Attributes.Add(new UsdFloat3f(
                    $"inputs:normal.connect",
                    $"</{_rootPrimName}/_materials/{matName}/{imageTexture.Name}.outputs:rgb>"));
                break;

            case Texture.MapTypeEnum.Specular:
            case Texture.MapTypeEnum.TexSlot4:
            case Texture.MapTypeEnum.TexSlot6:
            case Texture.MapTypeEnum.TexSlot10:
                imageTexture.Attributes.Add(new UsdFloat3f("outputs:rgb"));
                imageTexture.Attributes.Add(new UsdFloat("outputs:a"));

                // Connect RGB to specular color
                principleBSDF.Attributes.Add(new UsdColor3f(
                    $"inputs:specularColor.connect",
                    $"</{_rootPrimName}/_materials/{matName}/{imageTexture.Name}.outputs:rgb>"));

                // Specular alpha carries glossiness/smoothness data in CryEngine.
                // Older shaders use %SPECULARPOW_GLOSSALPHA flag; newer shaders (Layer, HardSurface)
                // treat this as default behavior. Always connect spec alpha → roughness.
                principleBSDF.Attributes.Add(new UsdFloat(
                    $"inputs:roughness.connect",
                    $"</{_rootPrimName}/_materials/{matName}/{imageTexture.Name}.outputs:a>"));
                break;

            case Texture.MapTypeEnum.Opacity:
                // In Cryengine, Opacity maps are primarily for translucency/backlighting effects on vegetation
                // USD PreviewSurface doesn't support this - only has a single opacity input
                // If material has AlphaTest set, the diffuse alpha is used for cutout transparency
                // For now, only use Opacity map if there's no diffuse texture (fallback)
                var hasDiffuse = submat.Textures.Any(t => t.Map == Texture.MapTypeEnum.Diffuse);
                if (!hasDiffuse)
                {
                    // No diffuse texture, use opacity map as transparency mask
                    imageTexture.Attributes.Add(new UsdFloat("outputs:r"));
                    principleBSDF.Attributes.Add(new UsdFloat(
                        $"inputs:opacity.connect",
                        $"</{_rootPrimName}/_materials/{matName}/{imageTexture.Name}.outputs:r>"));
                }
                else
                {
                    // Diffuse texture exists - its alpha channel handles transparency
                    // Opacity map is for translucency which USD PreviewSurface doesn't support
                    Log.D($"Skipping Opacity map for {matName} - USD PreviewSurface doesn't support translucency, using diffuse alpha instead");
                }
                break;

            // Additional texture types can be added here
            default:
                Log.D($"Unhandled texture map type: {texture.Map}");
                break;
        }
    }

    private UsdShader? CreateUsdImageTextureShader(Texture texture, string matName)
    {
        if (string.IsNullOrEmpty(texture.File))
        {
            Log.D("Texture has no file path specified");
            return null;
        }

        // Filter out null/empty data directories
        var dataDirs = new List<string>();
        if (!string.IsNullOrEmpty(_args.DataDir))
            dataDirs.Add(_args.DataDir);

        var textureFile = ResolveTextureFile(texture.File, _args.PackFileSystem, dataDirs);

        if (_args.UnsplitTextures)
        {
            try
            {
                var fileInfo = new FileInfo(textureFile);
                long sizeBefore = fileInfo.Exists ? fileInfo.Length : -1;
                Log.D($"Combining texture file {textureFile} (exists={fileInfo.Exists}, size={sizeBefore})");

                string result = Combine(textureFile);

                fileInfo.Refresh();
                long sizeAfter = fileInfo.Exists ? fileInfo.Length : -1;
                Log.D($"Combine returned: {result} (size before={sizeBefore}, after={sizeAfter}, changed={sizeBefore != sizeAfter})");
            }
            catch (Exception ex)
            {
                Log.W($"Error combining texture {textureFile}: {ex.Message}");
            }
        }

        if (File.Exists(textureFile) == false)
        {
            Log.D("Texture file not found: {0}", texture.File);
            return null;
        }
        var usdImageTexture = new UsdShader(CleanPathString(Path.GetFileNameWithoutExtension(texture.File)));

        usdImageTexture.Attributes.Add(new UsdToken<string>("info:id", "UsdUVTexture", true));
        usdImageTexture.Attributes.Add(new UsdToken<string>("inputs:wrapS", "repeat"));
        usdImageTexture.Attributes.Add(new UsdToken<string>("inputs:wrapT", "repeat"));

        // Use forward slashes for USD asset paths (cross-platform compatible)
        var normalizedPath = textureFile.Replace('\\', '/');
        usdImageTexture.Attributes.Add(new UsdAsset("file", normalizedPath));

        var isBumpmap = texture.Map == Texture.MapTypeEnum.Normals ? "raw" : "sRGB";
        usdImageTexture.Attributes.Add(new UsdToken<string>("inputs:sourceColorSpace", isBumpmap));

        return usdImageTexture;
    }

    private static string GetMaterialName(string matKey, string submatName)
    {
        // material name is <mtlChunkName>_mtl_<submatName>
        var matfileName = Path.GetFileNameWithoutExtension(submatName);

        return $"{matKey}_mtl_{matfileName}".Replace(' ', '_');
    }
}
