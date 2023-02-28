using System;
using System.IO;
using System.Numerics;
using BCnEncoder.Decoder;
using BCnEncoder.Shared.ImageFiles;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Gltf.Models;
using Extensions;
using SixLabors.ImageSharp.PixelFormats;

namespace CgfConverter.Renderers.Gltf;

public partial class GltfRenderer
{
    private int WriteMaterial(ChunkNode nodeChunk)
    {
        var baseMaterialIndex = _gltf.Materials.Count;

        var mats = nodeChunk.Materials?.SubMaterials;
        if (mats is null)
            return baseMaterialIndex;

        var decoder = new BcDecoder();
        foreach (var m in mats)
        {
            var useAlphaColor = m.OpacityValue != null && Math.Abs(m.OpacityValue.Value - 1.0) > 0;

            var diffuse = -1;
            // TODO: var diffuseDetail = -1;
            var normal = -1;
            var specular = -1;
            var metallicRoughness = -1;

            foreach (var texture in m.Textures!)
            {
                var texturePath = FileHandlingExtensions.ResolveTextureFile(texture.File, Args.DataDir);
                if (!File.Exists(texturePath))
                {
                    Utilities.Log(LogLevelEnum.Warning,
                        $"Skipping {texture.File} because the file could not be found.");
                    continue;
                }

                DdsFile ddsFile;
                using (var ddsfs = new FileStream(texturePath, FileMode.Open, FileAccess.Read))
                    ddsFile = DdsFile.Load(ddsfs);

                var width = (int) ddsFile.header.dwWidth;
                var height = (int) ddsFile.header.dwHeight;
                var raw = decoder.Decode(ddsFile);

                var name = Path.GetFileNameWithoutExtension(texturePath);

                switch (texture.Map)
                {
                    case Texture.MapTypeEnum.Bumpmap:
                    {
                        if (GltfRendererUtilities.HasMeaningfulAlphaChannel(raw))
                        {
                            // https://docs.cryengine.com/display/SDKDOC2/Detail+Maps
                            // Red: Diffuse
                            // Green: Normal Red
                            // Blue: Gloss
                            // Alpha: Normal Green
                            var rawNormal = new Rgb24[raw.Length];
                            var rawMetallicRoughness = new Rgb24[raw.Length];
                            var rawDiffuseDetail = new Rgb24[raw.Length];
                            for (var i = 0; i < raw.Length; i++)
                            {
                                var r = (rawNormal[i].R = raw[i].g) / 255f;
                                var g = (rawNormal[i].G = raw[i].a) / 255f;
                                var b = Math.Sqrt(1 - Math.Pow(r * 2 - 1, 2) - Math.Pow(g * 2 - 1, 2)) / 2 + 0.5f;
                                rawNormal[i].B = (byte) (255 * b);

                                // Its green channel contains roughness values
                                rawMetallicRoughness[i].G = (byte) (255 - raw[i].b);
                                // and its blue channel contains metalness values.

                                rawDiffuseDetail[i].R = rawDiffuseDetail[i].G = rawDiffuseDetail[i].B = raw[i].r;
                            }

                            normal = _gltf.AddTexture(name, width, height, rawNormal);
                            metallicRoughness = _gltf.AddTexture(name, width, height, rawMetallicRoughness);
                            // TODO: diffuseDetail = _gltfDataBuffer.AddTexture(name, width, height, rawDiffuseDetail);
                            Utilities.Log(LogLevelEnum.Warning,
                                $"Not implemented: detailed diffuse map {texture.File}");
                        }
                        else
                        {
                            normal = _gltf.AddTexture(name, width, height, raw, GltfWriter.SourceAlphaModes.Disable);
                        }

                        break;
                    }
                    case Texture.MapTypeEnum.Diffuse:
                        // TODO: SpecularValue seemingly has a meaningful value as opacity, but, really?
                        diffuse = _gltf.AddTexture(name, width, height, raw, GltfWriter.SourceAlphaModes.Automatic, useAlphaColor ? m.SpecularValue?.Red : null);
                        break;

                    case Texture.MapTypeEnum.Specular:
                        specular = _gltf.AddTexture(name, width, height, raw, GltfWriter.SourceAlphaModes.Automatic);
                        break;

                    default:
                        Utilities.Log($"Ignoring texture type {texture.Map}");
                        break;
                }
            }

            _gltf.ExtensionsUsed.Add("KHR_materials_specular");
            _gltf.Add(new GltfMaterial
            {
                Name = m.Name,
                AlphaMode = useAlphaColor ? GltfMaterialAlphaMode.Blend : GltfMaterialAlphaMode.Opaque,
                DoubleSided = true,
                NormalTexture = normal == -1
                    ? null
                    : new GltfTextureInfo
                    {
                        Index = normal,
                    },
                PbrMetallicRoughness = new GltfMaterialPbrMetallicRoughness
                {
                    BaseColorTexture = diffuse == -1
                        ? null
                        : new GltfTextureInfo
                        {
                            Index = diffuse,
                        },
                    BaseColorFactor = new[]
                    {
                        m.DiffuseValue?.Red ?? 1f,
                        m.DiffuseValue?.Green ?? 1f,
                        m.DiffuseValue?.Blue ?? 1f,
                        (float) (m.OpacityValue ?? 1.0),
                    },
                    MetallicFactor = 0f,
                    RoughnessFactor = 1f,
                    MetallicRoughnessTexture = metallicRoughness == -1
                        ? null
                        : new GltfTextureInfo
                        {
                            Index = metallicRoughness,
                        },
                },
                EmissiveFactor = new[]
                {
                    m.EmissiveValue?.Red ?? 0f,
                    m.EmissiveValue?.Green ?? 0f,
                    m.EmissiveValue?.Blue ?? 0f,
                },
                Extensions = new GltfExtensions
                {
                    KhrMaterialsSpecular = new GltfExtensionKhrMaterialsSpecular
                    {
                        SpecularColorFactor = m.SpecularValue == null
                            ? null
                            : new[]
                            {
                                m.SpecularValue.Red,
                                m.SpecularValue.Green,
                                m.SpecularValue.Blue,
                            },
                        SpecularColorTexture = specular == -1
                            ? null
                            : new GltfTextureInfo
                            {
                                Index = specular,
                            },
                    },
                },
            });
        }

        return baseMaterialIndex;
    }
}