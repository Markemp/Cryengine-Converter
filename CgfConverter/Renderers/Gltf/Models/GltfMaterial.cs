using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfMaterial
{
    [JsonProperty("doubleSided", NullValueHandling = NullValueHandling.Ignore)]
    public bool? DoubleSided;

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    [JsonProperty("normalTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfMaterialTextureSpecifier? NormalTexture;

    [JsonProperty("pbrMetallicRoughness", NullValueHandling = NullValueHandling.Ignore)]
    public GltfMaterialPbrMetallicRoughness? PbrMetallicRoughness;

    [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
    public GltfExtensions? Extensions;
}