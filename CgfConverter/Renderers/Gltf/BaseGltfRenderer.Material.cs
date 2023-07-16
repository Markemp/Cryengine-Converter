using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CgfConverter.CryEngineCore;
using CgfConverter.Materials;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Utils;
using Extensions;
using SixLabors.ImageSharp.PixelFormats;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    private class WrittenMaterial
    {
        public const int SkippedFromArgsIndex = -1;
        
        public readonly int Index;
        public readonly Material Source;
        public readonly GltfMaterial? Target;

        public WrittenMaterial(int index, Material source, GltfMaterial? target)
        {
            Index = index;
            Source = source;
            Target = target;
        }

        public bool IsSkippedFromArgs => Index == SkippedFromArgsIndex;
    } 
    
    private Dictionary<int, WrittenMaterial> WriteMaterial(ChunkNode nodeChunk)
    {
        var materialIndices = new Dictionary<int, WrittenMaterial>();

        var submats = nodeChunk.Materials?.SubMaterials;
        if (submats is null)
            return materialIndices;
        
        var materialSetName = nodeChunk.MaterialLibraryChunk?.Name ?? nodeChunk._model.FileName;
        
        var matIds = nodeChunk._model.ChunkMap
            .Select(x => x.Value)
            .OfType<ChunkMeshSubsets>()
            .SelectMany(x => x.MeshSubsets)
            .Select(x => x.MatID)
            .ToHashSet();

        var decoder = new BcDecoder();
        foreach (var matId in matIds)
        {
            if (matId >= submats.Length)
            {
                Log.W("Material[{0}:{1}]: Not found.", materialSetName, matId);
                continue;
            }

            var m = submats[matId];

            if ((m.Name is not null && Args.IsMeshMaterialExcluded(m.Name))
                || (m.Shader is not null && Args.IsMeshMaterialShaderExcluded(m.Shader)))
            {
                materialIndices[matId] = new WrittenMaterial(WrittenMaterial.SkippedFromArgsIndex, m, null);
                continue;
            }

            if (_materialMap.TryGetValue(m, out var existingIndex))
            {
                materialIndices[matId] = new WrittenMaterial(existingIndex, m, _gltfRoot.Materials[existingIndex]);
                continue;
            }

            if ((m.MaterialFlags & MaterialFlags.NoDraw) != 0)
            {
                var matIndexHidden = _materialMap[m] = AddMaterial(new GltfMaterial
                {
                    Name = m.Name,
                    AlphaMode = GltfMaterialAlphaMode.Mask,
                    AlphaCutoff = 1f, // Fully transparent
                    DoubleSided = true, // (m.MaterialFlags & MaterialFlags.TwoSided) != 0,
                    PbrMetallicRoughness = new GltfMaterialPbrMetallicRoughness
                    {
                        BaseColorFactor = new[] {0f, 0f, 0f, 0f},
                    },
                });
                materialIndices[matId] = new WrittenMaterial(matIndexHidden, m, _gltfRoot.Materials[matIndexHidden]);
                continue;
            }

            var useAlphaColor = m.OpacityValue != null && Math.Abs(m.OpacityValue.Value - 1.0) > 0;

            // SpecularValue seemingly has a meaningful value as opacity for characters.
            // Not for decals.
            // ???
            var preferredAlphaColor = useAlphaColor ? m.OpacityValue : null;

            var diffuse = -1;
            // TODO: var diffuseDetail = -1;
            var emissive = -1;
            var normal = -1;
            var specular = -1;
            var metallicRoughness = -1;

            foreach (var texture in m.Textures!)
            {
                // seems to be a sentinel value
                if (texture.File == "nearest_cubemap" || string.IsNullOrWhiteSpace(texture.File))
                    continue;

                var texturePath = FileHandlingExtensions.ResolveTextureFile(texture.File, Args.PackFileSystem);
                if (!Args.PackFileSystem.Exists(texturePath))
                {
                    Log.W("Material[{0}:{1}]: Texture file not found: {2}", materialSetName, matId, texture.File);
                    continue;
                }

                DdsFile ddsFile;
                if (DDSFileCombiner.IsSplitDDSIsSplitDDS(texturePath))
                {
                    using var ddsfs = DDSFileCombiner.CombineToStream(texturePath);
                    ddsFile = DdsFile.Load(ddsfs);
                }
                else
                {
                    using var ddsfs = Args.PackFileSystem.GetStream(texturePath);
                    ddsFile = DdsFile.Load(ddsfs);
                }

                var width = (int) ddsFile.header.dwWidth;
                var height = (int) ddsFile.header.dwHeight;
                ColorRgba32[] raw;
                try
                {
                    raw = decoder.Decode(ddsFile);
                }
                catch (Exception e)
                {
                    Log.E(e, "Material[{0}:{1}]: Failed to decode: {2}", materialSetName, matId, texture.File);
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(texturePath);

                switch (texture.Map)
                {
                    case Texture.MapTypeEnum.Normals:
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

                            normal = AddTexture(name, width, height, rawNormal);
                            metallicRoughness = AddTexture(name, width, height, rawMetallicRoughness);
                            
                            // TODO: diffuseDetail = _gltfDataBuffer.AddTexture(name, width, height, rawDiffuseDetail);
                            Log.D("Material[{0}:{1}]: Detailed diffuse map is not implemented: {2}",
                                materialSetName, matId, texture.File);
                        }
                        else
                        {
                            normal = AddTexture(name, width, height, raw, SourceAlphaModes.Disable);
                        }

                        break;
                    }
                    case Texture.MapTypeEnum.Diffuse:
                        if (m.GlowAmount > 0)
                        {
                            var rawEmissive = new Rgb24[raw.Length];
                            for (var j = 0; j < raw.Length; j++)
                            {
                                rawEmissive[j].R = (byte)(raw[j].r * raw[j].a / 255);
                                rawEmissive[j].G = (byte)(raw[j].g * raw[j].a / 255);
                                rawEmissive[j].B = (byte)(raw[j].b * raw[j].a / 255);
                            }
                            
                            emissive = AddTexture(name, width, height, rawEmissive);
                        }
                        diffuse = AddTexture(name, width, height, raw, SourceAlphaModes.Disable,
                            preferredAlphaColor);
                        break;

                    case Texture.MapTypeEnum.Specular:
                        specular = AddTexture(name, width, height, raw, SourceAlphaModes.Automatic);
                        break;

                    default:
                        Log.D("Material[{0}:{1}]: Ignoring texture type {2}: {3}",
                            materialSetName, matId, texture.MapString, texture.File);
                        break;
                }
            }
            
            const float legacyHdrDynMult = 2.0f;
            const float legacyIntensityScale = 10.0f;
            const float emissiveIntensitySoftMax = 200.0f;
            var emissionMultiplier = (float)(Math.Min(
                Math.Pow(m.GlowAmount * legacyHdrDynMult, legacyHdrDynMult) * legacyIntensityScale,
                emissiveIntensitySoftMax) / 65535f);
            // TODO: How are this and m.emissiveValue related?

            _gltfRoot.ExtensionsUsed.Add("KHR_materials_specular");
            var matIndex = _materialMap[m] = AddMaterial(new GltfMaterial
            {
                Name = m.Name,
                AlphaMode = useAlphaColor
                    ? GltfMaterialAlphaMode.Blend
                    : m.AlphaTest == 0
                        ? GltfMaterialAlphaMode.Opaque
                        : GltfMaterialAlphaMode.Mask,
                AlphaCutoff = useAlphaColor || m.AlphaTest == 0 ? null : (float) m.AlphaTest,
                DoubleSided = true, // (m.MaterialFlags & MaterialFlags.TwoSided) != 0,
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
                    emissionMultiplier,
                    emissionMultiplier,
                    emissionMultiplier,
                },
                EmissiveTexture = emissive == -1
                    ? null
                    : new GltfTextureInfo
                    {
                        Index = emissive,
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
            materialIndices[matId] = new WrittenMaterial(matIndex, m, _gltfRoot.Materials[matIndex]);
        }

        return materialIndices;
    }
}