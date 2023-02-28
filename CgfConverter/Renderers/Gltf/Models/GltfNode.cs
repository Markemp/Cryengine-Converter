using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfNode
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    [JsonProperty("mesh", NullValueHandling = NullValueHandling.Ignore)]
    public int? Mesh;

    [JsonProperty("skin", NullValueHandling = NullValueHandling.Ignore)]
    public int? Skin;

    [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
    public List<int> Children = new();

    [JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Rotation;

    [JsonProperty("scale", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Scale;

    [JsonProperty("translation", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Translation;

    public bool ShouldSerializeChildren() => Children.Any();
}