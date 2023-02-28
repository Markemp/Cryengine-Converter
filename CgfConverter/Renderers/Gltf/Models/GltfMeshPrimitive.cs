using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfMeshPrimitive
{
    [JsonProperty("attributes")] public GltfMeshPrimitiveAttributes Attributes = new();

    [JsonProperty("indices", NullValueHandling = NullValueHandling.Ignore)]
    public int? Indices;

    [JsonProperty("material", NullValueHandling = NullValueHandling.Ignore)]
    public int? Material;
}