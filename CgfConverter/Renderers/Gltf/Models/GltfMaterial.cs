using System;
using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfMaterial
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    [JsonProperty("pbrMetallicRoughness", NullValueHandling = NullValueHandling.Ignore)]
    public GltfMaterialPbrMetallicRoughness? PbrMetallicRoughness;

    [JsonProperty("normalTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? NormalTexture;

    [JsonProperty("occlusionTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? OcclusionTexture;

    [JsonProperty("emissiveTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? EmissiveTexture;

    [JsonProperty("emissiveFactor", NullValueHandling = NullValueHandling.Ignore)]
    public float[]? EmissiveFactor;

    [JsonIgnore] public GltfMaterialAlphaMode? AlphaMode;

    [JsonProperty("alphaCutoff", NullValueHandling = NullValueHandling.Ignore)]
    public float? AlphaCutoff;

    [JsonProperty("doubleSided", NullValueHandling = NullValueHandling.Ignore)]
    public bool? DoubleSided;

    [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
    public GltfExtensions? Extensions;

    [JsonProperty("alphaMode", NullValueHandling = NullValueHandling.Ignore)]
    public string? AlphaModeString
    {
        get => AlphaMode switch
        {
            null => null,
            GltfMaterialAlphaMode.Opaque => "OPAQUE",
            GltfMaterialAlphaMode.Mask => "MASK",
            GltfMaterialAlphaMode.Blend => "BLEND",
            _ => throw new ArgumentOutOfRangeException(nameof(AlphaMode)),
        };
        set => AlphaMode = value switch
        {
            null => null,
            "OPAQUE" => GltfMaterialAlphaMode.Opaque,
            "MASK" => GltfMaterialAlphaMode.Mask,
            "BLEND" => GltfMaterialAlphaMode.Blend,
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }

    public bool HasNormalTexture() =>
        NormalTexture is not null;

    public bool HasAnyTexture() =>
        EmissiveTexture is not null
        || OcclusionTexture is not null
        || PbrMetallicRoughness?.BaseColorTexture is not null
        || PbrMetallicRoughness?.MetallicRoughnessTexture is not null
        || Extensions?.KhrMaterialsSpecular?.SpecularTexture is not null
        || Extensions?.KhrMaterialsSpecular?.SpecularColorTexture is not null;
}