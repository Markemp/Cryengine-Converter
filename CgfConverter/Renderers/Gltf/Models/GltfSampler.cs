using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfSampler
{
    [JsonProperty("magFilter")] public GltfSamplerFilters MagFilter = GltfSamplerFilters.Linear;

    [JsonProperty("minFilter")] public GltfSamplerFilters MinFilter = GltfSamplerFilters.LinearMipmapLinear;
}