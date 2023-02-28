using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfTextureInfo
{
    [JsonProperty("index")]
    public int Index;

    [JsonProperty("texCoord", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int TexCoord;
}