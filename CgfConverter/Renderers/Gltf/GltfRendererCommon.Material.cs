﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCnEncoder.Decoder;
using BCnEncoder.Shared.ImageFiles;
using CgfConverter.CryEngineCore;
using CgfConverter.Materials;
using CgfConverter.Renderers.Gltf.Models;
using Extensions;
using SixLabors.ImageSharp.PixelFormats;

namespace CgfConverter.Renderers.Gltf;

public partial class GltfRendererCommon
{
    private readonly Dictionary<Material, int> _materialMap = new();

    private Dictionary<int, int?> WriteMaterial(ChunkNode nodeChunk)
    {
        var materialIndices = new Dictionary<int, int?>();
        return materialIndices;

        var childIds = nodeChunk.MaterialLibraryChunk?.ChildIDs;
        var submats = nodeChunk.Materials?.SubMaterials;
        if (submats is null || childIds is null)
            return materialIndices;

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
                Utilities.Log(LogLevelEnum.Error, $"Requested material ID {matId} was not found.");
                continue;
            }

            var m = submats[matId];

            if (_materialMap.TryGetValue(m, out var existingIndex))
            {
                materialIndices[matId] = existingIndex;
                continue;
            }

            if ((m.MaterialFlags & MaterialFlags.NoDraw) != 0 || string.IsNullOrWhiteSpace(m.StringGenMask))
            {
                _gltf.Add(new GltfMaterial
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

                materialIndices[matId] = _materialMap[m] = _gltf.Materials.Count - 1;
                continue;
            }

            // TODO: This ideally should be always true; need to figure out what's the alpha of diffuse really supposed to be
            var useAlphaColor = m.OpacityValue != null && Math.Abs(m.OpacityValue.Value - 1.0) > 0;

            // TODO: SpecularValue seemingly has a meaningful value as opacity, but, really?
            var preferredAlphaColor = useAlphaColor ? m.SpecularValue?.Red : null;

            var diffuse = -1;
            // TODO: var diffuseDetail = -1;
            var normal = -1;
            var specular = -1;
            var metallicRoughness = -1;

            foreach (var texture in m.Textures!)
            {
                // seems to be a sentinel value
                if (texture.File == "nearest_cubemap" || string.IsNullOrWhiteSpace(texture.File))
                    continue;

                var texturePath = FileHandlingExtensions.ResolveTextureFile(texture.File, _packFileSystem);
                if (!_packFileSystem.Exists(texturePath))
                {
                    Utilities.Log(LogLevelEnum.Warning,
                        $"Skipping {texture.File} because the file could not be found.");
                    continue;
                }

                DdsFile ddsFile;
                using (var ddsfs = _packFileSystem.GetStream(texturePath))
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
                        diffuse = _gltf.AddTexture(name, width, height, raw, GltfWriter.SourceAlphaModes.Automatic,
                            preferredAlphaColor);
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

            materialIndices[matId] = _materialMap[m] = _gltf.Materials.Count - 1;
        }

        return materialIndices;
    }
}