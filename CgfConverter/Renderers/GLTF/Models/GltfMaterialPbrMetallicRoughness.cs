using System.Numerics;
using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfMaterialPbrMetallicRoughness
{
    [JsonProperty("baseColorFactor", NullValueHandling = NullValueHandling.Ignore)]
    public Vector4? BaseColorFactor;

    [JsonProperty("baseColorTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfMaterialTextureSpecifier? BaseColorTexture;

    [JsonProperty("metallicFactor", NullValueHandling = NullValueHandling.Ignore)]
    public float? MetallicFactor;

    [JsonProperty("metallicTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfMaterialTextureSpecifier? MetallicTexture;

    [JsonProperty("roughnessFactor", NullValueHandling = NullValueHandling.Ignore)]
    public float? RoughnessFactor;

    [JsonProperty("metallicRoughnessTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfMaterialTextureSpecifier? MetallicRoughnessTexture;
}