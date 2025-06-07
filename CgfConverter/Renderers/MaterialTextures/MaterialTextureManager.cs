using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CgfConverter.Models.Materials;
using CgfConverter.Utilities;
using CgfConverter.Utils;
using Extensions;

namespace CgfConverter.Renderers.MaterialTextures;

public class MaterialTextureManager
{
    private readonly TaggedLogger _log = new(nameof(MaterialTextureManager));

    private readonly ArgsHandler _args;

    private readonly Dictionary<IMaterialTextureKey, MaterialTexture> _textures = [];

    public MaterialTextureManager(ArgsHandler args)
    {
        _args = args;
    }

    public MaterialTextureSet CreateSet(Material cryMaterial)
    {
        Dictionary<Texture.MapTypeEnum, Texture> texturesDict = cryMaterial.Textures!
            .Where(
                x => x.TexType == Texture.TypeEnum.Default
                    && x.File != "nearest_cubemap"
                    && !string.IsNullOrWhiteSpace(x.File))
            .ToDictionary(x => x.Map, x => x);

        MaterialTexture? rawDiffuse, rawNormal, rawSpecular;

        rawDiffuse = FromCryTexture(
            texturesDict,
            Texture.MapTypeEnum.TexSlot1,
            Texture.MapTypeEnum.TexSlot9,
            Texture.MapTypeEnum.Diffuse);
        rawNormal = FromCryTexture(
            texturesDict,
            Texture.MapTypeEnum.TexSlot2,
            Texture.MapTypeEnum.Normals);
        rawSpecular = FromCryTexture(
            texturesDict,
            Texture.MapTypeEnum.TexSlot4,
            Texture.MapTypeEnum.TexSlot10,
            Texture.MapTypeEnum.Specular);

        ParsedGenMask genMask = new(cryMaterial.StringGenMask);

        MaterialTextureSet materialTextureSet = new();

        if (rawSpecular is not null)
        {
            if (genMask.UseGlossInSpecularMap)
            {
                materialTextureSet.SpecularGlossiness = rawSpecular;
                materialTextureSet.Glossiness = _args.EmbedTextures
                    ? ExtractChannel(materialTextureSet.SpecularGlossiness, MaterialTextureChannel.Alpha)
                    : rawSpecular; // verify alpha is connected to glossiness
            }

            materialTextureSet.Specular = DropAlpha(rawSpecular);
        }

        if (rawDiffuse is not null)
        {
            if (genMask.UseSpecAlphaInDiffuseMap)
            {
                if (materialTextureSet.Specular is null)
                    materialTextureSet.Specular = rawDiffuse;
                else
                {
                    materialTextureSet.Specular = _args.EmbedTextures
                        ? MergeChannels(
                            rgb: materialTextureSet.Specular,
                            a: ExtractChannel(rawDiffuse, MaterialTextureChannel.Alpha))
                        : rawDiffuse;  // verify alpha is connected to specular
                }
            }

            materialTextureSet.Diffuse = cryMaterial.AlphaTest == 0
                ? _args.EmbedTextures ? DropAlpha(rawDiffuse) : rawDiffuse
                : rawDiffuse;
        }

        if (rawNormal is not null)
        {
            if (genMask.UseScatterGlossInNormalMap)
            {
                materialTextureSet.Normal = _args.EmbedTextures
                    ? MergeChannels(
                        r: ExtractChannel(rawNormal, MaterialTextureChannel.Green),
                        g: ExtractChannel(rawNormal, MaterialTextureChannel.Alpha),
                        b: SolidChannel(rawNormal, 1f),
                        a: null)
                    : rawNormal;
                materialTextureSet.Scatter = _args.EmbedTextures
                    ? ExtractChannel(rawNormal, MaterialTextureChannel.Red)
                    : rawNormal;
                materialTextureSet.Glossiness = _args.EmbedTextures
                    ? ExtractChannel(rawNormal, MaterialTextureChannel.Blue)
                    : rawNormal;
            }
            else if (genMask.UseHeightGlossInNormalMap)
            {
                materialTextureSet.Normal = _args.EmbedTextures
                    ? MergeChannels(
                        r: ExtractChannel(rawNormal, MaterialTextureChannel.Green),
                        g: ExtractChannel(rawNormal, MaterialTextureChannel.Alpha),
                        b: SolidChannel(rawNormal, 1f),
                        a: null)
                    : rawNormal;
                materialTextureSet.Height = _args.EmbedTextures
                    ? ExtractChannel(rawNormal, MaterialTextureChannel.Red)
                    : rawNormal;
                materialTextureSet.Glossiness = _args.EmbedTextures
                    ? ExtractChannel(rawNormal, MaterialTextureChannel.Blue)
                    : rawNormal;
            }
            else
                materialTextureSet.Normal = _args.EmbedTextures
                    ? DropAlpha(rawNormal)
                    : rawNormal;
        }

        if (materialTextureSet.SpecularGlossiness is null && materialTextureSet.Specular is not null && materialTextureSet.Glossiness is not null)
            materialTextureSet.SpecularGlossiness = _args.EmbedTextures
                ? MergeChannels(rgb: materialTextureSet.Specular, a: materialTextureSet.Glossiness)
                : rawSpecular;

        if (materialTextureSet.MetallicRoughness is null && materialTextureSet.Glossiness is not null)
        {
            materialTextureSet.MetallicRoughness = _args.EmbedTextures
                ? MergeChannels(
                    r: SolidChannel(materialTextureSet.Glossiness, 1f),
                    g: Invert(materialTextureSet.Glossiness),
                    b: SolidChannel(materialTextureSet.Glossiness, 1f),
                    a: null)
                : null;
        }

        return materialTextureSet;
    }

    public MaterialTexture? FromCryTexture(
        IReadOnlyDictionary<Texture.MapTypeEnum, Texture> cryTextures,
        params Texture.MapTypeEnum[] mapTypes)
    {
        foreach (Texture.MapTypeEnum mapType in mapTypes)
        {
            if (!cryTextures.TryGetValue(mapType, out Texture? t))
                continue;

            string texturePath = FileHandlingExtensions.ResolveTextureFile(t.File, _args.PackFileSystem, [_args.DataDir]);
            string normalizedPath = FileHandlingExtensions.CombineAndNormalizePath(texturePath);

            if (!_args.PackFileSystem.Exists(normalizedPath))
            {
                _log.W("Texture file not found: {0}", t.File);
                continue;
            }

            if (GetMaterialTextureFromPath(normalizedPath) is { } tt)
                return tt;
        }

        return null;
    }

    private MaterialTexture? GetMaterialTextureFromPath(string path)
    {
        FileMaterialTextureKey key = new(path);
        if (_textures.TryGetValue(key, out MaterialTexture? t))
            return t;

        try
        {
            if (_args.EmbedTextures)
            {
                DdsFile ddsFile;
                using Stream ddsfs = _args.PackFileSystem.GetStream(path);
                ddsFile = DdsFile.Load(ddsfs);

                return _textures[key] = new MaterialTexture(
                    key,
                    new BcDecoder().Decode(ddsFile),
                    (int)ddsFile.header.dwWidth,
                    (int)ddsFile.header.dwHeight,
                    4);
            }
            else
            {
                // If not embedding, just return a placeholder texture
                return _textures[key] = new MaterialTexture(key, null, null, null, null);
            }
        }
        catch (Exception e)
        {
            _log.E(e, "Material error: Failed to decode: {0}", path);
            return null;
        }
    }

    private MaterialTexture ExtractChannel(MaterialTexture parent, MaterialTextureChannel channel)
    {
        IMaterialTextureKey key = new ChannelMaterialTextureKey(parent.Key, channel);
        if (_textures.TryGetValue(key, out MaterialTexture? mt))
            return mt;

        ColorRgba32[] data2 = new ColorRgba32[parent.Data.Length];
        switch (channel)
        {
            case MaterialTextureChannel.Red:
                for (int i = 0; i < data2.Length; i++)
                    data2[i].r = parent.Data[i].r;
                break;
            case MaterialTextureChannel.Green:
                for (int i = 0; i < data2.Length; i++)
                    data2[i].r = parent.Data[i].g;
                break;
            case MaterialTextureChannel.Blue:
                for (int i = 0; i < data2.Length; i++)
                    data2[i].r = parent.Data[i].b;
                break;
            case MaterialTextureChannel.Alpha:
                for (int i = 0; i < data2.Length; i++)
                    data2[i].r = parent.Data[i].a;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
        }

        return _textures[key] = new MaterialTexture(key, data2, parent.Width, parent.Height, 1);
    }

    private MaterialTexture Invert(MaterialTexture parent)
    {
        IMaterialTextureKey key = new InvertedMaterialTextureKey(parent.Key);
        if (_textures.TryGetValue(key, out MaterialTexture? mt))
            return mt;

        ColorRgba32[] data2 = new ColorRgba32[parent.Data.Length];
        for (int i = 0; i < data2.Length; i++)
            data2[i] = new ColorRgba32(255, 255, 255, 255) - parent.Data[i];

        return _textures[key] = new MaterialTexture(key, data2, parent.Width, parent.Height, parent.NumChannels);
    }

    private MaterialTexture MergeChannels(
        MaterialTexture r,
        MaterialTexture g,
        MaterialTexture b,
        MaterialTexture? a)
    {
        IMaterialTextureKey key = new MergedMaterialTextureKey(r.Key, g.Key, b.Key, a?.Key);
        if (_textures.TryGetValue(key, out MaterialTexture? mt))
            return mt;

        ColorRgba32[] data2 = new ColorRgba32[r.Data.Length];
        for (int i = 0; i < data2.Length; i++)
            data2[i] = new ColorRgba32(r.Data[i].r, g.Data[i].r, b.Data[i].r, a?.Data[i].r ?? 0);

        return _textures[key] = new MaterialTexture(key, data2, r.Width, r.Height, a is null ? 3 : 4);
    }

    private MaterialTexture MergeChannels(
        MaterialTexture rgb,
        MaterialTexture a)
    {
        IMaterialTextureKey key = new MergedMaterialTextureKey(
            r: new ChannelMaterialTextureKey(rgb.Key, MaterialTextureChannel.Red),
            g: new ChannelMaterialTextureKey(rgb.Key, MaterialTextureChannel.Green),
            b: new ChannelMaterialTextureKey(rgb.Key, MaterialTextureChannel.Blue),
            a: a.Key);
        if (_textures.TryGetValue(key, out MaterialTexture? mt))
            return mt;

        ColorRgba32[] data2 = rgb.Data.ToArray();
        for (int i = 0; i < data2.Length; i++)
            data2[i].a = a.Data[i].r;

        return _textures[key] = new MaterialTexture(key, data2, rgb.Width, rgb.Height, 4);
    }

    private MaterialTexture DropAlpha(MaterialTexture rgba)
    {
        IMaterialTextureKey key = new MergedMaterialTextureKey(
            r: new ChannelMaterialTextureKey(rgba.Key, MaterialTextureChannel.Red),
            g: new ChannelMaterialTextureKey(rgba.Key, MaterialTextureChannel.Green),
            b: new ChannelMaterialTextureKey(rgba.Key, MaterialTextureChannel.Blue),
            a: null);
        if (_textures.TryGetValue(key, out MaterialTexture? mt))
            return mt;

        return _textures[key] = new MaterialTexture(key, rgba.Data?.ToArray(), rgba.Width, rgba.Height, 3);
    }

    private MaterialTexture SolidChannel(MaterialTexture spec, float value)
    {
        IMaterialTextureKey key = new SolidChannelMaterialTextureKey(value, spec.Width ?? default, spec.Height ?? default);
        if (_textures.TryGetValue(key, out MaterialTexture? mt))
            return mt;

        ColorRgba32[] data = new ColorRgba32[spec.Data.Length];
        byte bv = (byte) Math.Clamp(value * 255f, 0, 255);
        foreach (ref ColorRgba32 d in data.AsSpan())
            d.r = bv;
        return _textures[key] = new MaterialTexture(key, data, spec.Width, spec.Height, 1);
    }

    public void Clear()
    {
        foreach (MaterialTexture t in _textures.Values)
            t.Dispose();
        _textures.Clear();
    }
}
