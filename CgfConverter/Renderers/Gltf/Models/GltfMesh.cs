using System.Collections.Generic;
using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfMesh
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    [JsonProperty("primitives")] public List<GltfMeshPrimitive> Primitives = new();
}