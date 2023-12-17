using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CgfConverter.Renderers.MaterialTextures;

public class ParsedGenMask : IEnumerable<string>, ICloneable
{
    private HashSet<string> _items;

    public ParsedGenMask() => _items = new();

    public ParsedGenMask(string? stringGenMask) : this(
        (stringGenMask ?? "").Split("%", StringSplitOptions.RemoveEmptyEntries))
    {
    }

    public ParsedGenMask(IEnumerable<string> genMaskSet)
    {
        _items = genMaskSet.ToHashSet();
        UseBumpMap = _items.Remove("BUMP_MAP");
        UseSpecularMap = _items.Remove("GLOSS_MAP");
        UseHeightGlossInNormalMap = _items.Remove("BLENDHEIGHT_DISPL");
        UseSpecAlphaInDiffuseMap = _items.Remove("GLOSS_DIFFUSEALPHA");
        UseGlossInSpecularMap = _items.Remove("SPECULARPOW_GLOSSALPHA");
        UseScatterGlossInNormalMap = _items.Remove("TEMP_SKIN");
        UseNormalMapInDetailMap = _items.Remove("DETAIL_TEXTURE_IS_NORMALMAP");
        UseInvertedBlendMap = _items.Remove("BLENDHEIGHT_INVERT");
        UseGlowDecalMap = _items.Remove("DECAL_ALPHAGLOW");
        UseBlendSpecularInSubSurfaceMap = _items.Remove("BLENDSPECULAR");
        UseBlendDiffuseInCustomMap = _items.Remove("BLENDLAYER");
        UseDirtLayerInCustomMap2 = _items.Remove("DIRTLAYER");
        UseAddNormalInCustomMap2 = _items.Remove("BLENDNORMAL_ADD");
        UseBlurRefractionInCustomMap2 = _items.Remove("BLUR_REFRACTION");
        UseVertexColors = _items.Remove("VERTCOLORS");
    }

    public IEnumerator<string> GetEnumerator()
    {
        if (UseBumpMap) yield return "BUMP_MAP";
        if (UseSpecularMap) yield return "GLOSS_MAP";
        if (UseHeightGlossInNormalMap) yield return "BLENDHEIGHT_DISPL";
        if (UseSpecAlphaInDiffuseMap) yield return "GLOSS_DIFFUSEALPHA";
        if (UseGlossInSpecularMap) yield return "SPECULARPOW_GLOSSALPHA";
        if (UseScatterGlossInNormalMap) yield return "TEMP_SKIN";
        if (UseNormalMapInDetailMap) yield return "DETAIL_TEXTURE_IS_NORMALMAP";
        if (UseInvertedBlendMap) yield return "BLENDHEIGHT_INVERT";
        if (UseGlowDecalMap) yield return "DECAL_ALPHAGLOW";
        if (UseBlendSpecularInSubSurfaceMap) yield return "BLENDSPECULAR";
        if (UseBlendDiffuseInCustomMap) yield return "BLENDLAYER";
        if (UseDirtLayerInCustomMap2) yield return "DIRTLAYER";
        if (UseAddNormalInCustomMap2) yield return "BLENDNORMAL_ADD";
        if (UseBlurRefractionInCustomMap2) yield return "BLUR_REFRACTION";
        if (UseVertexColors) yield return "VERTCOLORS";
        foreach (string item in _items)
            yield return item;
    }

    public bool UseBumpMap { get; set; }
    public bool UseSpecularMap { get; set; }
    public bool UseScatterGlossInNormalMap { get; set; }
    public bool UseHeightGlossInNormalMap { get; set; }
    public bool UseSpecAlphaInDiffuseMap { get; set; }
    public bool UseGlossInSpecularMap { get; set; }
    public bool UseNormalMapInDetailMap { get; set; }
    public bool UseInvertedBlendMap { get; set; }
    public bool UseGlowDecalMap { get; set; }
    public bool UseBlendSpecularInSubSurfaceMap { get; set; }
    public bool UseBlendDiffuseInCustomMap { get; set; }
    public bool UseDirtLayerInCustomMap2 { get; set; }
    public bool UseAddNormalInCustomMap2 { get; set; }
    public bool UseBlurRefractionInCustomMap2 { get; set; }
    public bool UseVertexColors { get; set; }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        string r = string.Join('%', this);
        return r == "" ? "" : "%" + r;
    }

    public object Clone()
    {
        ParsedGenMask res = (ParsedGenMask) MemberwiseClone();
        res._items = _items.ToHashSet();
        return res;
    }
}