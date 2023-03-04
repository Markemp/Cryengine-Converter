using System;
using System.Collections.Generic;
using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Shared.ImageFiles;
using CgfConverter.CryEngineCore;
using CgfConverter.Materials;
using CgfConverter.Renderers.Gltf.Models;
using Extensions;
using SixLabors.ImageSharp.PixelFormats;

namespace CgfConverter.Renderers.Gltf;

public partial class GltfRenderer
{
    private readonly Dictionary<Material, int> _materialMap = new();
    
    private Dictionary<int, int?> WriteMaterial(ChunkNode nodeChunk)
    {
        var materialIndices = new Dictionary<int, int?>();

        var mats = nodeChunk.Materials?.SubMaterials;
        if (mats is null)
            return materialIndices;

        var decoder = new BcDecoder();
        for (var i = 0; i < mats.Length; i++)
        {
            var m = mats[i];
            if (_materialMap.TryGetValue(m, out var existingIndex))
            {
                materialIndices[i] = existingIndex;
                continue;
            }
            
            var useAlphaColor = m.OpacityValue != null && Math.Abs(m.OpacityValue.Value - 1.0) > 0;

            var diffuse = -1;
            // TODO: var diffuseDetail = -1;
            var normal = -1;
            var specular = -1;
            var metallicRoughness = -1;

            foreach (var texture in m.Textures!)
            {
                var texturePath = FileHandlingExtensions.ResolveTextureFile(texture.File, Args.PackFileSystem);
                if (!Args.PackFileSystem.Exists(texturePath))
                {
                    Utilities.Log(LogLevelEnum.Warning,
                        $"Skipping {texture.File} because the file could not be found.");
                    continue;
                }

                DdsFile ddsFile;
                using (var ddsfs = Args.PackFileSystem.GetStream(texturePath))
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
                            for (var j = 0; j < raw.Length; j++)
                            {
                                var r = (rawNormal[j].R = raw[j].g) / 255f;
                                var g = (rawNormal[j].G = raw[j].a) / 255f;
                                var b = Math.Sqrt(1 - Math.Pow(r * 2 - 1, 2) - Math.Pow(g * 2 - 1, 2)) / 2 + 0.5f;
                                rawNormal[j].B = (byte) (255 * b);

                                // Its green channel contains roughness values
                                rawMetallicRoughness[j].G = (byte) (255 - raw[j].b);
                                // and its blue channel contains metalness values.

                                rawDiffuseDetail[j].R = rawDiffuseDetail[j].G = rawDiffuseDetail[j].B = raw[j].r;
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

            materialIndices[i] = _materialMap[m] = _gltf.Materials.Count - 1;
        }

        return materialIndices;
    }
}