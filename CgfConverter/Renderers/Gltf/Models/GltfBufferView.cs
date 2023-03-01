using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfBufferView
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    [JsonProperty("buffer")] public int Buffer;

    [JsonProperty("byteLength")] public long ByteLength;

    [JsonProperty("byteOffset")] public long ByteOffset;

    [JsonIgnore] public long ByteOffsetTo => ByteLength + ByteOffset;
}