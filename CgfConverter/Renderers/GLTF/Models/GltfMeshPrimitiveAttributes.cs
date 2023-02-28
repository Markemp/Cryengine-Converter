using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfMeshPrimitiveAttributes
{
    [JsonProperty("POSITION", NullValueHandling = NullValueHandling.Ignore)]
    public int? Position;

    [JsonProperty("NORMAL", NullValueHandling = NullValueHandling.Ignore)]
    public int? Normal;

    [JsonProperty("TANGENT", NullValueHandling = NullValueHandling.Ignore)]
    public int? Tangent;

    [JsonProperty("TEXCOORD_0", NullValueHandling = NullValueHandling.Ignore)]
    public int? TexCoord0;

    [JsonProperty("COLOR_0", NullValueHandling = NullValueHandling.Ignore)]
    public int? Color0;

    [JsonProperty("JOINTS_0", NullValueHandling = NullValueHandling.Ignore)]
    public int? Joints0;

    [JsonProperty("WEIGHTS_0", NullValueHandling = NullValueHandling.Ignore)]
    public int? Weights0;
}