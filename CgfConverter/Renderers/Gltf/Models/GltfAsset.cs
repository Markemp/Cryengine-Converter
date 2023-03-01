using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfAsset
{
    [JsonProperty("generator", NullValueHandling = NullValueHandling.Ignore)]
    public string? Generator = "Cryengine Converter";

    [JsonProperty("version")] public string Version = "2.0";
}