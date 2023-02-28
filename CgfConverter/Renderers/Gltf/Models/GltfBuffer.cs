using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfBuffer
{
    [JsonProperty("byteLength")] public long ByteLength;

    [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
    public string? Uri;
}