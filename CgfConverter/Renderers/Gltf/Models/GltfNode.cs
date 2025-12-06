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

    /// <summary>
    /// 4x4 transformation matrix in column-major order.
    /// When present, rotation/scale/translation are ignored (per glTF spec).
    /// </summary>
    [JsonProperty("matrix", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Matrix;

    [JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Rotation;

    [JsonProperty("scale", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Scale;

    [JsonProperty("translation", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Translation;

    public bool ShouldSerializeChildren() => Children.Any();
}