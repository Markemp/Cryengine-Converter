using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CgfConverter.Models.Materials;
using CgfConverter.Utils;
using Extensions;

namespace CgfConverter.Renderers.MaterialTextures;

public class MaterialTextureManager
{
    private readonly TaggedLogger _log = new(nameof(MaterialTextureManager));

    private readonly ArgsHandler _args;

    private readonly Dictionary<IMaterialTextureKey, MaterialTexture> _textures = new();

    public MaterialTextureManager(ArgsHandler args)
    {
        _args = args;
    }

    public void Clear()
    {
        foreach (MaterialTexture t in _textures.Values)
            t.Dispose();
        _textures.Clear();
    }

    public MaterialTexture? FromCryTexture(
        IReadOnlyDictionary<Texture.MapTypeEnum, Texture> cryTextures,
        params Texture.MapTypeEnum[] mapTypes)
    {
        foreach (Texture.MapTypeEnum mapType in mapTypes)
        {
            if (!cryTextures.TryGetValue(mapType, out Texture? t))
                continue;

            string texturePath = FileHandlingExtensions.ResolveTextureFile(t.File, _args.PackFileSystem, _args.DataDirs);
            string normalizedPath = FileHandlingExtensions.CombineAndNormalizePath(texturePath);

            if (!_args.PackFileSystem.Exists(normalizedPath))
            {
                _log.W("Texture file not found: {0}", t.File);
                continue;
            }

            if (GetTextureFromPath(normalizedPath) is {} tt)
                return tt;
        }

        return null;
    }

    public MaterialTextureSet CreateSet(Material cryMaterial)
    {
        Dictionary<Texture.MapTypeEnum, Texture> texturesDict = cryMaterial.Textures!
            .Where(
                x => x.TexType == Texture.TypeEnum.Default
                    && x.File != "nearest_cubemap"
                    && !string.IsNullOrWhiteSpace(x.File))
            .ToDictionary(x => x.Map, x => x);

        MaterialTexture? rawDiffuse = FromCryTexture(
            texturesDict,
            Texture.MapTypeEnum.TexSlot1,
            Texture.MapTypeEnum.TexSlot9,
            Texture.MapTypeEnum.Diffuse);
        MaterialTexture? rawNormal = FromCryTexture(
            texturesDict,
            Texture.MapTypeEnum.TexSlot2,
            Texture.MapTypeEnum.Normals);
        MaterialTexture? rawSpecular = FromCryTexture(
            texturesDict,
            Texture.MapTypeEnum.TexSlot4,
            Texture.MapTypeEnum.TexSlot10,
            Texture.MapTypeEnum.Specular);

        ParsedGenMask genMask = new(cryMaterial.StringGenMask);

        MaterialTextureSet result = new();

        if (rawSpecular is not null)
        {
            if (genMask.UseGlossInSpecularMap)
            {
                result.SpecularGlossiness = rawSpecular;
                result.Glossiness = ExtractChannel(result.SpecularGlossiness, MaterialTextureChannel.Alpha);
            }

            result.Specular = DropAlpha(rawSpecular);
        }

        if (rawDiffuse is not null)
        {
            if (genMask.UseSpecAlphaInDiffuseMap)
            {
                if (result.Specular is null)
                {
                    result.Specular = rawDiffuse;
                }
                else
                {
                    result.Specular = MergeChannels(
                        rgb: result.Specular,
                        a: ExtractChannel(rawDiffuse, MaterialTextureChannel.Alpha));
                }
            }

            result.Diffuse = cryMaterial.AlphaTest == 0 ? DropAlpha(rawDiffuse) : rawDiffuse;
        }

        if (rawNormal is not null)
        {
            if (genMask.UseScatterGlossInNormalMap)
            {
                result.Normal = MergeChannels(
                    r: ExtractChannel(rawNormal, MaterialTextureChannel.Green),
                    g: ExtractChannel(rawNormal, MaterialTextureChannel.Alpha),
                    b: SolidChannel(rawNormal, 1f),
                    a: null);
                result.Scatter = ExtractChannel(rawNormal, MaterialTextureChannel.Red);
                result.Glossiness ??= ExtractChannel(rawNormal, MaterialTextureChannel.Blue);
            }
            else if (genMask.UseHeightGlossInNormalMap)
            {
                result.Normal = MergeChannels(
                    r: ExtractChannel(rawNormal, MaterialTextureChannel.Green),
                    g: ExtractChannel(rawNormal, MaterialTextureChannel.Alpha),
                    b: SolidChannel(rawNormal, 1f),
                    a: null);
                result.Height = ExtractChannel(rawNormal, MaterialTextureChannel.Red);
                result.Glossiness ??= ExtractChannel(rawNormal, MaterialTextureChannel.Blue);
            }
            else
            {
                result.Normal = DropAlpha(rawNormal);
            }
        }

        if (result.SpecularGlossiness is null && result.Specular is not null && result.Glossiness is not null)
        {
            result.SpecularGlossiness = MergeChannels(rgb: result.Specular, a: result.Glossiness);
        }

        if (result.MetallicRoughness is null && result.Glossiness is not null)
        {
            result.MetallicRoughness = MergeChannels(
                r: SolidChannel(result.Glossiness, 1f),
                g: Invert(result.Glossiness),
                b: SolidChannel(result.Glossiness, 1f),
                a: null);
        }

        return result;
    }

    private MaterialTexture? GetTextureFromPath(string path)
    {
        FileMaterialTextureKey key = new(path);
        if (_textures.TryGetValue(key, out MaterialTexture? t))
            return t;

        try
        {
            DdsFile ddsFile;
            if (DDSFileCombiner.IsSplitDDS(path))
            {
                using Stream ddsfs = DDSFileCombiner.CombineToStream(path);
                ddsFile = DdsFile.Load(ddsfs);
            }
            else
            {
                using Stream ddsfs = _args.PackFileSystem.GetStream(path);
                ddsFile = DdsFile.Load(ddsfs);
            }

            return _textures[key] = new MaterialTexture(
                key,
                new BcDecoder().Decode(ddsFile),
                (int) ddsFile.header.dwWidth,
                (int) ddsFile.header.dwHeight,
                4);
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

        return _textures[key] = new MaterialTexture(key, rgba.Data.ToArray(), rgba.Width, rgba.Height, 3);
    }

    private MaterialTexture SolidChannel(MaterialTexture spec, float value)
    {
        IMaterialTextureKey key = new SolidChannelMaterialTextureKey(value, spec.Width, spec.Height);
        if (_textures.TryGetValue(key, out MaterialTexture? mt))
            return mt;

        ColorRgba32[] data = new ColorRgba32[spec.Data.Length];
        byte bv = (byte) Math.Clamp(value * 255f, 0, 255);
        foreach (ref ColorRgba32 d in data.AsSpan())
            d.r = bv;
        return _textures[key] = new MaterialTexture(key, data, spec.Width, spec.Height, 1);
    }
}
