using System.Numerics;
using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfMaterialPbrMetallicRoughness
{
    [JsonProperty("baseColorFactor", NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float[]? BaseColorFactor = {1f, 1f, 1f, 1f};

    [JsonProperty("baseColorTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? BaseColorTexture;

    [JsonProperty("metallicFactor", NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? MetallicFactor = 1f;

    [JsonProperty("roughnessFactor", NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? RoughnessFactor = 1f;

    [JsonProperty("metallicRoughnessTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? MetallicRoughnessTexture;
}