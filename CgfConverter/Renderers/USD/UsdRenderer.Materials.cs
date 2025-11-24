using CgfConverter.Models.Materials;
using CgfConverter.Models.Shaders;
using CgfConverter.Renderers.USD.Attributes;
using CgfConverter.Renderers.USD.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        foreach (var matKey in _cryData.Materials.Keys) // Each mtl file is a Key
        {
            foreach (var submat in _cryData.Materials[matKey].SubMaterials)
            {
                var matName = GetMaterialName(matKey, submat.Name);
                var cleanMatName = CleanPathString(matName);
                var usdMat = new UsdMaterial(cleanMatName);
                usdMat.Attributes.Add(new UsdToken<string>(
                    "outputs:surface.connect",
                    $"</root/_materials/{cleanMatName}/Principled_BSDF.outputs:surface>"));
                usdMat.Children.AddRange(CreateShaders(submat, matKey, cleanMatName));
                matList.Add(usdMat);
            }
        }
        scope.Children.AddRange(matList);
        return scope;
    }

    private IEnumerable<UsdShader> CreateShaders(Material submat, string matKey, string matName)
    {
        List<UsdShader> shaders = new();

        // Look up shader definition and generate rules
        ShaderDefinition? shaderDef = null;
        List<MaterialRule> materialRules = new();

        if (!string.IsNullOrEmpty(submat.Shader))
        {
            _shaderDefinitions.TryGetValue(submat.Shader, out shaderDef);
            materialRules = _shaderRules.GenerateRules(submat, shaderDef);
        }

        // Check for Nodraw shader - these materials should be invisible
        bool isNodraw = !string.IsNullOrEmpty(submat.Shader) &&
                        submat.Shader.Equals("Nodraw", StringComparison.OrdinalIgnoreCase);

        // Add the PrincipleBSDF shader
        var principleBSDF = new UsdShader($"Principled_BSDF");
        principleBSDF.Attributes.Add(new UsdToken<string>("info:id", "UsdPreviewSurface", true));

        // Check if material rules will override these properties
        bool hasAlphaGlow = materialRules.Any(r => r.PropertyName == "%ALPHAGLOW");

        // Material color properties
        if (submat.DiffuseValue is not null)
        {
            var diffuse = $"{submat.DiffuseValue.Red}, {submat.DiffuseValue.Green}, {submat.DiffuseValue.Blue}";
            principleBSDF.Attributes.Add(new UsdColor3f("inputs:diffuseColor", diffuse));
        }

        if (submat.SpecularValue is not null)
        {
            var specular = $"{submat.SpecularValue.Red}, {submat.SpecularValue.Green}, {submat.SpecularValue.Blue}";
            principleBSDF.Attributes.Add(new UsdColor3f("inputs:specularColor", specular));
        }

        // Set emissive color from material (even with %ALPHAGLOW)
        // Note: %ALPHAGLOW tries to route diffuse alpha to emissive, but USD doesn't support this
        // (UsdPreviewSurface emissiveColor is color3f, can't connect float alpha to it)
        if (submat.EmissiveValue is not null)
        {
            var emissive = $"{submat.EmissiveValue.Red}, {submat.EmissiveValue.Green}, {submat.EmissiveValue.Blue}";
            principleBSDF.Attributes.Add(new UsdColor3f("inputs:emissiveColor", emissive));
        }

        // Opacity
        if (isNodraw)
        {
            // Nodraw materials should be fully transparent/invisible
            principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:opacity", 0.0f));
        }
        else if (submat.OpacityValue.HasValue)
        {
            principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:opacity", submat.OpacityValue.Value));
        }

        // Roughness - convert from Shininess
        // Shininess ranges from 0-255, where higher = more shiny (less rough)
        // Formula matches glTF renderer: roughness = (255 - shininess) / 255
        float roughness = Math.Clamp((255.0f - (float)submat.Shininess) / 255.0f, 0.0f, 1.0f);
        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:roughness", roughness));

        // Metallic - default to 0 (non-metallic)
        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:metallic", 0.0f));

        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:clearcoat", 0));
        principleBSDF.Attributes.Add(new UsdAttribute<float>("inputs:clearcoatRoughness", 0.03f));
        principleBSDF.Attributes.Add(new UsdToken<string?>("outputs:surface", null, false));
        shaders.Add(principleBSDF);

        if (submat.Textures == null)
            return shaders;

        // Track which textures we've already created to avoid duplicates
        var createdTextures = new HashSet<string>();

        foreach (var texture in submat.Textures)
        {
            // Skip environment maps (cubemaps cause crashes in Blender)
            if (texture.Map == Texture.MapTypeEnum.Env)
                continue;

            // Get the shader name we would create
            if (string.IsNullOrEmpty(texture.File))
                continue;

            var shaderName = CleanPathString(Path.GetFileNameWithoutExtension(texture.File));

            // Skip if we've already created a shader for this texture
            if (createdTextures.Contains(shaderName))
                continue;

            var imageTexture = CreateUsdImageTextureShader(texture, matName);
            if (imageTexture is not null)
            {
                createdTextures.Add(shaderName);
                shaders.Add(imageTexture);

                // Apply connections based on texture type and shader rules
                ApplyTextureConnections(imageTexture, texture, principleBSDF, matName, materialRules);
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
        List<MaterialRule> rules)
    {
        switch (texture.Map)
        {
            case Texture.MapTypeEnum.Diffuse:
                imageTexture.Attributes.Add(new UsdFloat3f("outputs:rgb"));
                imageTexture.Attributes.Add(new UsdFloat("outputs:a"));

                // Connect RGB to diffuse color
                principleBSDF.Attributes.Add(new UsdColor3f(
                    $"inputs:diffuseColor.connect",
                    $"</root/_materials/{matName}/{imageTexture.Name}.outputs:rgb>"));

                // Check shader rules for alpha channel routing
                var alphaTarget = _shaderRules.GetChannelTarget(rules, "Diffuse", "alpha");

                if (alphaTarget == "emissiveColor")
                {
                    // %ALPHAGLOW: Cannot directly connect float alpha to color3f emissiveColor in USD
                    // UsdPreviewSurface doesn't have a separate emissive intensity parameter
                    // The emissive color is already set to white above, user will see glow in diffuse alpha visually
                    Log.D($"  %ALPHAGLOW: Diffuse alpha controls glow (not directly supported in USD PreviewSurface)");
                    // Don't create a connection - it would be invalid USD
                }
                else if (!_shaderRules.ShouldOverrideDefaultAlphaConnection(rules))
                {
                    // Default behavior: connect alpha to opacity (unless overridden by rules)
                    principleBSDF.Attributes.Add(new UsdFloat(
                        $"inputs:opacity.connect",
                        $"</root/_materials/{matName}/{imageTexture.Name}.outputs:a>"));
                }
                break;

            case Texture.MapTypeEnum.Normals:
                imageTexture.Attributes.Add(new UsdFloat3f("outputs:rgb"));
                principleBSDF.Attributes.Add(new UsdFloat3f(
                    $"inputs:normal.connect",
                    $"</root/_materials/{matName}/{imageTexture.Name}.outputs:rgb>"));
                break;

            case Texture.MapTypeEnum.Specular:
                imageTexture.Attributes.Add(new UsdFloat3f("outputs:rgb"));
                imageTexture.Attributes.Add(new UsdFloat("outputs:a"));

                // Connect RGB to specular color
                principleBSDF.Attributes.Add(new UsdColor3f(
                    $"inputs:specularColor.connect",
                    $"</root/_materials/{matName}/{imageTexture.Name}.outputs:rgb>"));

                // Check for %SPECULARPOW_GLOSSALPHA rule (specular alpha → roughness)
                var specAlphaTarget = _shaderRules.GetChannelTarget(rules, "Specular", "alpha");
                if (specAlphaTarget == "roughness")
                {
                    Log.D($"  Applying %SPECULARPOW_GLOSSALPHA: specular alpha → roughness");
                    principleBSDF.Attributes.Add(new UsdFloat(
                        $"inputs:roughness.connect",
                        $"</root/_materials/{matName}/{imageTexture.Name}.outputs:a>"));
                }
                break;

            case Texture.MapTypeEnum.Opacity:
                // Opacity maps control transparency
                imageTexture.Attributes.Add(new UsdFloat("outputs:r"));
                principleBSDF.Attributes.Add(new UsdFloat(
                    $"inputs:opacity.connect",
                    $"</root/_materials/{matName}/{imageTexture.Name}.outputs:r>"));
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

    private string GetMaterialName(string matKey, string submatName)
    {
        // material name is <mtlChunkName>_mtl_<submatName>
        var matfileName = Path.GetFileNameWithoutExtension(submatName);

        return $"{matKey}_mtl_{matfileName}".Replace(' ', '_');
    }
}
