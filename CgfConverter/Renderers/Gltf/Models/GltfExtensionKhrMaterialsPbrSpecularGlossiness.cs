using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfExtensionKhrMaterialsPbrSpecularGlossiness {
    [JsonProperty("diffuseFactor", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float[]? DiffuseFactor;

    [JsonProperty("diffuseTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? DiffuseTexture;

    [JsonProperty("specularFactor", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float[]? SpecularFactor;

    [JsonProperty("glossinessFactor", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? GlossinessFactor;

    [JsonProperty("specularGlossinessTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? SpecularGlossinessTexture;
}
