using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfExtensionMaterialsEmissiveStrength {
    [JsonProperty(
        "emissiveStrength",
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? EmissiveStrength = 1f;
}
